# Low-code reference (lab 02)

This folder will hold the exported Copilot Studio solution + managed solution zip used as the recovery artefact during the workshop.

## Contents (populated during rehearsal week)

- `AgenticTimesheet_unmanaged_<version>.zip` — full solution export including the three agents
- `AgenticTimesheet_managed_<version>.zip` — managed equivalent for clean import
- `README.md` — import steps + known-good environment configuration

## Import (workshop-day rescue path)

If an attendee's lab 02 build is broken beyond debugging in the time budget, import the managed solution into their environment:

```
pac solution import --path AgenticTimesheet_managed_<version>.zip --activate-plugins
```

…then continue from lab 03. We don't lose anybody to a configuration accident in lab 02.
