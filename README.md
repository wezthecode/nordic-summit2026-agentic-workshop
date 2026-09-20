# Nordic Summit 2026 — Agentic Timesheet Workshop

**Build the Same Multi-Agent System Twice — Copilot Studio AND Microsoft Agent Framework SDK — in 4 Hours**

Half-day, hands-on workshop for Nordic Summit 2026 (Billund, Denmark). Same scenario — "log my
time for this week" — built twice: once low-code in Microsoft Copilot Studio, once pro-code in
C# with the Microsoft Agent Framework SDK on Microsoft Foundry. Same Dataverse data model, same
first-party MCP servers (Dataverse MCP, Work IQ Calendar MCP), same system prompts, two different
runtimes. The workshop is the comparison.

## What's in this repo

```
.
├── workshop/            # Everything used on the day — start here if you're attending
│   ├── labs/             # 6 hands-on modules, lab-00 through lab-05
│   ├── attendee-prep/    # Read this before you arrive
│   ├── facilitator/      # Run-of-show, timings
│   └── shared-assets/    # Dataverse schema, shared prompts, sample data, eval scenarios
└── reference-repo/      # The actual buildable artifacts the labs produce
    ├── low-code/          # Copilot Studio solution notes (lab 02)
    ├── pro-code/          # .NET 8 Microsoft Agent Framework SDK build (lab 03)
    ├── eval/              # Evaluation harness (lab 05)
    └── docs/              # Setup guides and the real build history — read this if
                             something in lab 03 doesn't work the way you expect
```

**Start here:** [`workshop/README.md`](workshop/README.md) for the full module map, then
[`workshop/labs/lab-00-environment-setup/`](workshop/labs/lab-00-environment-setup/) to begin.

## The two builds

- **Low-code (lab 02):** Three Copilot Studio agents on the GitHub Copilot harness — an
  orchestrator plus a Calendar specialist (Work IQ Calendar MCP) and a Time-Entry specialist
  (Dataverse MCP) — wired together as connected agents.
- **Pro-code (lab 03):** [`reference-repo/pro-code/TimesheetAgent.Foundry/`](reference-repo/pro-code/TimesheetAgent.Foundry/)
  — the same three agents as Microsoft Agent Framework `ChatClientAgent` objects in a C# console
  app, calling the same two MCPs directly, backed by a Foundry-hosted model. Read
  [`reference-repo/docs/foundry-buildout-log.md`](reference-repo/docs/foundry-buildout-log.md)
  for the real story of getting this working — the auth model, the platform gotchas, and the
  bugs found and fixed along the way. It's better material than a clean writeup would have been.

## License

MIT. See [`LICENSE`](LICENSE).
