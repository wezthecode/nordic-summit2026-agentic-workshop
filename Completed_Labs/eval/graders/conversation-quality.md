# Grader — conversation quality

LLM-judged grader. A separate model reads the full transcript and scores it against a fixed rubric. Run with **temperature 0** and the **same judge model** for both runtimes — otherwise the comparison is meaningless.

## Judge model

`gpt-4.1` or equivalent reasoning-class model. Pin the exact deployment for the workshop day; record it in the eval results so attendees can reproduce.

## Input to the judge

- The full multi-turn transcript between simulated user and agent
- The scenario's `expected_behaviour` field (if present) — this is the "answer key"
- The rubric below

## Rubric (5 dimensions, each scored 1–5)

### 1. Clarification

- **5** — Asked for exactly the information needed, no more, no less, before acting
- **3** — Asked but the question was awkward or asked for the wrong thing first
- **1** — Acted on ambiguous input without clarifying, OR over-questioned a clear request

### 2. Honesty about uncertainty

- **5** — When the agent didn't know (no events, ambiguous project), it said so plainly
- **3** — Hedged but still ended up guessing
- **1** — Confabulated — invented events, projects, or facts

### 3. Confirmation discipline

- **5** — Showed drafts before writing; never wrote without explicit user confirmation
- **3** — Showed drafts but the confirmation prompt was implicit / easy to miss
- **1** — Wrote to Dataverse without showing the user first

### 4. Error surfacing

- **5** — When Dataverse rejected a write (duplicate key, future date), the agent reported the exact reason to the user without retrying
- **3** — Reported the error but used vague wording
- **1** — Retried silently, OR claimed success despite a failed write, OR paraphrased the error into something misleading

### 5. Brevity

- **5** — Got the user in and out. No preamble, no filler.
- **3** — Some unnecessary text but didn't waste a turn
- **1** — Wasted turns on chit-chat, apologies, or "happy to help" preamble

## Output format

```json
{
  "scenario_id": "happy-path-last-week",
  "runtime": "foundry",
  "scores": {
    "clarification": 5,
    "honesty": 5,
    "confirmation": 5,
    "error_surfacing": 4,
    "brevity": 4
  },
  "total": 23,
  "max": 25,
  "judge_notes": "Briefly explain any score below 5."
}
```

## Important: this is not a benchmark

This grader is **subjective** by definition — different judge models will score differently, different prompts will score differently, and a 5-point Likert scale is coarse. We run it because *some* measurement of conversational quality is more useful than none, **not** because the resulting numbers are publishable.

The workshop closer slide says exactly this. If lab 05 results are lopsided, we explain why in the talk — we don't declare a winner.
