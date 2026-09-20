# Reference repo

The buildable artifacts the workshop labs produce and use.

```
reference-repo/
├── docs/                         # Setup guides + the real build history
│   ├── dataverse-mcp-custom-client.md
│   └── foundry-buildout-log.md
├── low-code/                     # Copilot Studio notes (lab 02)
│   └── README.md
├── pro-code/
│   └── TimesheetAgent.Foundry/   # .NET 10 Microsoft Agent Framework SDK build (lab 03)
│       ├── TimesheetAgent.Foundry.csproj
│       ├── Program.cs
│       ├── Agents/
│       │   ├── OrchestratorAgent.cs
│       │   ├── CalendarChildAgent.cs
│       │   └── TimeEntryChildAgent.cs
│       └── Mcp/
│           ├── WorkIqCalendarMcpClient.cs
│           ├── DataverseMcpClient.cs
│           ├── BearerTokenHandler.cs
│           └── ReconnectingMcpTool.cs
└── eval/                         # Harness for lab 05
```

## Status

`pro-code/TimesheetAgent.Foundry/` builds clean and runs as a local console app — verified
end-to-end against live Work IQ Calendar MCP and live Dataverse, including a real write. It's not
a hosted Foundry agent (see [`docs/foundry-buildout-log.md`](docs/foundry-buildout-log.md) for why,
and what that distinction actually means in practice). Follow
[`docs/dataverse-mcp-custom-client.md`](docs/dataverse-mcp-custom-client.md) for the one-time Entra
setup lab 03 needs before it will run in your own tenant.

## What's intentionally absent

- No plugins folder — workshop doesn't ship one
- No Azure Functions folder — workshop doesn't ship one
- No custom MCP server — both MCPs are first-party
- No infrastructure-as-code — the pro-code build runs as a local console app, not a deployed
  Azure resource; see `docs/foundry-buildout-log.md` for the reasoning
