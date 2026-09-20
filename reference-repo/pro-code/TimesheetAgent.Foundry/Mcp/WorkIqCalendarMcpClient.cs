using Azure.Identity;
using ModelContextProtocol.Client;

namespace TimesheetAgent.Foundry.Mcp;

/// <summary>
/// Connects to the Work IQ MCP server (preview). Unlike Dataverse MCP, Work IQ acts on behalf of
/// a signed-in M365 Copilot-licensed user — delegated auth, not app-only — because calendar data
/// is inherently user-specific. Uses device-code auth so a headless/hosted Foundry agent can still
/// complete an interactive sign-in once and cache the resulting token.
///
/// IMPORTANT — the raw Work IQ MCP surface is NOT per-workload named tools. It exposes a small
/// fixed set of generic tools (fetch, create_entity, do_action, call_function, ask) that operate
/// on resource paths, e.g. `fetch` on path `/me/events` for a calendar read. That is different
/// from the friendlier named "Work IQ Calendar" tools Copilot Studio's authoring UI shows — those
/// are a Copilot-Studio-specific presentation layer, not the underlying protocol. Callers of this
/// client must filter to calendar-shaped paths (`/me/events`) themselves; the server will just as
/// happily hand back mail or Teams data if asked.
///
/// Requires a custom Entra app registration with the WorkIQAgent.Ask delegated permission
/// (resource app ea fdcc1f02-fc51-4226-8753-f668596af7f7, "Work IQ") and tenant admin consent —
/// see docs/dataverse-mcp-custom-client.md's sibling setup for Work IQ, and
/// https://learn.microsoft.com/microsoft-365/copilot/extensibility/work-iq/a2a/quickstart.
/// Confirmed 2026-09-20: without an explicit ClientId here, DeviceCodeCredential silently falls
/// back to Azure Identity's generic "Azure development application" (client ID
/// 04b07795-8ddb-461a-bbee-02f9e1bf7b46), which Work IQ's resource explicitly rejects with
/// AADSTS65002 ("must be configured via preauthorization") — Work IQ only accepts apps the tenant
/// has explicitly registered and consented, never an arbitrary/default client.
/// </summary>
public static class WorkIqCalendarMcpClient
{
    private const string WorkIqEndpoint = "https://workiq.svc.cloud.microsoft/mcp";

    public static async Task<McpClient> ConnectAsync(
        string tenantId,
        string clientId,
        CancellationToken cancellationToken = default)
    {
        var credential = new DeviceCodeCredential(new DeviceCodeCredentialOptions
        {
            TenantId = tenantId,
            ClientId = clientId,
            TokenCachePersistenceOptions = new TokenCachePersistenceOptions { Name = "TimesheetAgent.WorkIq" },
            DeviceCodeCallback = (info, ct) =>
            {
                Console.WriteLine(info.Message);
                return Task.CompletedTask;
            },
        });

        // Fail fast with a clear device-code prompt if no cached/refreshable token exists yet,
        // rather than surfacing that failure later as an opaque "Calendar agent is failing" during
        // the first real tool call. See BearerTokenHandler for why this is only acquired once here
        // (a fast-fail check) and re-acquired per request everywhere else.
        await credential.GetTokenAsync(
            new Azure.Core.TokenRequestContext(new[] { $"{WorkIqEndpoint}/.default" }), cancellationToken);

        var httpClient = new HttpClient(new BearerTokenHandler(credential, $"{WorkIqEndpoint}/.default"));
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions { Endpoint = new Uri(WorkIqEndpoint) },
            httpClient,
            loggerFactory: null!,
            ownsHttpClient: true);

        return await McpClient.CreateAsync(transport, cancellationToken: cancellationToken);
    }
}
