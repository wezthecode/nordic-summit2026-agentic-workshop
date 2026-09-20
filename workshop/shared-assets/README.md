# Shared assets

Single source of truth for things both the Copilot Studio build (lab 02) and the Foundry build (lab 03) consume. If you change something here, you change it once.

## What lives here

```
shared-assets/
├── README.md                       ← you are here
├── dataverse-schema.md             ← THE schema spec (publisher prefix, tables, columns, role)
├── solution/                       ← solution exports — generated, not hand-edited
│   ├── AgenticTimesheet_managed.zip
│   └── AgenticTimesheet_unmanaged.zip
├── sample-data/
│   └── projects.csv                ← 3 seed rows for cre_project
├── prompts/
│   ├── orchestrator.system.md      ← same wording in both runtimes (where the platform allows)
│   ├── calendar-child.system.md
│   └── time-entry-child.system.md
└── eval/
    ├── scenarios.json              ← 10–15 test scenarios used by both runtimes
    ├── graders/
    │   ├── task-success.md         ← did the right Dataverse rows land?
    │   └── conversation-quality.md ← LLM grader rubric
    └── README.md                   ← how to run the harness against each runtime
```

## Build order for these assets

Author in this order — each depends on the previous:

1. **`dataverse-schema.md`** — the spec. Done first because it's the gate.
2. **`sample-data/projects.csv`** — needed for any end-to-end demo to work.
3. **`prompts/*.system.md`** — write the wording once, paste into both CS and Foundry.
4. **Solution export** — only after the schema is built in a real environment and round-tripped.
5. **`eval/scenarios.json` + graders** — once a happy path works in either runtime.

## Why this folder exists

Lab 02 (Copilot Studio) and lab 03 (Foundry) both demo the same multi-agent system, and the 20-min hook session's third demo ("the orchestrator-plus-children Copilot Studio pattern") reuses the lab 02 build directly. Without a shared-assets folder, the builds drift, the comparison becomes apples-to-oranges, and the central claim ("same problem, two runtimes") collapses. This folder is the contract that keeps them honest.
