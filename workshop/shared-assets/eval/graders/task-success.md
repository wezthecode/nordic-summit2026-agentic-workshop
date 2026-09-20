# Grader — task success

Binary, deterministic grader. No LLM in this grader. Runs identically against the Copilot Studio build and the Microsoft Foundry build.

## Input

- A scenario from `scenarios.json`
- The set of `cre_timeentry` rows that exist in Dataverse **after** the agent run completes
- Filtered to: `ownerid == test_user` and `cre_calendareventid in scenario.expected_writes[*].cre_calendareventid`

## Pass criteria — all must be true

1. **Row count matches.** `actual_rows.length == scenario.expected_writes.length`
2. **Per-row match.** For each expected row, an actual row exists with the same `cre_calendareventid` AND:
   - `cre_date` equals expected `cre_date`
   - `cre_hours` equals expected `cre_hours` (exact decimal match — no tolerance)
   - The linked `cre_project.cre_code` equals expected `project_code`
3. **No extra rows.** No actual rows exist with `cre_calendareventid` values not in the expected set.
4. **Source attribution.** Every actual row has `cre_source` set to the correct runtime (`Copilot Studio agent` for lab 02, `Foundry agent` for lab 03).

## Fail modes (each scored separately for diagnosis)

- `missing_row` — expected row not found
- `extra_row` — unexpected row written
- `wrong_hours` — row found but `cre_hours` mismatches
- `wrong_project` — row found but `cre_project.cre_code` mismatches
- `wrong_source` — `cre_source` doesn't match the runtime under test
- `duplicate_key_paper_over` — agent claimed success when the alternate key should have rejected the write
- `future_date_paper_over` — agent claimed success when the future-date business rule should have rejected
- `unexpected_failure` — agent reported a write failure when none was expected

## Output format

```json
{
  "scenario_id": "happy-path-last-week",
  "runtime": "foundry",
  "pass": true,
  "fail_modes": [],
  "actual_row_count": 3,
  "expected_row_count": 3
}
```

## What this grader does NOT measure

- How the agent talked to the user (use `conversation-quality.md` for that)
- How many turns it took
- How long it took
- Cost or token usage

Those go in the comparison table separately. This grader is one thing: **did the right rows land in the database?**
