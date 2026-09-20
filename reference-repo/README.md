# Reference repo — staging area

This folder is a **staging area** for the public reference repo `agentic-timesheet-workshop` that attendees will clone on workshop day. Once stable, the contents of this folder get pushed to a standalone GitHub repo at `github.com/wezthecode/agentic-timesheet-workshop` and this folder gets retired.

Structure tracks [`reference-design/repo-plan/repo-plan.md`](../reference-design/repo-plan/repo-plan.md).

```
reference-repo/
├── low-code/                    # Copilot Studio solution exports (lab 02)
│   └── README.md
├── pro-code/
│   └── TimesheetAgent.Foundry/  # .NET 10 Agent Framework SDK build (lab 03)
│       ├── TimesheetAgent.Foundry.csproj
│       ├── Program.cs
│       ├── Agents/
│       │   ├── OrchestratorAgent.cs
│       │   ├── CalendarChildAgent.cs
│       │   └── TimeEntryChildAgent.cs
│       ├── Mcp/
│       │   ├── WorkIqCalendarMcpClient.cs
│       │   └── DataverseMcpClient.cs
│       └── Foundry/
│           ├── agent.yaml
│           └── README.md
└── eval/                        # Harness for lab 05
    ├── Eval.csproj
    ├── Program.cs
    └── Runners/
        ├── CopilotStudioRunner.cs
        └── FoundryRunner.cs
```

## Status

Skeleton only — **not yet compilable**. The intent is that the `.csproj` + `Program.cs` files are scaffolded with TODO markers, then fleshed out during rehearsal week (week of 14 Sept 2026) once the Agent Framework SDK preview API has stabilised. Building it earlier than that means rewriting it later.

## What's intentionally absent

- No plugins folder — workshop doesn't ship one
- No Azure Functions folder — workshop doesn't ship one
- No custom MCP server — both MCPs are first-party
- No infrastructure-as-code (Bicep/Terraform) — Foundry deployment is via `az foundry agent deploy` + `agent.yaml`, no separate infra
- No `pipelines/` — GitHub Actions definitions are minimal and live at the repo root when the time comes
