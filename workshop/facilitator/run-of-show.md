# Facilitator run-of-show — 21 September 2026

**This is for me, not the attendees.** Times are minutes-into-workshop. Buffer is built in; if a module finishes early, move forward — don't pad.

| Time   | Module | What's happening                                                | What I'm doing                                  | Risk |
|--------|--------|------------------------------------------------------------------|--------------------------------------------------|------|
| 00:00  | 1      | Welcome, story, what we're building today                        | Slides; attendees opening lab-00 in parallel     | Late arrivals — keep welcome to 5 min |
| 00:15  | 1      | Architecture walkthrough                                         | Slide deck + live MDA tour                       | If MDA is slow, switch to screenshots |
| 00:30  | 2      | Lab 01 — Dataverse foundation                                    | Floor-walking; helping with `pac` auth           | `pac` auth fail — have shared sandbox creds ready |
| 01:00  | 3      | Lab 02 — Copilot Studio build                                    | Demo first 10 min, then attendees build          | Work IQ MCP licence gap — keep partner list |
| 02:00  | —      | **15 min break**                                                 | Coffee. Reset.                                   | Run-over from module 3 — eat into break, not module 4 |
| 02:15  | 4      | Lab 03 — Foundry build                                           | Demo deploy first, then attendees                | Foundry provisioning lag — fall back to shared sandbox |
| 03:15  | 5      | Lab 04 — governance comparison                                   | Whiteboard the comparison table together         | None major |
| 03:45  | 6      | Lab 05 — evaluation                                              | Run harness live, discuss numbers honestly       | Eval harness flaky — have pre-canned results.json |
| 04:00  | —      | Wrap, Q&A, repo handoff                                          | Sit on the floor; chat                           | — |

## Pre-day checklist (week of 14 Sept)

- [ ] Shared sandbox alive, 30 attendee slots provisioned, creds in sealed envelopes
- [ ] Solution exports re-tested end-to-end on a fresh tenant
- [ ] Both reference implementations re-deployed, smoke-tested
- [ ] Eval harness run end-to-end and `results.json` captured as the fallback canned output
- [ ] Slide deck final, embargo'd preview screenshots removed
- [ ] Repo public, README final, MIT licence in place
- [ ] Demo gear bag: HDMI/USB-C dongles, backup laptop, ethernet cable, personal hotspot charged

## On-the-day fail-safes

- **Wi-Fi dies** → personal hotspot for me, attendees fall back to shared sandbox over LTE
- **Power Platform region issue** → shared sandbox in a different region pre-provisioned
- **Foundry provisioning timing out** → shared sandbox with pre-deployed agents, attendees inspect rather than build
- **Work IQ MCP preview removed / broken** → pre-recorded video of that segment + slide explanation of what attendees would have done

## Things I will explicitly say at the start

1. "This is opinionated, not authoritative. I'm not Microsoft."
2. "Preview features in this stack are changing weekly. The repo is dated; what you build today might look different next month. That's fine — the pattern is what we're teaching."
3. "There will be no plugin. There will be no Azure Function. There will be no custom MCP. If you came expecting those, the repo retired those modules deliberately and there's a write-up on why."
4. "Two runtimes. Same problem. By the end, you'll have an opinion."
