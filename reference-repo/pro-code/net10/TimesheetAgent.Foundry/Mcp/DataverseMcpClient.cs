using Azure.Identity;
using ModelContextProtocol.Client;

namespace TimesheetAgent.Foundry.Mcp;

/// <summary>
/// Connects to the Dataverse MCP server as a registered custom client, using delegated
/// (device-code) auth — mirroring <see cref="WorkIqCalendarMcpClient"/>. Confirmed 2026-09-20:
/// app-only/client-credentials auth is NOT viable here. The Dataverse resource exposes zero
/// application-permission app roles; `mcp.tools` exists only as a delegated (`"type": "User"`)
/// OAuth2 permission scope, and Entra's client-credentials grant can only carry application
/// permissions — so a service principal can never present `mcp.tools`, no matter how the app
/// registration or Dataverse Application User is configured. An earlier version of this client
/// assumed app-only per-agent identity; that assumption was wrong and unverified. See
/// docs/dataverse-mcp-custom-client.md for the (simpler, delegated) setup this now requires.
/// </summary>
public static class DataverseMcpClient
{
    public static async Task<McpClient> ConnectAsync(
        string environmentUrl,
        string tenantId,
        string clientId,
        CancellationToken cancellationToken = default)
    {
        var credential = new DeviceCodeCredential(new DeviceCodeCredentialOptions
        {
            TenantId = tenantId,
            ClientId = clientId,
            TokenCachePersistenceOptions = new TokenCachePersistenceOptions { Name = "TimesheetAgent.Dataverse" },
            DeviceCodeCallback = (info, ct) =>
            {
                Console.WriteLine(info.Message);
                return Task.CompletedTask;
            },
        });

        // Dataverse's own resource scope — the Web API's well-known application ID URI. The
        // signed-in user's own security-role privileges (not an Application User's) govern what
        // the resulting calls can actually do.
        var scope = $"{environmentUrl.TrimEnd('/')}/.default";

        // Fail fast with a clear device-code prompt if no cached/refreshable token exists yet — see
        // BearerTokenHandler for why every subsequent request re-acquires its own token instead of
        // reusing this one. Without this fix, a long conversation's later Dataverse writes silently
        // started failing once the token acquired here expired (found live, 2026-09-20 — same root
        // cause as the Work IQ calendar failures, just not yet observed on this client in testing).
        await credential.GetTokenAsync(new Azure.Core.TokenRequestContext(new[] { scope }), cancellationToken);

        var httpClient = new HttpClient(new BearerTokenHandler(credential, scope));
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions { Endpoint = new Uri($"{environmentUrl.TrimEnd('/')}/api/mcp") },
            httpClient,
            loggerFactory: null!,
            ownsHttpClient: true);

        return await McpClient.CreateAsync(transport, cancellationToken: cancellationToken);
    }
}
