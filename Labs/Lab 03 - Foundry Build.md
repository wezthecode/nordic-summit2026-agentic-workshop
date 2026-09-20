# Lab 03 — Microsoft Foundry build (pro-code path)

**Time budget:** 60 minutes (module 4)
**Prereq:** Lab 01 complete (shared data layer), Lab 02 complete (you've seen what the agents are *supposed* to do), and the [Dataverse MCP custom-client registration](../Completed_Labs/pro-code/dataverse-mcp-custom-client.md) done once for your environment — the delegated (device-code) auth this build uses won't get past the first MCP call without it.
**Authoritative prompts:** [`prompts/`](prompts/) — same files, used verbatim
**Reference repo:** this same repo — `Completed_Labs/pro-code/net10/TimesheetAgent.Foundry/`. This lab was originally advertised on .NET 8; identical code targeting that framework is preserved at `Completed_Labs/pro-code/net8/TimesheetAgent.Foundry/` if you specifically need it, but .NET 10 (the current LTS) is what's actively maintained and what these steps assume.

## Goal

Rebuild the exact same orchestrator + 2 specialist agents from lab 02, but with the **.NET 10 Microsoft Agent Framework SDK**, calling the same **Foundry-hosted model** and the same two first-party MCPs. End state:

- Three agents defined in C#, sharing the same system prompts as lab 02 (loaded from `Labs/prompts/`)
- Same two MCPs — Work IQ Calendar (preview), Dataverse (GA) — wired via the SDK's MCP client
- Run as a **local console app** — `dotnet run` on your machine, authenticating straight to the Foundry model's OpenAI-compatible endpoint. No Azure hosting step required to use it.
- Rows land in `cre_timeentry` with `cre_source = Foundry agent`

> **On "deploying to Foundry":** that phrase means something specific — publishing this as a *Foundry-hosted agent resource* (its own hosted runtime, invoked over REST), which is a different hosting model from what's built here. This lab doesn't do that; it's optional, unverified, stretch material — see **Optional: previewing the hosted-Foundry / web-UI path** at the end. The lab's actual deliverable is the console app talking to live MCPs and live Dataverse, same as lab 02's agents do.

## Why this exists

This is the pro-code half. Same MCPs, same prompts, same Dataverse schema — different runtime. Lab 04 reads off the differences.

## Steps

### 1. Clone the repo

```bash
git clone https://github.com/wezthecode/nordic-summit2026-agentic-workshop.git
cd nordic-summit2026-agentic-workshop/Completed_Labs/pro-code/net10/TimesheetAgent.Foundry
```

```
TimesheetAgent.Foundry/
├── Program.cs
├── Agents/
│   ├── OrchestratorAgent.cs
│   ├── CalendarChildAgent.cs
│   └── TimeEntryChildAgent.cs
├── Mcp/
│   ├── WorkIqCalendarMcpClient.cs
│   └── DataverseMcpClient.cs
├── Foundry/                    ← optional stretch material, see end of this lab
│   ├── agent.yaml
│   └── README.md
└── TimesheetAgent.Foundry.csproj
```

### 2. Configure local secrets

```bash
dotnet user-secrets init
dotnet user-secrets set "Foundry:Endpoint" "https://<your-foundry-resource>.openai.azure.com/"
dotnet user-secrets set "Foundry:DeploymentName" "<your model deployment name>"
dotnet user-secrets set "Dataverse:EnvironmentUrl" "https://<your-env>.crm.dynamics.com"
dotnet user-secrets set "Dataverse:TenantId" "<tenant guid>"
dotnet user-secrets set "Dataverse:ClientId" "<app registration client id — from the custom-client setup>"
```

No client secret — the Dataverse MCP connection uses **delegated** (device-code) auth, not app-only
(confirmed 2026-09-20: app-only can't work here at all, see the custom-client doc). The first
`dotnet run` will print a device-code URL/prompt; sign in with an account that has Dataverse access
to the target environment. `WorkIq:TenantId` is optional — it defaults to `Dataverse:TenantId` if
unset (`Program.cs`). The Foundry model call uses `DefaultAzureCredential` — it picks up your `az
login` session directly, no app registration needed for that part.

> **There's no `Source:Value` config key.** The `cre_source` numeric value (`100000002` = "Foundry
> agent" in the reference environment) is hardcoded directly in `TimeEntryChildAgent.cs`, not read
> from configuration — it's baked into the `{{SOURCE_VALUE}}` substitution at agent-build time.
> Re-verify that number against your own environment's option set before relying on it; Dataverse
> generates these at solution build time and they aren't guaranteed portable across environments.

### 3. Wire prompts from `Labs/prompts/`

Open `Agents/OrchestratorAgent.cs`. The prompt is loaded from a relative path to
`Labs/prompts/orchestrator.system.md` at startup. **Do not copy-paste prompts
into source**, and **do not edit them here** — if you tune a prompt, tune it in `Labs/prompts/` so
lab 02 stays in sync.

Same for `CalendarChildAgent.cs` and `TimeEntryChildAgent.cs`.

### 4. Run it

```bash
dotnet run
```

With no arguments this sends "Log my time for last week." to the orchestrator and prints the
response. To run a specific scenario instead:

```bash
dotnet run -- --scenario happy-path-last-week
```

This loads a named scenario from `Completed_Labs/eval/scenarios.json`, runs it against the in-process
agent — talking to live MCPs and live Dataverse, same as lab 02 — and prints the orchestrator's
response. Confirm:

- The expected rows were written (check the count against the scenario)
- All tagged `cre_source = Foundry agent`
- The MDA in your browser shows them

This **is** the lab's deliverable — there's no separate deploy step required to call it done.

### 5. Run the same two manual scenarios as lab 02

`happy-path-last-week` and `duplicate-event-rejection`, via `dotnet run -- --scenario <name>`. Same
expectations as lab 02. **The whole point** is that the conversation should feel almost identical —
the prompts are the same, the MCPs are the same, the data layer is the same. What's different is
hosting, identity, observability, and how you ship changes — that comparison is lab 04.

## You will leave this lab with

- A working console app that runs the same three-agent conversation as lab 02, against the same live data
- Rows in `cre_timeentry` tagged `Foundry agent` for the two scenarios you ran
- A working pro-code baseline for the lab 04 comparison

## Common failures

| Symptom | Cause | Fix |
|---------|-------|-----|
| `dotnet user-secrets set` fails / app throws on startup looking for secrets | Missing `<UserSecretsId>` in the csproj | Should already be set in the reference repo; if you forked the project, add one (`dotnet user-secrets init` generates it) |
| Local run errors on Work IQ MCP connect | M365 Copilot licence missing on the local-dev account | Use shared sandbox creds |
| App never prints a device-code URL, or the device-code sign-in fails outright | App registration isn't a public client, or `mcp.tools` delegated permission missing | See steps 1–2 of the [custom-client doc](../Completed_Labs/pro-code/dataverse-mcp-custom-client.md) |
| Sign-in completes but the Dataverse MCP call fails with access-denied | App registration isn't allow-listed in PPAC | See step 3 of the [custom-client doc](../Completed_Labs/pro-code/dataverse-mcp-custom-client.md) |
| Sign-in completes, PPAC allow-list is fine, but `create_record`/`read_query` still fails on privileges | The signed-in user's own Dataverse security role doesn't cover `cre_timeentry`/`cre_project` — this is a normal Dataverse privilege problem, not MCP-specific | Check the signed-in user's security role in the target environment |
| Rows tagged `Manual` not `Foundry agent` | Shouldn't happen — the value is hardcoded, not config — but if it does, check `TimeEntryChildAgent.cs`'s `{{SOURCE_VALUE}}` substitution wasn't reverted | Re-check the source in `Agents/TimeEntryChildAgent.cs` |
| Duplicate test "succeeds" | Code wraps `MCP write failed` into a soft message | The MCP client in `Mcp/DataverseMcpClient.cs` is written to throw on duplicate-key. If you don't see the exception, you've caught it somewhere upstream. |
| `dotnet build --configuration Release` fails with a pile of analyzer errors unrelated to this project | A `Directory.Build.props` higher up your directory tree (e.g. a personal/day-job one) is leaking into this repo | This repo ships its own `Completed_Labs/pro-code/Directory.Build.props` to stop MSBuild's upward search at that boundary — confirm it's present; if you moved the project outside `Completed_Labs/pro-code/`, you'll need to add an equivalent boundary file yourself |

## Optional: previewing the hosted-Foundry / web-UI path

Not part of the required lab — a short, honest preview of what's *next* if you wanted to go further:

- **A web UI in front of this agent** doesn't need Azure deployment either. Microsoft Agent
  Framework's hosting docs show wrapping the same agent objects in a thin ASP.NET Core layer
  (`builder.AddAIAgent(...)` + `app.MapOpenAIResponses(...)`) to expose an OpenAI-compatible HTTP
  endpoint a frontend can call — or the **AG-UI** protocol integration, built specifically for
  web-based agent UIs. Same agents, same MCPs, just a different front door.
- **Publishing to Teams / Microsoft 365 Copilot** is a bigger step, not a flag flip: it means either
  redeploying as an actual **Foundry Hosted Agent** (Foundry portal's one-click "Publish to
  Microsoft 365 and Teams" — but that changes the hosting model away from this in-process console
  app) or bridging this same orchestration with the **Microsoft 365 Agents SDK**, which requires a
  registered Azure Bot Service resource with a public HTTPS endpoint. Neither is "free" — good
  material for the comparison talking points, not something to live-demo.
- `Foundry/agent.yaml` and `Foundry/README.md` in this project are a **sketch, not a tested
  deployment** — written against `az foundry agent deploy`, a command surface that was never
  verified to exist as described. They're useful to *show* as "here's the shape it would take,"
  explicitly labelled as unverified if you reference them live.

## Done?

Move to [Lab 04 - Governance](Lab%2004%20-%20Governance.md).
