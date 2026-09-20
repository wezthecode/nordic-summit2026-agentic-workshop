# Lab 02 — Copilot Studio build (low-code path)

**Time budget:** 60 minutes (module 3)
**Prereq:** Lab 01 complete — solution + tables + role + MDA live in your dev environment
**Authoritative prompts:** [`prompts/`](prompts/) — copy these verbatim
**Harness:** **GitHub Copilot harness** (the "new experience"). Not the standard harness. This matters — see the callout below.

## Goal

Build the multi-agent timesheet system entirely in Copilot Studio:

- **Orchestrator (primary) agent** — talks to the user, plans the work, never calls MCPs itself
- **Calendar agent** — wraps the **Work IQ Calendar MCP** (first-party preview)
- **Time-entry agent** — wraps the **Dataverse MCP** (first-party GA)

The two specialists are wired to the orchestrator as **connected agents**.

> **Read this before you start.** On the GitHub Copilot harness there are **no child agents and no topics.** The only multi-agent primitive is the connected agent, and routing to it is decided by the orchestration runtime from the agent's **name and description** — there is no trigger to wire and no typed input/output contract. Two consequences for this lab:
> 1. **Descriptions are load-bearing.** A vague description is the #1 cause of "it didn't delegate." Treat the description field as code.
> 2. **You must publish the two specialists before you can connect them.** The connect dialog only lists published agents. Build order below reflects this — don't skip ahead.
>
> Also note: MCP tool support on this harness shipped **July 2026** and is still **preview**. If it misbehaves, that's the preview, not you.

End state: you log into the published agent, say "log my time for last week", it asks you to confirm a draft, you say yes, rows land in `cre_timeentry` with `cre_source = Copilot Studio agent`.

## Why this exists

This is the low-code half of the comparison. Lab 03 rebuilds the same thing with the .NET Agent Framework SDK on Foundry. Lab 04 puts them side by side. The prompts in `prompts/` are the **identical text** used by both builds — that's the whole point of the comparison.

## Steps

> **Build order matters on this harness.** Specialists first, publish them, *then* the orchestrator. If you build the orchestrator first you'll have nothing to connect to it.

### 1. Create the calendar agent

In your dev environment: Copilot Studio home → create a new agent on the **new experience** (GitHub Copilot harness). If your home page shows a **New experience** toggle, leave it **on**; "Other ways to build" is the standard harness — not this lab.

On the **Build** tab:

- **Name:** `Calendar Agent`
- **Description:** "Reads the signed-in user's Outlook calendar and returns meetings for a given date range as JSON. Use for any request about what meetings or events happened or are scheduled." — *this text is the routing contract; don't shorten it*
- **Instructions:** paste [`prompts/calendar-child.system.md`](prompts/calendar-child.system.md) verbatim
- **Tools:** add the **Work IQ Calendar MCP** (Tools → Add → Model Context Protocol). Authorise with your M365 Copilot-licensed account.

> **If Work IQ MCP isn't available in your tenant:** you're missing the M365 Copilot licence. Use the shared sandbox creds from the prep envelope. Do not skip — there is no fallback specialist for this lab.

> **Test this agent alone before moving on.** Open this agent's own **Preview** tab (not the
> orchestrator's) and ask it something calendar-shaped, e.g. "What's on my calendar this week?".
> MCP tool consent on this harness is granted **per agent, in that agent's own Preview** — it does
> not carry over when the agent is later connected to the orchestrator. If you skip this and go
> straight to wiring up connected agents in step 3, the first sign of trouble is the orchestrator
> reporting a vague failure or a "consent declined" error with no obvious source — confirmed the
> hard way in testing on 2026-09-19/20. Granting consent here, once, avoids that entirely.

**Publish it.** Build tab → Publish. An unpublished agent won't appear in the connect dialog in step 3.

### 2. Create the time-entry agent

Same creation path, same harness.

- **Name:** `Time Entry Agent`
- **Description:** "Writes time entry rows to Dataverse from a structured list of entries, and reports which succeeded or were rejected. Use for any request to log, save, or record hours." — again, load-bearing
- **Instructions:** paste [`prompts/time-entry-child.system.md`](prompts/time-entry-child.system.md) verbatim, **substituting the `{{SOURCE_VALUE}}` placeholder with `100000001`** — the *numeric* value for "Copilot Studio agent" in this environment, not the label text. `cre_source` is a Dataverse choice column under the hood; passing the string instead of the number causes the write to fail (confirmed directly in testing on 2026-09-19). Values are environment-specific — if yours differ, check `EntityDefinitions(LogicalName='cre_timeentry')/Attributes(LogicalName='cre_source')/Microsoft.Dynamics.CRM.PicklistAttributeMetadata?$expand=OptionSet` via the Dataverse Web API, or just ask the agent to `describe cre_timeentry` and read the option set back.
- **Tools:** add the **Dataverse MCP** scoped to your dev environment. Turn off **Allow all** and enable only the tools you need (`create_record`, `read_query`, `describe`) — narrower surface, fewer ways for the agent to wander.

> **Test this agent alone before moving on** — same reason as the Calendar agent above. Open its
> own **Preview** tab and ask it to read the active project list (`read_query` on `cre_project`).
> Grant MCP consent here; it won't carry over from the Calendar agent's consent or apply
> automatically once this agent is connected to the orchestrator.

**Publish it.**

### 3. Create the orchestrator and connect the specialists

- **Name:** `Timesheet Orchestrator`
- **Description:** "Logs time entries to Dataverse based on calendar events."
- **Instructions:** paste [`prompts/orchestrator.system.md`](prompts/orchestrator.system.md) verbatim
- **Knowledge:** none
- **Tools:** none — the orchestrator never calls an MCP itself; it delegates

Then, on the **Build** tab → **Connected agents** → add both specialists. For each one, review the description that carries over and sharpen it if it overlaps with the other. Confirm both appear in the components panel.

> There is no "generative orchestration" toggle on this harness — enhanced orchestration is always on and isn't configurable. If you're looking for that switch, you're on the wrong harness.

### 4. Test in the Preview tab

Open the **Preview** tab on the orchestrator, and open the **activity trace** alongside it. Try:

> "Log my time for last Tuesday."

Expected flow (watch it in the trace, not just the chat):
1. Orchestrator routes to Calendar Agent for events
2. Calendar Agent returns the JSON event array
3. Orchestrator drafts entries with project mappings
4. Orchestrator shows the draft and asks for confirmation
5. On "yes", Time Entry Agent writes to Dataverse
6. Open the `Weekly Timesheet` MDA — rows appear, `cre_source = Copilot Studio agent`

> **Note what the trace shows you:** which agent was invoked, the arguments passed, and what came back. On this harness that trace *is* your debugging story — there's no explicit trigger to inspect. Get comfortable reading it here; lab 04 compares it against Foundry's tracing.

### 5. Run two scenarios from `scenarios.json`

Pick `happy-path-last-week` and `duplicate-event-rejection` from [`eval/scenarios.json`](../Completed_Labs/eval/scenarios.json). Manually drive them through the Preview tab. Confirm:

- Happy path writes 3 rows
- Duplicate path surfaces the alternate-key violation **to you** — agent doesn't retry silently, doesn't paper over

This is the manual rehearsal for the automated eval in lab 05.

### 6. Publish to a channel

Publish to the **Teams** channel (it's the fastest path; web channel is fine if Teams admin consent is blocked). Pin the published agent to your taskbar — you'll demo from this in lab 04.

## You will leave this lab with

- Three published agents wired together
- A handful of rows in `cre_timeentry` tagged `Copilot Studio agent`
- Manual confirmation that two of the eval scenarios behave correctly

## Common failures

| Symptom | Cause | Fix |
|---------|-------|-----|
| Specialist agents don't appear in the connect dialog | They aren't published, or aren't shared with you | Publish both specialists; confirm you own them or they're shared with you |
| Orchestrator answers the user directly instead of delegating | Connected agent descriptions too vague, or instructions missing the "never call MCPs directly" line | Sharpen the connected agent **descriptions** first — that's the routing contract on this harness — then re-paste the orchestrator prompt |
| Orchestrator routes to the *wrong* specialist | Overlapping descriptions | Make the two descriptions mutually exclusive; name the domain explicitly in each |
| Calendar Agent returns natural language instead of JSON | Prompt drift | Re-paste the calendar-child prompt verbatim |
| Time Entry Agent writes `cre_source = Manual` | Forgot to substitute `{{SOURCE_VALUE}}` | Fix the prompt |
| `create_record` fails with a FormatException / "cannot convert" on `cre_source` | Substituted the *label* ("Copilot Studio agent") instead of the *number* (`100000001`) | Use the numeric value — see the instructions above |
| Same error on `cre_calendareventid` when there's no real event | An explicit `null` was passed instead of omitting the field | Tell the agent to omit optional fields entirely, not pass `null` |
| Agent says the project lookup field doesn't exist | Prompt drift back to `cre_project` instead of `cre_projectid` | Re-paste the prompt verbatim — the lookup field is `cre_projectid`, the table is `cre_project` |
| Duplicate test "succeeds" (no error surfaced) | Time Entry Agent wrapped the error and pretended it was fine | Re-paste the prompt; this is the exact failure mode the prompt is written to prevent |
| Dataverse MCP shows hundreds of tools | **Allow all** left on | Edit the MCP tool, turn off **Allow all**, enable only `create_record` / `read_query` / `describe` |
| Looking for the "generative orchestration" toggle | You're thinking of the standard harness | It doesn't exist here — enhanced orchestration is always on |

## Done?

Move to [Lab 03 - Foundry Build](Lab%2003%20-%20Foundry%20Build.md).
