# How the Foundry pro-code build actually came together — and why

**Written 2026-09-20, the night before the workshop, immediately after the first fully verified
end-to-end run.** This is the real build history, not a cleaned-up version — every bend in this
path is either genuine platform behaviour worth knowing, or a genuine bug this project shipped with
until tonight. Good material for module 4's demo-and-inspect narration and for the slide deck:
"pro-code doesn't remove platform complexity, it relocates where you encounter it" is the thread
running through almost all of it.

## Starting point

`TimesheetAgent.Foundry` compiled and had never been run against live services. The design on paper:
three Microsoft Agent Framework agents (orchestrator, calendar, time-entry) built in-process in a C#
console app, talking to the same two first-party MCPs as the Copilot Studio build (Work IQ Calendar,
Dataverse), backed by a Foundry-hosted model. Simple, in theory.

## 1. The auth design was wrong, and the code even said so

`DataverseMcpClient.cs` used `ClientSecretCredential` — app-only, client-credentials auth — on the
theory that each agent should act under its own service-principal identity, mirroring the
per-agent-identity story used elsewhere in the project.

Checking this **before** building the Entra app registration (rather than after) found the real
problem: the Dataverse resource in Entra exposes **zero application-permission app roles** —
`mcp.tools` exists only as a delegated (`"type": "User"`) OAuth2 permission scope. Entra's
client-credentials grant can only carry application permissions. A service principal can never
present `mcp.tools`, no matter how the app registration or a Dataverse Application User is
configured. This is a platform fact, not a config mistake.

Fix: `DataverseMcpClient.cs` switched to `DeviceCodeCredential` (delegated), mirroring
`WorkIqCalendarMcpClient.cs`, which had always used delegated auth for the obvious reason — calendar
data is inherently user-specific. This also **simplified** the setup: no client secret, no separate
Dataverse Application User. Just a public-client app registration, the delegated permission, tenant
admin consent, and the Dataverse MCP's PPAC allow-list entry. See
[`dataverse-mcp-custom-client.md`](dataverse-mcp-custom-client.md) for the concrete steps.

## 2. Work IQ needed enabling in the tenant from zero, in three real layers

Registering the Work IQ custom client (same shape: public client, `WorkIQAgent.Ask` delegated
permission, admin consent) hit `AADSTS65002` — Azure CLI's own generic fallback client
(`04b07795-...`, used automatically by `DeviceCodeCredential` whenever `ClientId` isn't set — a bug
in `WorkIqCalendarMcpClient.cs` at the time, since fixed) isn't preauthorized for Work IQ's resource.

Registering a proper custom app and retrying consent surfaced the real, deeper blocker: **the GPG
tenant had never been enabled for Work IQ at all.** Three separate, real prerequisites, none
optional:

1. **The Work IQ service principal didn't exist in the tenant.** A one-time, free, purely technical
   step — `az ad sp create --id fdcc1f02-fc51-4226-8753-f668596af7f7` — but a blocking one; without
   it, Work IQ doesn't even appear as an option to grant permissions against.
2. **A usage-based Copilot Credits billing policy had to actually cover Work IQ API**, not just
   exist for Cowork. Confirmed empirically — provisioning the SP alone still failed the consent
   grant with "your organization has not subscribed to" until the billing policy's covered-services
   list explicitly included Work IQ API. Turned out this was already configured in GPG's tenant, so
   this step cost a screenshot-check, not new setup — but it is a real, separate gate, and would have
   been a hard blocker in a tenant where it wasn't already there.
3. **Group membership, not ownership.** The billing policy was scoped to a security group Wesley
   owned but wasn't a member of — Entra distinguishes the two, and only membership counts for policy
   scoping. A 30-second fix once noticed, easy to miss.

None of this is documented as one place in Microsoft's own docs — it's assembled from three
different doc pages (the MCP auth doc, the Work IQ A2A quickstart, and the usage-based billing docs)
plus one genuinely undocumented CLI schema quirk (see below).

## 3. Multi-model "flexibility" has a real asterisk

Attempted swapping the Foundry model from `gpt-5-mini` to `claude-sonnet-4-6`, on the same resource,
expecting a deployment-name config change. Two real, separate hurdles:

- Deploying any non-Microsoft model at all requires `ModelProviderData` (organisation name, country
  code, industry) — deploying a partner model via Foundry is Azure **accepting an Azure Marketplace
  subscription and the partner's commercial terms on the organisation's behalf.** Not exposed on the
  standard `az cognitiveservices account deployment create` CLI surface; a portal click-through
  precisely because it's a real legal/commercial acceptance, not a technical toggle.
- Once deployed, calling it failed outright: `HTTP 404 (api_not_supported)`. Claude models on Foundry
  speak the **native Anthropic Messages API**, at a structurally different endpoint
  (`/anthropic/v1/messages`), not the Azure OpenAI Chat Completions shape `AzureOpenAIClient` (the
  standard .NET client for OpenAI-family Foundry models) uses. Swapping providers isn't a config
  change — it's a different client library entirely.

Reverted to `gpt-5-mini` for the live demo. Full writeup, and why it's good hook-talk material, in
[`../../notes/multi-model-provider-lessons.md`](../../notes/multi-model-provider-lessons.md).

## 4. The real, live bug: a static token in a long-lived session

First full conversation: calendar read succeeded, a Dataverse write succeeded (correctly rejecting
one real duplicate), then a **second** calendar read — minutes later, same conversation — failed,
and retrying in the same session failed identically. Root cause: both MCP clients acquired an OAuth
token **once**, at connection time, and baked it into a static `Authorization` header on the
transport. The `McpClient` built from that transport is reused for the agent's entire lifetime; once
that one token needed a refresh mid-conversation, there was no mechanism to get one.

A fast, scripted repro (two calls seconds apart, both succeeded) ruled out a bad request shape and
confirmed the failure needed real elapsed time to surface — consistent with a token or session
needing to refresh mid-conversation with no refresh path wired up.

Fixed at two layers:

- **`BearerTokenHandler`** — a `DelegatingHandler` that acquires a fresh token from the credential on
  *every* outgoing request instead of once. Azure.Identity's own credential caching means this isn't
  a new sign-in per call, just a cheap silent cache/refresh check.
- **`ReconnectingMcpTool`** — wraps each MCP tool so a failed call reconnects the underlying
  `McpClient` fresh and retries once, as a second line of defence regardless of the exact failure
  mode. Safe even for `create_record`: the `cre_calendareventid` alternate key turns a retried
  already-succeeded write into an ordinary, correct duplicate-key rejection, never a double write.

## Where it landed

First fully verified end-to-end run, 2026-09-20: orchestrator → Work IQ calendar read → Dataverse
draft/confirm/write cycle → a real row confirmed directly against the Dataverse Web API (not just
trusted from the console output), including a **second** calendar call succeeding cleanly in the
same conversation. Token-cache persistence was also added for convenience, with one caveat: it
persists the token store correctly, but doesn't yet do the `AuthenticationRecord` serialize/deserialize
step needed for genuinely silent reuse *across separate process runs* — each fresh `dotnet run`
still prompts once. Documented, not yet fixed; low priority against everything else tonight.
