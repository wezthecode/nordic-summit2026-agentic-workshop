# Attendee prep — read before 21 September 2026

Welcome. This page is your **only required reading** before the workshop. If you complete the checklist below before you arrive, you'll spend the 4 hours building instead of installing.

## What you're building

A multi-agent timesheet system. A user says "log my time for this week" and an orchestrator agent delegates to two specialist agents — one reads the user's calendar, one writes time entries to Dataverse. You'll build the same thing **twice**: once low-code in **Copilot Studio**, once pro-code in **C# (.NET 10) with the Microsoft Agent Framework SDK** deployed to **Microsoft Foundry**.

Both share the same Dataverse table, the same model-driven app, and the same two first-party Microsoft MCP servers (**Dataverse MCP** and **Work IQ Calendar MCP**).

## Prerequisites checklist

Tick all of these before the workshop. If you can't get one of them, email me (LinkedIn DM is fine) **before** the day — there is a shared sandbox fallback for some items, but it's better if you arrive with your own.

### Identity & licences
- [ ] **Microsoft 365 Copilot licence** assigned to your account — required for Work IQ Calendar MCP. **No fallback for this one** — without it you can't run module 4 hands-on.
- [ ] **Power Platform developer environment** you can build in (a Microsoft 365 Developer subscription works)
- [ ] **Azure subscription** with rights to create a **Microsoft Foundry** resource. *Fallback available* — a shared sandbox will be provided if your corporate subscription can't provision in the workshop window.

### Local tooling
- [ ] **VS Code** (latest)
- [ ] **.NET 10 SDK** installed (`dotnet --version` returns 10.x)
- [ ] **Azure CLI** signed in (`az login`)
- [ ] **Power Platform CLI** (`pac auth list` works)
- [ ] **Git**

### Skills (self-check)
- [ ] You've built at least **one Copilot Studio agent** before, or one production Power Automate flow
- [ ] You can read and edit **C#** without a translator — you don't need to be a daily C# dev, but `Task`, `async`, and `var` should be familiar
- [ ] You're comfortable in a terminal

## What you do NOT need

- Any prior experience with **Microsoft Foundry** — we cover it module 1 and 4
- Any prior experience with the **Microsoft Agent Framework SDK** — same
- Any prior experience with **MCP** — we cover the concepts in module 1
- Any custom plugins, Azure Functions, or Service Bus — these are deliberately out of scope

## Day-of logistics

- **Date:** 21 September 2026
- **Location:** Billund, Denmark — see Nordic Summit 2026 venue confirmation in your ticket
- **Length:** 4 hours
- **Bring:** Laptop, charger, a power adapter for Danish sockets if you need one
- **Wi-Fi:** Provided by the venue. Personal hotspot recommended as backup; Foundry deployments are bandwidth-light but Dataverse solution imports aren't

## Repo

The reference repo (working name `agentic-timesheet-workshop`) will be public **on the morning of 21 September** at: `[URL to be confirmed by 1 Sept]`.

You don't need to clone it before you arrive. We'll do that together in module 1.

## Questions before the day

LinkedIn: <https://www.linkedin.com/in/wesley-nell-4175bb95/>

See you in Billund.
