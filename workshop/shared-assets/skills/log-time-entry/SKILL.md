---
name: log-time-entry
description: Turn a list of calendar events into draft cre_timeentry rows, confirm them with the user, then write the confirmed rows to Dataverse via the Dataverse MCP. Use when asked to log, record, draft, confirm, or write time entries or hours.
---

# Log Time Entry

Draft Phase 2 skill for the Time Entry Agent (GitHub Copilot harness). **Not yet attached or tested
live** — see `notes/copilot-studio-phase2-skills.md`. Extracted from the already-validated
`shared-assets/prompts/time-entry-child.system.md` after a full day of real debugging on
2026-09-19/20 found and fixed three genuine bugs. **Every fix below was earned the hard way — do
not simplify this file back toward the original, shorter version those bugs came from.**

## When to use this skill

Anything about logging, recording, drafting, confirming, or writing time entries or hours. This is
the Time Entry Agent's entire job — if this skill is attached, the agent's base Instructions should
shrink to little more than "you are the Time-Entry agent, use the Log Time Entry skill for anything
about logging or writing hours" and this file carries the rest.

## Tool

Dataverse MCP (Microsoft-hosted, GA), scoped to only `create_record` and `read_query` (Allow all
off). No other tool is used; this skill never reads the calendar itself — events are supplied by
whoever calls this skill (the Get Calendar Entries skill, or the orchestrator relaying its output).

## Inputs

- `events` (required, list) — event objects matching the Get Calendar Entries skill's output shape
- `user_confirmed` (required, boolean) — `false` for "draft these for me", `true` for "now write them"
- `project_overrides` (optional, map of `event_id` → `project_code`) — user corrections from the
  previous turn

## Behaviour

### When `user_confirmed = false` (draft mode)

1. Read the active project list once via `read_query` on `cre_project` (filter:
   `cre_isactive == true`) — cache for the turn.
2. For each event, propose a project by matching the event subject or attendee domains against
   project names and codes. If there is no clear match, set project to `null` and flag the event
   for user clarification.
3. Apply `project_overrides` if present — they always win over your inference.
4. Build draft `cre_timeentry` rows. **Do not write yet.** Return the drafts as structured output.

### When `user_confirmed = true` (write mode)

For each draft, call `create_record` on `cre_timeentry` with:

- **`cre_name`** = `{date:yyyy-MM-dd} · {project code} · {hours}h` (for example
  `2026-09-19 · PROJ-A · 2h`). **This field is required and there is no auto-populate rule doing it
  for you** — you must set it explicitly, every time, in this exact format. Do not leave it to a
  default, do not truncate, do not use a single letter or fragment of the description. *(Found
  2026-09-20: omitting explicit guidance here caused the model to write single-letter garbage
  values like `"D"` — literally the first character of the description — to satisfy the required
  field. This instruction exists specifically to prevent that.)*
- **`cre_date`** = event date
- **`cre_hours`** = event `duration_hours`
- **`cre_projectid`** = lookup to the chosen project. **The field is `cre_projectid`, not
  `cre_project`** — Dataverse appends `id` to a lookup's schema name. *(Found 2026-09-19: the
  original prompt said `cre_project`; the agent had to self-correct by calling `describe
  cre_timeentry` mid-conversation to find the real name. If `create_record` rejects `cre_projectid`,
  run `describe cre_timeentry` to confirm the actual lookup field name in this environment before
  retrying.)*
- **`cre_description`** = event subject
- **`cre_source`** = a **numeric choice value, never a label.** In the reference environment:
  `100000000` = Manual, `100000001` = Copilot Studio agent, `100000002` = Foundry agent. *(Found
  2026-09-19: passing the string label instead of the number caused a FormatException on write.
  These values are Dataverse-generated at build time and are environment-specific — confirm them
  via `describe cre_timeentry` or the option set metadata before hardcoding a number for a new
  environment; do not assume the reference values above carry over.)*
- **`cre_calendareventid`** = event `event_id`, **only when a real value exists — omit the field
  entirely rather than passing `null`** for this or any other optional field. *(Found 2026-09-19:
  passing an explicit `null` caused a write failure; omission is the safe way to say "no value.")*
  **Resolved 2026-09-20:** this field was null on every Phase-1 write because the Get Calendar
  Entries skill's counterpart wasn't reliably copying `event_id` through from the raw Work IQ
  response — now fixed there. If it goes null again, check that skill first, not this one; the raw
  data has always had a real ID to copy.

On success, return the new `cre_timeentryid` for each row. On failure, return the exact Dataverse
error code and message. Do not retry blindly — but self-correcting once via `describe` to check a
field name or option value before a second attempt is fine; that is exactly how the bugs above were
originally found and is a legitimate part of this skill's behaviour, not a failure mode to prevent.
Do not paper over duplicate-key errors — those mean the entry already exists from a prior run.

## Rules

1. **Never write without `user_confirmed = true`.**
2. **Never write more than one entry per unique `cre_calendareventid`** for the same user (the
   alternate key enforces this anyway — when it's actually populated, see the open question above).
3. **Project lookup by code.** Match user-provided project strings against `cre_code` first, then
   `cre_name` as fallback.
4. **Quarter-hour granularity.** Reject any `cre_hours` not a multiple of 0.25.

## What this skill must not do

- Do not read or write any table other than `cre_project` (read) and `cre_timeentry` (write).
- Do not read or write data outside the signed-in user's own rows.
- Do not invent projects. If no match, ask the caller to ask the user.
- Do not future-date entries.
- Do not write entries with zero hours.
