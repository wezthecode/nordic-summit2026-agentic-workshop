# Eval harness — how to run

Both runtimes use the same `scenarios.json` and the same two graders. Each runtime exposes a programmatic invocation surface; the harness wraps that surface.

## Architecture

```
scenarios.json  ─┐
                  ├──→  harness  ──→  agent under test  ──→  Dataverse
graders/        ─┘                        │                       │
                                          └─── transcript ────────┘
                                                  │
                                                  ▼
                                           graders read state
                                                  │
                                                  ▼
                                              results.json
```

## Per-runtime invocation (TODO)

### Copilot Studio (lab 02)

> **TODO (author):** decide whether we use the built-in Copilot Studio evaluations UI or a programmatic harness via the Direct Line / Power Platform API. Built-in UI is easier to demo; programmatic is easier to reproduce. **Default plan:** programmatic via the bot connection, results imported into the same `results.json` shape.

### Microsoft Foundry (lab 03)

> **TODO (author):** call the deployed Foundry agent via its REST endpoint with a test-user token. Output rows are read back via the Dataverse Web API as the test user. Graders run locally over the merged transcript + row set.

## Output

A single `results.json` per run:

```json
{
  "run_id": "2026-09-21T13:45:00Z",
  "judge_model": "gpt-4.1",
  "runtime": "foundry",
  "scenarios": [
    { "scenario_id": "happy-path-last-week", "task_success": {...}, "conversation_quality": {...} }
  ],
  "totals": {
    "task_pass_rate": 0.9,
    "conv_avg_total": 22.4
  }
}
```

Two runs (`runtime: "copilot-studio"` and `runtime: "foundry"`) get diffed into a comparison table for the lab 05 read-out.

## What "good" looks like

The point isn't to hit 100%. The point is for both runtimes to be **comparable** on the same scenarios with the same judge. If one runtime scores wildly higher on conversation-quality, the first thing to check is prompt drift — did the prompts actually stay aligned, or did one runtime get tweaked mid-build? Re-aligning is the fix. Cherry-picking results to favour a runtime is not.
