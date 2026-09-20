# Lab 05 — Evaluation: same scenarios, both runtimes

**Time budget:** 15 minutes (module 6)
**Prereq:** Labs 02 + 03 deployed and working. Both runtimes can be invoked end-to-end.
**Authoritative inputs:** [`eval/scenarios.json`](../Completed_Labs/eval/scenarios.json), [`task-success.md`](../Completed_Labs/eval/graders/task-success.md), [`conversation-quality.md`](../Completed_Labs/eval/graders/conversation-quality.md)

## Goal

Look at the same set of test scenarios against both runtimes, and discuss honestly where they agree,
where they diverge, and why.

## Why this exists

Anyone can demo a happy path. The point of this lab is to look at **edge cases** — duplicate keys,
ambiguous projects, future dates, empty days — and see which failure modes each runtime handles
gracefully vs which it papers over.

This is **not** a benchmark. It's a teaching exercise about the difference between "demo worked
once" and "I'd trust this in prod."

## What's here, and what isn't

`Completed_Labs/eval/` has the real scenario set (`scenarios.json`, 10 scenarios covering happy
paths, edge cases, and guardrails) and two complete grading rubrics — `task-success.md` (did the
right Dataverse rows land) and `conversation-quality.md` (an LLM-graded rubric for the
conversation itself). Read both before this module; they're short.

What's **not** built is an automated CLI harness that runs all 10 scenarios against both runtimes
unattended and diffs the results. That's a real, larger piece of work — driving Copilot Studio via
Direct Line and the Foundry console app via a scripted conversation, running both graders, and
rendering a comparison table — deliberately out of scope for this workshop. If you want to build
it yourself afterwards, `Completed_Labs/eval/Program.cs` sketches the intended command surface as a
starting point.

## Steps

### 1. Pick 2–3 scenarios from `scenarios.json`

Good ones to compare: `happy-path-last-week` (the baseline), `duplicate-event-rejection` (a real
guardrail — Dataverse's alternate key should reject it, not silently overwrite), and
`future-date-rejected` (tests whether the runtime actually stops you, or just complains and does it
anyway).

### 2. Run each one manually against both runtimes

Copilot Studio: the Preview tab, same as lab 02. Foundry: `dotnet run` in
`Completed_Labs/pro-code/net10/TimesheetAgent.Foundry/`, same as lab 03. Use the scenario's
`user_turn_1` as your opening message.

### 3. Grade what you see, by hand, against the two rubrics

For `task-success.md`: did the expected rows land in `cre_timeentry`, with the right fields? For
`conversation-quality.md`: read the rubric's criteria and score the conversation yourself — this is
exactly what the LLM grader would be automating, so doing it by hand once is a good way to see what
it's actually checking.

### 4. Discuss as a group

For any scenario where the two runtimes handled things differently:

1. **Was it the runtime, or was it the prompt?** Confirm both runtimes actually loaded the same
   prompt verbatim from `Labs/prompts/`. A lot of apparent "runtime differences" are really prompt
   drift between the two builds.
2. **Was it a real failure, or a different-but-acceptable behaviour?** Example: one runtime says
   "I can't do that, it's in the future" and the other says "That date is in the future — please
   pick a past date." Both are honest, correct behaviour, just worded differently.
3. **What would you change?** If you were shipping this to your team Monday, what would you tune —
   the prompt, the schema, the grader rubric, or the runtime choice itself?

## You will leave this lab with

- Hands-on experience grading real agent output against a real rubric, not just watching a demo
- A clear-eyed view that **neither runtime "wins"** — they handle the same scenarios with slightly
  different shapes, and the job is picking the one whose shape matches your constraints

## Closing thought

Both runtimes ran. Both worked. Both have rough edges. The evaluation habit — testing edge cases,
not just the happy path, against a rubric you actually wrote down — matters more than which runtime
you picked.
