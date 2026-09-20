# Lab 04 — Governance & operations comparison

**Time budget:** 30 minutes (module 5)
**Prereq:** Lab 02 and Lab 03 complete — both runtimes have written rows to `cre_timeentry` for the same scenarios
**Format:** discussion-led, whiteboarded, attendees fill in the comparison table together

## Goal

Walk out with an **honest, evidence-based** comparison of the two runtimes across the dimensions that actually matter in production. No declared winner. The point is to give attendees a framework to choose for themselves.

## Why this exists

If the workshop stopped after lab 03, attendees would leave with "I built two things, they both worked, now what." Lab 04 is the *now what*.

## The comparison table

We build this on a whiteboard / flipchart together. Each row is filled from real evidence from labs 02 and 03 — not vendor docs.

| Dimension | Copilot Studio | Microsoft Foundry + Agent Framework SDK | What this means in production |
|-----------|----------------|------------------------------------------|-------------------------------|
| **Source control** |  |  | Can two engineers merge changes? |
| **Diff-ability** |  |  | Can you code-review a prompt change? |
| **CI/CD** |  |  | Build → test → deploy story |
| **Local dev loop** |  |  | F5 → see it work, how fast? |
| **Identity model** |  |  | Who does the agent run as? |
| **Secrets handling** |  |  | Where do API keys / connection strings live? |
| **Observability** |  |  | Can you see what the agent did, 3 weeks later? |
| **Prompt iteration speed** |  |  | Change → test cycle time |
| **MCP surface control** |  |  | Can you restrict what the agent can touch? |
| **Cost model** |  |  | Per-message? Per-token? Bundled into a licence? |
| **Licensing prerequisite** |  |  | What does the user need to invoke the agent? |
| **Skill required to maintain** |  |  | Power Platform maker vs .NET engineer |
| **Time to first working agent** |  |  | Greenfield → demo |
| **Time to second working agent** |  |  | Same shape, different data — how reusable? |
| **Recovery if vendor changes preview behaviour** |  |  | What happens when Work IQ MCP's response shape changes? |

> **Diagnostic key used during the discussion:** the `cre_source` column in `cre_timeentry`. We can query rows tagged `Copilot Studio agent` vs `Foundry agent` and look at the actual write rate, the failure rate, and the lock-step on duplicate-key handling.

## How this module runs

1. **5 min — compare the audit surfaces.** Look at Power Platform admin center and the Foundry portal side by side. Note what each one *does* and *doesn't* capture.
2. **5 min — trace a prompt change through both runtimes.** Edit a line of `orchestrator.system.md` in `Labs/prompts/`. Walk through how that change reaches each runtime — Copilot Studio: copy-paste, re-publish; Foundry: edit the file, `git commit`, redeploy. Time both, note the seconds.
3. **15 min — fill in the table as a group.** Call out what you found for each cell from your own labs 02/03 runs. Where people disagree, that disagreement *is* the answer for that row — write both.
4. **5 min — the question to leave with.**

## The question to leave with

Not "which is better." That's the wrong question.

The right question is: **"Which constraints do I have that make one of these obviously wrong for *this* problem?"**

Examples of constraint-led choices:

- "Our compliance team requires every prompt change to have a code review and a ticket" → Foundry, no debate
- "The agent will be maintained by the same Power Platform team that already runs our model-driven apps" → Copilot Studio
- "We need to invoke this from a backend cron job with no human in the loop" → Foundry (or Logic Apps + Dataverse, even simpler)
- "Our user base is licensed for M365 Copilot already and the agent is conversational" → Copilot Studio
- "We expect the prompt to change weekly based on prod feedback loops" → Foundry, for the diff history alone

## What this lab does NOT teach

- "Best practice" rules for picking one — there aren't any, only constraints
- Cost modelling at scale — the data isn't public enough to be honest about
- Hybrid patterns (CS calling Foundry agents as tools or vice versa) — possible, out of scope, mentioned in the closing slide only

## Done?

Move to [Lab 05 - Evaluation](Lab%2005%20-%20Evaluation.md).
