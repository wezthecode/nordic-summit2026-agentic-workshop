# Nordic Summit 2026 — Half-Day Workshop

**Session:** Build the Same Multi-Agent System Twice — Copilot Studio AND Microsoft Agent Framework SDK — in 4 Hours
**Status:** ✅ Accepted (15 May 2026)
**Date:** 21 September 2026 (workshop day), Billund, Denmark
**Duration:** 240 minutes (4 hours), 6 modules
**Level:** Advanced
**Audience:** Developer

---

## Structure

This folder holds everything used **on the day** of the workshop. It is the single source of truth for what attendees do.

```
workshop/
├── README.md                  ← you are here
├── attendee-prep/             ← what attendees read before they arrive
│   └── README.md
├── labs/                      ← 6 hands-on modules
│   ├── lab-00-environment-setup/
│   ├── lab-01-dataverse-foundation/
│   ├── lab-02-copilot-studio-build/
│   ├── lab-03-foundry-build/
│   ├── lab-04-governance/
│   └── lab-05-evaluation/
├── facilitator/               ← Wesley's run-of-show, timings, fallback plans
│   └── run-of-show.md
└── shared-assets/             ← solution exports, dataset, eval harness
    └── README.md
```

## Module → Lab map

The 6 modules in the abstract map directly onto the 6 labs:

| # | Module (from abstract)                              | Time   | Lab folder                          |
|---|------------------------------------------------------|--------|--------------------------------------|
| 1 | Foundations                                          | 30 min | `lab-00-environment-setup/` (env verification happens here too) |
| 2 | Dataverse foundation                                 | 30 min | `lab-01-dataverse-foundation/`       |
| 3 | Build it in Copilot Studio                           | 60 min | `lab-02-copilot-studio-build/`       |
| 4 | Build the same thing on Foundry                      | 60 min | `lab-03-foundry-build/`              |
| 5 | Governance, identity, cost across two runtimes       | 30 min | `lab-04-governance/`                 |
| 6 | Evaluation                                           | 30 min | `lab-05-evaluation/`                 |

Module 1 (Foundations) is mostly walk-through with slides; the env-verification steps live inside `lab-00-environment-setup` so attendees can self-check while the slides run.

## Re-use from existing TechCon material

| Existing                                                   | Re-used in                                  | What changes |
|------------------------------------------------------------|---------------------------------------------|--------------|
| [`exercises/lab-01-dataverse-setup/`](../../exercises/lab-01-dataverse-setup/) | `lab-01-dataverse-foundation/`              | Swap *Case* schema for *Timesheet* schema; keep solution / security-role / MDA pattern |
| [`docs/architecture/overview.md`](../../docs/architecture/overview.md) | `attendee-prep/README.md` (linked context)  | Reference, no copy |
| `exercises/lab-02-plugin-development/`                     | **Retired** — out of scope (no plugin)      | — |
| `exercises/lab-03-azure-function/`                         | **Retired** — out of scope (no Azure Functions in this scenario) | — |
| `exercises/lab-04-alm-pipeline/`                            | **Retired for the workshop** — may surface later in a follow-up post | — |

## Build order (between now and 21 Sept)

This is the order I'll author the labs in, not the order attendees do them.

1. **Shared assets first** — Dataverse solution skeleton + MDA + sample dataset + the eval harness. These are dependencies for several labs.
2. **`lab-01-dataverse-foundation/`** — quickest win; mostly a fork of existing TechCon `01-dataverse-setup` with Timesheet schema.
3. **`lab-02-copilot-studio-build/`** — this pattern (orchestrator + two connected agents) also underpins demo 3 of the accepted 20-min hook session, so doubles its value.
4. **`lab-03-foundry-build/`** — biggest unknown; needs the most rehearsal because Foundry provisioning timings vary.
5. **`lab-04-governance/`** — depends on labs 2 + 3 existing.
6. **`lab-05-evaluation/`** — depends on labs 2 + 3 existing.
7. **`lab-00-environment-setup/`** — written last on purpose: only then do we know exactly what every attendee needs.

## Open questions (to resolve before 25 May agenda announce)

- [ ] Shared Foundry sandbox: which Azure subscription hosts it, and who pays?
- [ ] Repo home: public GitHub repo name (working name `agentic-timesheet-workshop`) and owner (personal vs Opera org)?
- [ ] Workshop ticket / capacity from Nordic Summit organisers — affects sandbox sizing
- [ ] Dataverse trial provisioning script: do we hand attendees a PowerShell snippet or rely on the trial UI?
- [ ] Tagging on every Azure / Dataverse resource attendees create (so they can clean up after)

## Things deliberately NOT in this workshop

- No custom plugins (`exercises/lab-02-plugin-development/` retired for this path)
- No Azure Functions or Service Bus (`exercises/lab-03-azure-function/` retired for this path)
- No custom MCP server — both MCPs are first-party Microsoft (Dataverse MCP + Work IQ Calendar MCP)
- No ALM pipeline build during the 4 hours — attendees take an opinionated repo home, but we don't burn workshop time on ADO/GitHub Actions setup
- No production-grade auth deep dive — covered briefly in lab-04, deferred to a follow-up post

Keep this list short and visible. Every "wouldn't it be cool if…" expansion needs a corresponding cut elsewhere or the 4 hours overrun.
