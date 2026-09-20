# Completed Labs

Reference / answer-key material — if you fall behind during a lab, or just want to check your work
or skip straight to inspecting a finished build, it's here.

- **`dataverse/`** — the Dataverse solution from lab 01 (`cre_project`, `cre_timeentry`, the
  `TimesheetUser` security role, the Weekly Timesheet model-driven app). Managed + unmanaged.
- **`copilot-studio-agents/`** — the three Copilot Studio agents from lab 02 (Orchestrator,
  Calendar Agent, Time Entry Agent) as an importable solution, separate from the Dataverse layer.
- **`complete-solution/`** — the Dataverse layer and the Copilot Studio agents together, one import.
- **`pro-code/`** — the C# / Microsoft Agent Framework SDK build from lab 03.
  - `net10/` — current, actively maintained (.NET 10, the current LTS)
  - `net8/` — this workshop was originally advertised on .NET 8; identical code and fixes,
    preserved at the older target framework for anyone who specifically needs it
  - `dataverse-mcp-custom-client.md` — the one-time Entra setup lab 03 needs before it runs in
    your own tenant
- **`low-code/`** — notes on the Copilot Studio solution structure.
- **`eval/`** — the real scenario set (`scenarios.json`, 10 scenarios) and both complete grading
  rubrics (`graders/`) used in lab 05. The automated CLI harness that would run all of this
  unattended against both runtimes isn't built — lab 05 is a manual, by-hand grading exercise
  instead. `Program.cs` sketches the intended command surface if you want to build the automation
  yourself afterwards.
