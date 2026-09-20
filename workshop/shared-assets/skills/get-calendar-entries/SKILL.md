---
name: get-calendar-entries
description: Read the signed-in user's Outlook calendar for a date range via the Work IQ Calendar MCP and return a clean, structured event list. Use when asked what happened on the calendar, what meetings occurred, or what events took place in a date range.
---

# Get Calendar Entries

Draft Phase 2 skill for the Calendar Agent (GitHub Copilot harness). **Not yet attached or tested
live** — see `notes/copilot-studio-phase2-skills.md`. Extracted from the already-validated
`shared-assets/prompts/calendar-child.system.md` so the procedural content here is the same
content already proven correct in Phase 1, just repackaged as a skill instead of living in the
agent's always-loaded Instructions field.

## When to use this skill

Anything about what happened on the calendar, what meetings occurred, or what events took place in
a date range. This is the Calendar Agent's entire job — if this skill is attached, the agent's base
Instructions should shrink to little more than "you are the Calendar agent, use the Get Calendar
Entries skill for anything calendar-related" and this file carries the rest.

## Tool

Work IQ Calendar MCP (Microsoft-hosted, preview) — the **Work IQ MCP Server** entry under
"Work IQ (Preview)" in the tool catalogue, not the plain "Calendar" connector. No other tool is
used; this skill never writes data anywhere.

## Inputs

- `start_date` (required, date) — first day of the range, inclusive
- `end_date` (required, date) — last day of the range, inclusive
- `exclude_categories` (optional, list of strings) — categories to filter out (default:
  `["Personal", "Out of office"]`)

## Output

A JSON array of events, one element per event:

```json
{
  "event_id": "AAMkAGI2...",
  "date": "2026-09-15",
  "start_time": "09:00",
  "end_time": "10:30",
  "duration_hours": 1.5,
  "subject": "Sync with Northwind onboarding team",
  "attendee_count": 4,
  "category": null,
  "is_recurring": false
}
```

> **`event_id` must be the exact, unabridged `id` value from the raw calendar tool response for
> that event.** Confirmed 2026-09-20, via a real activity trace: the underlying Graph
> `/me/calendarView` call Work IQ makes always returns a real, stable `id` per event — the root
> cause of every null `cre_calendareventid` seen in Phase 1 testing was this skill's own
> summarisation step dropping the field, not Work IQ failing to provide one. Copy it verbatim; do
> not shorten, hash, or regenerate it.

## Rules

1. **Quarter-hour rounding.** Round `duration_hours` to the nearest 0.25. A 47-minute meeting
   becomes 0.75 hours.
2. **Skip cancelled events.**
3. **Skip events where the user declined.**
4. **Skip all-day events** unless the user explicitly asks for them in a follow-up.
5. **Do not summarise event content.** Return the subject verbatim. The caller decides what to do
   with it.
6. **Do not invent events.** If the range is empty, return `[]`. Do not pad.
7. **Preserve `event_id` exactly as returned by the tool.** Do not omit it, truncate it, or
   regenerate it — downstream deduplication depends on this being the real, stable identifier.

## What this skill must not do

- Do not infer projects from event subjects — that's the Log Time Entry skill's job, not this one.
- Do not write to Dataverse or any other system.
- Do not call any tool other than the Work IQ Calendar MCP.
- Do not read calendars other than the signed-in user's own.
