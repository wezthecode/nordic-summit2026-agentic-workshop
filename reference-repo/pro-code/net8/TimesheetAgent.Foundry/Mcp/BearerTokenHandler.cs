using System.Net.Http.Headers;
using Azure.Core;

namespace TimesheetAgent.Foundry.Mcp;

/// <summary>
/// Acquires a fresh access token from <paramref name="credential"/> on every outgoing request,
/// instead of baking a single token into a static header at connection time.
///
/// Found 2026-09-20, live: both MCP clients originally called <c>credential.GetTokenAsync(...)</c>
/// once when the client was constructed, then passed the resulting string into
/// <c>HttpClientTransportOptions.AdditionalHeaders["Authorization"]</c> — a static value baked in
/// at connect time. The <see cref="McpClient"/> built from that transport is reused for the whole
/// agent's lifetime, potentially across a long multi-turn conversation, so once that one token
/// expired every subsequent call failed with no retry or refresh. This is exactly what happened
/// live: a calendar read succeeded once, then failed on every attempt a few minutes later with no
/// clear error surfaced to the user ("the Calendar agent is failing").
///
/// This handler fixes it by calling <c>GetTokenAsync</c> per request instead of once.
/// Azure.Identity's own credential implementations (including <c>DeviceCodeCredential</c> with
/// <c>TokenCachePersistenceOptions</c> set) cache internally and only re-prompt interactively when
/// a cached token and refresh token are both unusable — so this does not mean a new device-code
/// sign-in on every call, only a cheap silent cache check (or silent refresh) per request.
/// </summary>
public sealed class BearerTokenHandler : DelegatingHandler
{
    private readonly TokenCredential credential;
    private readonly string scope;

    public BearerTokenHandler(TokenCredential credential, string scope)
        : base(new HttpClientHandler())
    {
        this.credential = credential;
        this.scope = scope;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await credential.GetTokenAsync(
            new TokenRequestContext(new[] { scope }), cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        return await base.SendAsync(request, cancellationToken);
    }
}
