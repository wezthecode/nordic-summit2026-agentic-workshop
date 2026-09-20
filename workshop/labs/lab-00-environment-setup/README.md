# Lab 00 — Environment setup & verification

**Time budget:** 15 min hands-on (runs during the 30-min Foundations module 1 slides)
**Prereq:** Everything on the [attendee-prep checklist](../../attendee-prep/README.md)

## Goal

Confirm every tool, licence, and subscription works **before** module 2 starts. By the end of this lab you should have:

- A signed-in `pac` CLI pointing at your Power Platform dev environment
- A signed-in `az` CLI pointing at the Azure subscription you'll deploy Foundry to
- A working `dotnet` install on .NET 8
- Confirmation that your account has an M365 Copilot licence assigned

## Steps

> **TODO (author):** flesh out exact commands once the shared Foundry sandbox details are finalised (see workshop open questions).

### 1. Power Platform CLI

```bash
pac auth create --environment <YOUR-DEV-ENV-URL>
pac auth list
```

Expected: one auth profile, marked active. If not — see troubleshooting below.

### 2. Azure CLI

```bash
az login
az account set --subscription "<YOUR-SUB-NAME-OR-ID>"
az account show
```

Expected: your name + tenant + subscription ID.

### 3. .NET 8

```bash
dotnet --version
```

Expected: `8.x.y`. If you have multiple .NET SDKs, that's fine as long as 8 is present.

### 4. M365 Copilot licence check

> **TODO (author):** add the exact Microsoft 365 admin centre URL / Graph call attendees should use to self-verify.

### 5. Clone the workshop repo

```bash
git clone <REPO-URL>
cd agentic-timesheet-workshop
```

### 6. Smoke test

Run the smoke-test script (`scripts/smoke-test.sh` — to be written):

```bash
./scripts/smoke-test.sh
```

It calls `pac whoami`, `az account show`, `dotnet --info`, and a Graph endpoint to confirm Copilot licence. Green ticks across the board = ready for lab 01.

## Troubleshooting

> **TODO (author):** populate after first dry-run — collect the actual errors people hit, don't pre-invent them.

## Done?

Move to [lab-01-dataverse-foundation](../lab-01-dataverse-foundation/).
