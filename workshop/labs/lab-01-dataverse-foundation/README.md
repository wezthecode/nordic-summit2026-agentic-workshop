# Lab 01 — Dataverse foundation

**Time budget:** 30 minutes (module 2)
**Prereq:** Lab 00 complete — `pac` signed in, dev environment chosen
**Authoritative spec:** [`shared-assets/dataverse-schema.md`](../../shared-assets/dataverse-schema.md) — read this first

## Goal

Stand up the data + security + UI layer that **both** the Copilot Studio agent (lab 02) and the Foundry agent (lab 03) will share. By the end of this lab you'll have:

- The `AgenticTimesheet` solution (publisher prefix `cre_`) in your dev environment
- The two tables (`cre_project`, `cre_timeentry`) with the columns and constraints from the [schema spec](../../shared-assets/dataverse-schema.md)
- 3 seeded projects from [`shared-assets/sample-data/projects.csv`](../../shared-assets/sample-data/projects.csv)
- The `TimesheetUser` security role granting per-user CRUD on time entries and org-read on projects
- The `Weekly Timesheet` model-driven app with the views and forms in the schema spec
- Dataverse-native validation working: future-dated entries rejected, fractional non-quarter hours rejected, duplicate `cre_calendareventid` rejected — **all without a single line of plugin code**

## Why this exists in the workshop

This 30 minutes is the most boring module on paper and the most important in practice. If the data layer isn't right, neither agent demo works. We do it once, well, and never touch it again.

## Steps

The schema spec is the source of truth. These steps are the order to apply it in.

### 1. Create the solution

Via maker portal: Solutions → New solution → Display name `Agentic Timesheet`, name `AgenticTimesheet`, publisher prefix `cre`.

### 2. Create `cre_project` (the lookup)

Columns from the [schema spec](../../shared-assets/dataverse-schema.md#table-1--cre_project). Mark as **organisation-owned**. Set primary name column to `cre_name`.

### 3. Seed `cre_project`

Import [`projects.csv`](../../shared-assets/sample-data/projects.csv) via the maker portal table import. 3 rows.

### 4. Create `cre_timeentry`

Columns from the [schema spec](../../shared-assets/dataverse-schema.md#table-2--cre_timeentry). **User-owned.** Primary name column is `cre_name` with a business rule to auto-populate when blank (see spec).

> **Watch-out:** the `cre_source` choice column has labels that match the runtime values the agents will set. Get the values exact: `Manual = 1`, `Copilot Studio agent = 2`, `Foundry agent = 3`. Lab 04 reads this field directly.

### 5. Add the validation rules

All four rules in the [Validation table](../../shared-assets/dataverse-schema.md#validation-no-overlap-no-duplicate-no-future-dated-entries):

- Column constraint on `cre_hours` range
- Business rule: `cre_date <= UTCNOW()`
- Alternate key on (`ownerid`, `cre_calendareventid`)
- Business rule: `cre_hours` must be multiple of 0.25

**No plugin. No Azure Function.** If you find yourself reaching for either, stop and re-read the spec.

### 6. Create `TimesheetUser` security role

Privileges per the [security-role table](../../shared-assets/dataverse-schema.md#security-role) in the spec. Assign it to **your own user** in this environment.

### 7. Create the model-driven app

`Weekly Timesheet` per the [MDA section](../../shared-assets/dataverse-schema.md#model-driven-app). Enable Copilot on the app — we'll show it side-by-side with the standalone agents in lab 02.

### 8. Smoke test

Manually create one time entry via the MDA quick-create form. Confirm:
- Auto-populated `cre_name` looks right
- `cre_source` defaults to `Manual`
- Future-dating it is rejected
- Setting hours to `1.733` is rejected

### 9. Export the solution

```bash
pac solution export --path ./AgenticTimesheet_managed.zip --name AgenticTimesheet --managed
pac solution export --path ./AgenticTimesheet_unmanaged.zip --name AgenticTimesheet
```

Drop both into `workshop/shared-assets/solution/`. These are your recovery artefact for the rest of the workshop.

## You will leave this lab with

- Working solution in your dev environment
- Exported solution zips in `shared-assets/solution/`
- A smoke-tested MDA you can use as the visual confirmation surface for both upcoming agent builds

## Common failures

| Symptom | Cause | Fix |
|---------|-------|-----|
| `pac` says "no environments" | Wrong tenant signed in | `pac auth clear`, sign in again |
| Project lookup is empty when seeding from CSV | Wrong column header names | Match the exact headers in [`projects.csv`](../../shared-assets/sample-data/projects.csv) |
| Future-date rule fires on a date that looks today | Time zone — `cre_date` is date-only, business rule uses UTC | Document, don't fight it. Acceptable for the workshop. |
| Alternate key won't create | Need both columns in the key, `cre_calendareventid` must be non-null searchable | Confirm column type is Text, indexed |

## Done?

Move to [lab-02-copilot-studio-build](../lab-02-copilot-studio-build/).
