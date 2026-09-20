# Dataverse MCP — registering a custom client (delegated auth)

The Copilot Studio build (lab 02) doesn't need this: the GitHub Copilot harness handles MCP
authorization interactively, per-agent, the first time you open that agent's own Preview tab and it
prompts for consent.

The Foundry/pro-code build (lab 03) still needs its own registered Entra app to call the Dataverse
MCP server's remote endpoint (`/api/mcp`) — but **not** app-only/client-credentials auth. That was
the original design here and it's wrong: confirmed 2026-09-20 by inspecting the Dataverse resource
app directly in Entra (`az ad sp show --id 00000007-0000-0000-c000-000000000000`) — it exposes
**zero** application-permission app roles, and `mcp.tools` exists only as a **delegated** OAuth2
permission scope (`"type": "User"`). Entra's client-credentials grant can only carry application
permissions, so a service principal can never present `mcp.tools`, however the app registration or
a Dataverse Application User is configured. `DataverseMcpClient.cs` uses delegated (device-code)
auth instead, mirroring `WorkIqCalendarMcpClient.cs` — the signed-in user's own Dataverse privileges
govern what the calls can do, same as Work IQ already worked for calendar access.

This is a **shorter** setup than the app-only version would have needed — no client secret, no
separate Dataverse Application User.

> **Verify the exact click-path yourself before the workshop.** Power Platform Admin Center's MCP
> client screens are a recently-GA surface and menu wording moves. The *concept* — register a public
> client app → grant the delegated `mcp.tools` permission → allow-list its client ID in PPAC — is
> what to hold onto if a label below doesn't match what you see.

## 1. Register an Entra ID app as a public client

Entra admin center → **App registrations** → **New registration**. Single-tenant is enough for a
workshop reference build. Note the **Application (client) ID** and **Directory (tenant) ID** — these
become `Dataverse:ClientId` / `Dataverse:TenantId`.

Then, on the app registration → **Authentication** → under **Advanced settings**, set **Allow public
client flows** to **Yes**. Device-code auth (what `DataverseMcpClient.cs` uses) doesn't present a
client secret, so the app has to be registered as a public client or the token request is rejected.

No client secret is needed — skip **Certificates & secrets** entirely for this app.

## 2. Add the `mcp.tools` delegated permission

On the app registration → **API permissions** → **Add a permission** → **Microsoft APIs** →
**Dynamics CRM** → **Delegated permissions** → select **`mcp.tools`** → **Add permissions**.
Granting tenant admin consent here is recommended (avoids a per-user consent prompt showing up mid-demo)
but isn't strictly required if user consent is allowed in your tenant — the first `dotnet run` will
show a device-code sign-in prompt either way; the consent screen appears alongside it the first time.

## 3. Allow-list the app in Power Platform Admin Center

Dataverse MCP doesn't accept any Entra app with the right permission by default — the environment
also has to explicitly allow it as an MCP client. In [Power Platform admin
center](https://admin.powerplatform.microsoft.com/) → **Manage** → **Environments** → your
environment → **Settings** → **Product** → **Features** → **Dataverse Model Context Protocol** →
**Advanced Settings** → add a new client entry with this app's **Client ID**, set **Is Enabled** to
**Yes**, **Save & Close**.

## Fast sanity check

Once all three steps are done, run the app and confirm the device-code sign-in prompt appears and
completes:

```bash
dotnet run -- --scenario happy-path-last-week
```

If it hangs or fails immediately without ever printing a device-code URL/prompt, the app
registration itself (steps 1–2) is the first thing to recheck. If the sign-in completes but the
first Dataverse MCP call fails with an access-denied (not a consent-shaped) error, that's the PPAC
allow-list (step 3) — or the signed-in user's own Dataverse security role not covering
`cre_timeentry`/`cre_project`, which is a normal Dataverse privilege problem, not an MCP-specific one.
