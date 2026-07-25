# TMS-wide AI Copilot

- **Status**: Planned
- **Priority**: P1 - generalizes the agent from "dispatch feature" to "platform"; every tool added also lands on the MCP server for free
- **Effort**: L
- **Category**: AI differentiation

## Why

The tool registry + agent loop + quota/billing stack is a platform currently pointed at one job.
A chat panel in the TMS portal ("invoice all delivered loads from last week and send payment links",
"which trucks are due for service before their next dispatch?") multiplies the value of everything
already built. DataTruck sells narrower AI as $399+$99/mo add-ons.

## What to build

- Tool expansion in `Infrastructure.AI/Tools/` (via `add-dispatch-tool` skill): invoices (create/send/status), payment links, expenses query, maintenance due, customer lookup, load history/search. Reuse existing command/query handlers - tools should be thin MediatR dispatchers.
- Write tools go through the same `AIDispatchDecisionProcessor` suggestion flow (a copilot that sends invoices needs the same approval gate as one that dispatches trucks). Remember: every write tool must be added to the `WriteTools` HashSet.
- Copilot session type: conversational multi-turn (user replies mid-session) vs. the current fire-and-forget dispatch run - extend `AIDispatchService` or add a sibling service sharing the loop internals; stream via existing SignalR broadcast.
- UI: persistent chat drawer in tms-portal shell (`ui-drawer`), scoped by user role/permissions - tools must respect the caller's `Permission` constants, not run as super-tenant.
- Quota: copilot turns consume the same `AIQuotaService` units.
- System prompt: new builder in `Prompts/` (don't overload the dispatch prompt).

## Acceptance

From the chat panel, "invoice load #123 and send the customer a payment link" produces a suggested invoice + payment link pending approval, then executes on approve - with the action visible in the decision audit trail.

## Notes

_(add dated implementation notes here)_
