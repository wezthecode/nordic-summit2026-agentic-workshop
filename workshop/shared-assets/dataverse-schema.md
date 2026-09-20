# Dataverse schema — the spec

This is the **single source of truth** for the data layer that both the Copilot Studio build (lab 02) and the Microsoft Foundry build (lab 03) write into. Both runtimes consume this schema via the **Dataverse MCP** (first-party, GA).

If you change anything here, you change it in the live solution **and** re-export. Drift kills the demo.

---

## Solution

| Property              | Value                            |
|-----------------------|----------------------------------|
| Display name          | Agentic Timesheet                |
| Unique name           | AgenticTimesheet                 |
| Publisher display     | Wez the Code                     |
| Publisher prefix      | `cre`                            |
| Version               | 1.0.0.0                          |

Publisher prefix `cre` (short for "code-real") is chosen because it's short, unused by Microsoft samples, and doesn't read as a personal handle. Every custom column and table starts `cre_`.

---

## Tables

### Table 1 — `cre_project`

Lookup table. Seeded with 3 rows from `sample-data/projects.csv`. Read-only for the agents in lab 02 / 03 — they reference it but never create projects.

| Logical name       | Display name | Type             | Required | Notes                                  |
|--------------------|--------------|------------------|----------|----------------------------------------|
| `cre_projectid`    | Project      | Primary key (GUID, auto) | system | —                            |
| `cre_name`         | Name         | Text (100)       | yes      | Primary name column                    |
| `cre_code`         | Code         | Text (20)        | yes      | Short code used by users in chat ("PROJ-A") |
| `cre_isactive`     | Is Active    | Yes/No           | yes      | Default `Yes`. Filter agents to active only |

**Ownership:** organisation-owned (it's reference data, not user data).

### Table 2 — `cre_timeentry`

The thing both agents create. User-owned so security roles can scope to "your own rows".

| Logical name        | Display name  | Type                         | Required | Notes |
|---------------------|---------------|------------------------------|----------|-------|
| `cre_timeentryid`   | Time Entry    | Primary key (GUID, auto)     | system   | — |
| `cre_name`          | Summary       | Text (200)                   | yes      | Primary name column. Auto-populated from `Date + Project Code + Hours` if the agent doesn't set it. |
| `cre_date`          | Date          | Date only (user-local)       | yes      | The day the time was worked. **Not** the day it was logged. |
| `cre_hours`         | Hours         | Decimal (1 dp), min 0.25, max 24 | yes  | Quarter-hour granularity. |
| `cre_project`       | Project       | Lookup → `cre_project`       | yes      | Filtered to `cre_isactive == true` in the MDA. |
| `cre_description`   | Description   | Multiline text (2000)        | no       | What the user actually did. Free-text. |
| `cre_source`        | Source        | Choice                       | yes      | `Manual`, `Copilot Studio agent`, `Foundry agent`. Default `Manual`. Each runtime sets its own value — that's how lab-04 tells them apart. **Do not hardcode option numbers from this doc** — Dataverse auto-generates them at build time (confirmed in the reference environment as `100000000`/`100000001`/`100000002`, not the originally planned `1`/`2`/`3`) and they are not portable across environments. Always confirm via `describe cre_timeentry` or the option set metadata before writing. |
| `cre_calendareventid` | Calendar Event ID | Text (200)             | no       | Outlook event the entry was generated from, if any. Used for de-duplication. |
| `ownerid`           | Owner         | Lookup → User/Team (system)  | yes      | Standard Dataverse ownership. |

**Ownership:** user-owned.

**Primary name auto-populate rule:** when `cre_name` is blank on create, set it to `{cre_date:yyyy-MM-dd} · {project.cre_code} · {cre_hours}h`. Implement as a Dataverse business rule on Create.

### Validation: no overlap, no duplicate, no future-dated entries

These are Dataverse-native rules. **No plugin. No Azure Function. No agent-side check.** The data layer enforces them so both runtimes get the same guarantees for free.

| Rule | Implementation | Scope |
|------|----------------|-------|
| **Hours sane** | Column constraint: `cre_hours` between 0.25 and 24 | column |
| **No future dates** | Business rule on Create + Update: `cre_date <= UTCNOW()` | table |
| **No duplicate calendar event** | Alternate key on (`ownerid`, `cre_calendareventid`) where `cre_calendareventid` is not null | table |
| **Quarter-hour granularity** | Business rule on Create + Update: `cre_hours` must be a multiple of `0.25` | table |

**Overlap on the same day** (e.g. two entries both 09:00–11:00) is *not* enforced at the data layer because we don't store start/end times — only date and hours. The agents are responsible for not double-logging the same calendar event, which is what the alternate key on `cre_calendareventid` prevents. This is a deliberate design choice and a talking point in lab 04.

---

## Security role

### `TimesheetUser`

Granted to every workshop attendee on their own user account. Minimal privileges only.

| Table             | Create | Read    | Write   | Delete | Append | Append To | Assign | Share |
|-------------------|--------|---------|---------|--------|--------|-----------|--------|-------|
| `cre_project`     | none   | org     | none    | none   | none   | org       | none   | none  |
| `cre_timeentry`   | user   | user    | user    | user   | user   | user      | none   | none  |

Translation: an attendee can do anything they want with **their own** time entries and can read **all** projects. They can't see anyone else's entries and they can't modify projects. This is the same role both the Copilot Studio agent and the Foundry agent inherit when acting on the user's behalf.

---

## Model-driven app

| Property          | Value                                                          |
|-------------------|----------------------------------------------------------------|
| Display name      | Weekly Timesheet                                               |
| Unique name       | cre_weeklytimesheet                                            |
| Tables in sitemap | `cre_timeentry` (default landing), `cre_project` (read-only)   |
| Forms             | Main form on `cre_timeentry`, quick-create form on `cre_timeentry` |
| Views             | "My time entries — this week" (default), "My time entries — last 30 days", "All projects" |
| Copilot           | Enabled (lets us show the in-app Copilot side-by-side with the standalone agents) |

---

## Why these specific choices

A few decisions deserve their reason on the page so we don't relitigate them mid-workshop:

- **Date-only, not datetime range.** A timesheet entry is "I worked 2 hours on Monday on PROJ-A." The agents collapse calendar events into a per-day-per-project total. This is how real timesheet apps work — billing systems don't care that you took two coffee breaks.
- **`cre_source` choice column.** This is the single most useful diagnostic field in the whole workshop. Lab 04's governance comparison reads directly off it. Don't drop it.
- **No `status` / approval workflow.** Out of scope. The workshop is about the agent build, not about timesheet approvals. One sentence in the closer slide acknowledges this.
- **`cre_calendareventid` instead of a structured link.** Outlook event IDs are stable strings. A real implementation would link to the event; we don't because lab 02 / 03 attendees don't all have the same calendar data.
- **Quarter-hour granularity.** Matches Work IQ's typical rounding behaviour and stops the agent proposing `1.733 hours`.
