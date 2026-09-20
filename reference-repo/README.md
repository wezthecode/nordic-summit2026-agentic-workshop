# Reference repo

The buildable artifacts the workshop labs produce and use.

```
reference-repo/
├── docs/                         # Setup guides + the real build history
│   ├── dataverse-mcp-custom-client.md
│   └── foundry-buildout-log.md
├── low-code/                     # Copilot Studio notes (lab 02)
│   └── README.md
├── pro-code/                     # Microsoft Agent Framework SDK build (lab 03) — two target
│   │                               frameworks, identical code and fixes, pick one
│   ├── net10/TimesheetAgent.Foundry/   # Current — .NET 10 (current LTS). Use this one.
│   │   ├── TimesheetAgent.Foundry.csproj
│   │   ├── Program.cs
│   │   ├── Agents/
│   │   │   ├── OrchestratorAgent.cs
│   │   │   ├── CalendarChildAgent.cs
│   │   │   └── TimeEntryChildAgent.cs
│   │   └── Mcp/
│   │       ├── WorkIqCalendarMcpClient.cs
│   │       ├── DataverseMcpClient.cs
│   │       ├── BearerTokenHandler.cs
│   │       └── ReconnectingMcpTool.cs
│   └── net8/TimesheetAgent.Foundry/    # Archived — matches the originally-advertised .NET 8.
│                                          Same structure as net10/, not actively maintained.
└── eval/                         # Harness for lab 05
```

## Status

`pro-code/net10/TimesheetAgent.Foundry/` builds clean and runs as a local console app — verified
end-to-end against live Work IQ Calendar MCP and live Dataverse, including a real write. It's not
a hosted Foundry agent (see [`docs/foundry-buildout-log.md`](docs/foundry-buildout-log.md) for why,
and what that distinction actually means in practice). Follow
[`docs/dataverse-mcp-custom-client.md`](docs/dataverse-mcp-custom-client.md) for the one-time Entra
setup lab 03 needs before it will run in your own tenant.

`pro-code/net8/TimesheetAgent.Foundry/` is the same code on .NET 8 — this workshop's session
description originally advertised .NET 8, so that version is preserved rather than silently
replaced. It carries every fix `net10/` has (they diverged at exactly one point: the
`TargetFramework` line and a handful of doc mentions, nothing behavioural), but isn't the one
being actively tested or maintained going forward. .NET 8's LTS support ends November 2026; .NET
10 is supported until November 2028 — use `net10/` unless you have a specific reason not to.

## What's intentionally absent

- No plugins folder — workshop doesn't ship one
- No Azure Functions folder — workshop doesn't ship one
- No custom MCP server — both MCPs are first-party
- No infrastructure-as-code — the pro-code build runs as a local console app, not a deployed
  Azure resource; see `docs/foundry-buildout-log.md` for the reasoning
