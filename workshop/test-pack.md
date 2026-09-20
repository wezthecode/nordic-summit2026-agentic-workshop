# Workshop test pack & sign-off

**Purpose:** end-to-end rehearsal checklist with binary pass/fail criteria so you (Wez) can sign off on workshop readiness before Nordic Summit 2026 (21 Sep 2026, Billund).

**Intended use:**
1. Rehearsal week (14 Sep 2026) — run this top-to-bottom on a clean dev tenant. Anything that fails blocks shipping until fixed.
2. Workshop morning (21 Sep 2026, 07:00 CEST) — run the **smoke test** section only as a go/no-go check.

**How to read it:**
- Each section has **inputs** (what you do), **expected** (what should happen), **pass criteria** (binary checkbox).
- A section is **green** only when *every* checkbox passes. No partial credit.
- If something fails, log it under "Defects" at the bottom with severity (blocker / major / minor) and an owner.

**Sign-off:** when every section is green, Wez initials and dates the bottom of this file in the rehearsal-week PR.

---

## 0. Pre-rehearsal setup (one-off)

Done **before** the rehearsal run so the rehearsal isn't blocked on Microsoft provisioning lag.

| # | Item | Pass criterion |
|---|------|----------------|
| 0.1 | M365 tenant with Copilot licence on the test user | `Get-MgUserLicenseDetail` shows the SKU containing `Microsoft_365_Copilot` |
| 0.2 | Power Platform dev environment provisioned, Dataverse enabled | Visible in `https://admin.powerplatform.microsoft.com`, region = West Europe |
| 0.3 | Azure subscription with Foundry preview access enabled on the same tenant | `az foundry --help` returns without "extension not found" |
| 0.4 | Work IQ Calendar MCP available in the tenant (preview) | Listed under Power Platform → MCP servers (first-party) |
| 0.5 | `pac`, `az`, `dotnet --version` ≥ 8.0, `git` all on PATH | All four `--version` commands succeed |
| 0.6 | Test user's Outlook has ≥ 10 events in the past 14 days (real or seeded) | Verified in OWA |

---

## 1. Lab 00 — Environment setup ✅ when

**Time budget:** 15 min (overlaps module 1).

| # | Step | Expected | Pass |
|---|------|----------|------|
| 1.1 | `pac auth create --environment <dev-env-url>` | Returns `Authentication profile created` | ☐ |
| 1.2 | `pac org list` | Lists at least the dev environment | ☐ |
| 1.3 | `az login` then `az account show` | Returns the correct tenant + subscription | ☐ |
| 1.4 | `git clone https://github.com/wezthecode/agentic-timesheet-workshop` | Clone succeeds, repo at expected layout | ☐ |
| 1.5 | `dotnet build reference-repo/pro-code/net10/TimesheetAgent.Foundry` | Restores + compiles cleanly (warnings ok) | ☐ |
| 1.6 | VS Code recommended extensions install prompt fires + completes | All 4 extensions report installed | ☐ |

**Blocker if:** 1.1, 1.3, or 1.5 fail. Everything else can be hand-held during the lab.

---

## 2. Lab 01 — Dataverse foundation ✅ when

**Time budget:** 45 min.

| # | Step | Expected | Pass |
|---|------|----------|------|
| 2.1 | Solution `AgenticTimesheet` created, prefix `cre` | Visible in maker portal | ☐ |
| 2.2 | Table `cre_project` created with all 4 columns from [dataverse-schema.md](shared-assets/dataverse-schema.md) | Schema matches exactly | ☐ |
| 2.3 | Table `cre_timeentry` created with all 9 columns | Schema matches exactly | ☐ |
| 2.4 | Alternate key on (`ownerid`, `cre_calendareventid`) active | Visible under table → Keys, status = Active | ☐ |
| 2.5 | Business rules active: no future dates, quarter-hour granularity, auto-populate primary name | All 3 show status = Activated | ☐ |
| 2.6 | Security role `TimesheetUser` created and assigned to test user | Test user sees only own rows | ☐ |
| 2.7 | MDA `Weekly Timesheet` created with both tables | Loads, lists 3 seeded projects | ☐ |
| 2.8 | Seed projects loaded from `sample-data/projects.csv` | 3 rows, all `cre_isactive = true` | ☐ |
| 2.9 | **Validation test:** create a time entry with `cre_date` = tomorrow | Rejected by business rule | ☐ |
| 2.10 | **Validation test:** create a time entry with `cre_hours = 1.3` | Rejected by business rule | ☐ |
| 2.11 | **Validation test:** create two entries with same `cre_calendareventid` | Second rejected with duplicate-key error | ☐ |

**Blocker if:** any of 2.9 / 2.10 / 2.11 fail. The whole workshop premise rests on Dataverse-native validation.

---

## 3. Lab 02 — Copilot Studio build ✅ when

**Time budget:** 60 min.

| # | Step | Expected | Pass |
|---|------|----------|------|
| 3.1 | Three agents created in Copilot Studio: orchestrator, Calendar Agent, Time Entry Agent | All 3 visible in solution | ☐ |
| 3.2 | Each agent's instructions pasted **verbatim** from `shared-assets/prompts/*.system.md` | Diff against file = empty | ☐ |
| 3.3 | Time Entry Agent `{{SOURCE_VALUE}}` replaced with `2` | Instruction contains `Source = 2` | ☐ |
| 3.4 | Calendar Agent wired to Work IQ Calendar MCP | Connection authorised, test call returns events | ☐ |
| 3.5 | Time Entry Agent wired to Dataverse MCP, **Allow all** off | Only `create_record`, `read_query`, `describe` enabled | ☐ |
| 3.6 | Orchestrator delegates to both children (not direct MCP) | Topic graph shows no MCP nodes under orchestrator | ☐ |
| 3.7 | Publish all three agents (specialists first) | Publish succeeds, Preview tab responds | ☐ |
| 3.8 | **End-to-end:** "Log my time for last Tuesday" → confirms drafts → writes rows | Rows appear in MDA with `cre_source = Copilot Studio agent` | ☐ |
| 3.9 | **Negative path:** ask it to log time for tomorrow | Refuses or surfaces Dataverse rejection (does not retry) | ☐ |
| 3.10 | Solution exported (managed + unmanaged) to `reference-repo/low-code/` | Both zips present | ☐ |

**Blocker if:** 3.8 or 3.9 fails. 3.10 can slip to post-rehearsal.

---

## 4. Lab 03 — Microsoft Foundry build ✅ when

**Time budget:** 60 min.

| # | Step | Expected | Pass |
|---|------|----------|------|
| 4.1 | `dotnet build reference-repo/pro-code/net10/TimesheetAgent.Foundry` | Compiles with rehearsal-week implementations in place | ☐ |
| 4.2 | Local smoke test: `dotnet run -- --scenario s01` | Prints draft entries matching scenario expectation | ☐ |
| 4.3 | `dotnet publish -c Release` succeeds | Bin folder contains `TimesheetAgent.Foundry.dll` | ☐ |
| 4.4 | `az foundry agent deploy --file Foundry/agent.yaml` | Returns endpoint URL, status = Running | ☐ |
| 4.5 | Curl invoke returns 200 with a meaningful first-turn response | Response body parses as JSON, no auth errors | ☐ |
| 4.6 | **End-to-end:** drive same conversation as lab 02 step 3.8 | Rows appear with `cre_source = Foundry agent` (3) | ☐ |
| 4.7 | **Negative path:** future-dated request | Specialist surfaces Dataverse rejection verbatim, no retry, no rephrasing | ☐ |
| 4.8 | **Negative path:** duplicate calendar event | Specialist surfaces alternate-key violation distinctly from generic error | ☐ |
| 4.9 | Application Insights shows traces for the invoke | At least orchestrator + 2 specialist spans visible | ☐ |
| 4.10 | `az foundry agent delete` teardown | Resource removed cleanly | ☐ |

**Blocker if:** 4.6, 4.7, or 4.8 fail. These are the central claims of the talk.

---

## 5. Lab 04 — Governance comparison ✅ when

**Time budget:** 30 min, discussion-led.

| # | Step | Expected | Pass |
|---|------|----------|------|
| 5.1 | Both runtimes' audit logs viewable side-by-side | Both panels load in under 30s | ☐ |
| 5.2 | The same prompt change demoed in both runtimes within the time budget | Both edits complete inside 5 min | ☐ |
| 5.3 | Comparison table filled live with attendees | At least 12 of 15 rows discussed | ☐ |
| 5.4 | Facilitator can answer "when would you pick each?" with 5 constraint-led examples | Examples align with [lab-04 README](labs/lab-04-governance/README.md) | ☐ |

**Blocker if:** none — this lab degrades gracefully. But 5.1 should pass in rehearsal so we're not fighting tooling on the day.

---

## 6. Lab 05 — Evaluation ✅ when

**Time budget:** 15 min.

| # | Step | Expected | Pass |
|---|------|----------|------|
| 6.1 | `eval --runtime copilot-studio --scenario s01 --dry-run` | Prints planned conversation, exits 0 | ☐ |
| 6.2 | `eval --reset-test-data` | All test-user `cre_timeentry` rows deleted | ☐ |
| 6.3 | `eval --runtime copilot-studio --all > results-cs.json` | File written, ≥ 8 of 10 scenarios pass | ☐ |
| 6.4 | `eval --reset-test-data` again | Test data clean | ☐ |
| 6.5 | `eval --runtime foundry --all > results-foundry.json` | File written, ≥ 8 of 10 scenarios pass | ☐ |
| 6.6 | `eval --compare results-cs.json results-foundry.json` | Side-by-side table renders, ≤ 2 divergences | ☐ |
| 6.7 | Canned fallback files exist at `shared-assets/eval/canned-results/` | Both files present, valid JSON | ☐ |

**Blocker if:** 6.3 or 6.5 fall below 8/10. Either we fix the prompts or we lower the bar publicly and own it on stage.

---

## 7. Workshop-day smoke test (07:00 CEST, 21 Sep 2026)

Run this in **20 minutes** the morning of. Goal: catch overnight breakage from Microsoft preview moves.

| # | Step | Pass |
|---|------|------|
| 7.1 | `az login` + `az foundry agent show` returns the deployed agent | ☐ |
| 7.2 | Curl-invoke the Foundry agent with the canned "smoke" prompt | ☐ |
| 7.3 | Open published Copilot Studio agent in Preview tab, send same prompt | ☐ |
| 7.4 | Both produce a row in `cre_timeentry` with the expected source | ☐ |
| 7.5 | Wi-Fi at venue confirmed working (loaded `portal.azure.com`) | ☐ |
| 7.6 | Backup pre-recorded video of both end-to-end runs accessible offline | ☐ |

**If 7.2 or 7.3 fail:** switch to recorded-video fallback for that runtime, continue with the working one live. Do **not** cancel the workshop.

---

## 8. Materials sign-off

| # | Item | Pass |
|---|------|------|
| 8.1 | All 6 lab READMEs present, no stub placeholders | ☐ |
| 8.2 | `facilitator/run-of-show.md` minutes total = 240 | ☐ |
| 8.3 | `shared-assets/prompts/*.system.md` identical to what's pasted into Copilot Studio (lab 02) and loaded by the Foundry build (lab 03) | ☐ |
| 8.4 | `shared-assets/eval/scenarios.json` has 10 scenarios, all schema-valid | ☐ |
| 8.5 | Slide deck minute-aligned to run-of-show | ☐ |
| 8.6 | Reference repo public at `github.com/wezthecode/agentic-timesheet-workshop` and clones without auth | ☐ |
| 8.7 | Attendee prep email sent ≥ 7 days before event | ☐ |
| 8.8 | Speaker bio, headshot, session blurb confirmed with Nordic Summit organisers | ☐ |

---

## Defects log

Append rows during rehearsal. Format: `<date> | <section> | <severity: blocker/major/minor> | <description> | <owner> | <status>`

```
2026-09-14 | 4.4         | blocker | example placeholder — delete me        | wez   | open
```

---

## Sign-off

When every section above is green:

| Role | Name | Date | Initials |
|------|------|------|----------|
| Lead facilitator | Wesley Nell | | |
| Co-facilitator (if applicable) | | | |

Once signed, this file is frozen. Any change post-sign-off needs a defect log entry and a re-test of the affected section.
