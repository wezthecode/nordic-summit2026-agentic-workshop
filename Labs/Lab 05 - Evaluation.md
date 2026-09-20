# Lab 05 — Evaluation: same scenarios, both runtimes

**Time budget:** 15 minutes (module 6)
**Prereq:** Labs 02 + 03 deployed and working. Both runtimes can be invoked end-to-end.
**Authoritative inputs:** [`eval/scenarios.json`](../Completed_Labs/eval/scenarios.json), [`task-success.md`](../Completed_Labs/eval/graders/task-success.md), [`conversation-quality.md`](../Completed_Labs/eval/graders/conversation-quality.md)

## Goal

Run the same 10 scenarios against both runtimes. Collect two `results.json` files. Diff them. Discuss honestly.

## Why this exists

Anyone can demo a happy path. The point of this lab is to **stress both runtimes with the same edge cases** — duplicate keys, ambiguous projects, future dates, empty days — and see which failure modes each runtime handles gracefully vs which it papers over.

This is **not** a benchmark. It's a teaching exercise about the difference between "demo worked once" and "I trust this in prod."

## Steps

### 1. Pre-flight

Check the harness can reach both runtimes:

```bash
cd nordic-summit2026-agentic-workshop/Completed_Labs/eval
dotnet run -- --runtime copilot-studio --scenario happy-path-last-week --dry-run
dotnet run -- --runtime foundry        --scenario happy-path-last-week --dry-run
```

Both should print "connected, dry-run OK".

### 2. Reset the Dataverse state

Between runs we want a clean `cre_timeentry` table for the test user (otherwise alternate-key collisions from the previous run will pollute results):

```bash
dotnet run -- --reset-test-data
```

This deletes all `cre_timeentry` rows owned by the test user. **It does not touch projects.**

### 3. Run all 10 scenarios against Copilot Studio

```bash
dotnet run -- --runtime copilot-studio --all > results-cs.json
```

Takes 3–4 minutes. Each scenario:
- Resets the test user's `cre_timeentry` rows
- Drives the conversation per `scenarios.json`
- Reads back the written rows
- Runs `task-success` grader (local, fast)
- Runs `conversation-quality` grader (LLM call, slower)

### 4. Run all 10 scenarios against Foundry

```bash
dotnet run -- --runtime foundry --all > results-foundry.json
```

### 5. Diff

```bash
dotnet run -- --compare results-cs.json results-foundry.json
```

Output is a side-by-side table:

```
SCENARIO                       CS-TASK   FOUNDRY-TASK   CS-CONV   FOUNDRY-CONV
happy-path-last-week           PASS      PASS           24/25     23/25
user-rejects-draft             PASS      PASS           25/25     25/25
duplicate-event-rejection      PASS      PASS           22/25     24/25
future-date-rejected           PASS      FAIL           21/25     18/25
...
```

### 6. Read the results honestly — the 5-minute discussion

Walk the table. For every row where the two runtimes differ:

1. **Was it the runtime, or was it the prompt?** Re-check that both runtimes loaded the same prompt verbatim. 80% of the time, a divergence is prompt drift, not runtime difference.
2. **Was the failure a real failure, or a different-but-acceptable behaviour?** Example: one runtime says "I can't do that, it's in the future" and the other says "That date is in the future — please pick a past date." Both honest. Grader might score them differently. That's a grader limitation, not a runtime ranking.
3. **What would you change?** If you were shipping this to your team on Monday, what specifically would you tune — the prompt, the schema, the grader rubric, or the runtime choice?

## Fallback: pre-canned results

If the live harness is too flaky on the day (Foundry endpoint latency spike, judge model throttling), we have `Completed_Labs/eval/canned-results/` with a known-good run from the rehearsal week. Walk the discussion off those instead. **Tell the audience** that we're showing canned results and why — that itself is a useful production-honesty moment.

## You will leave this lab with

- Two `results.json` files for your own runs
- A diff table you've discussed as a group
- A clear-eyed view that **neither runtime "wins"** — they handle the same scenarios with slightly different shapes, and your job is to pick the one whose shape matches your constraints

## Common failures

| Symptom | Cause | Fix |
|---------|-------|-----|
| All scenarios fail with auth error | Tokens expired between rehearsal and live | `az login` again; refresh Copilot Studio Direct Line key |
| Judge model times out on half the scenarios | Throttling on the judge deployment | Switch the judge model in `eval/appsettings.json` — same family, different deployment |
| Foundry scenarios all FAIL on duplicate-key | Test data not reset between scenarios | Confirm `--reset-test-data` ran between runs |
| Copilot Studio scenarios PASS but with 0 rows written | The DV MCP connector lost auth | Re-authorise in Copilot Studio agent settings |

## Closing slide cue

After this lab, the closer slide reads:

> "Both ran. Both worked. Both have rough edges. **Now go pick the one whose rough edges you can live with.**"
