# Graduated Autonomy (per-action trust ladder)

- **Status**: Planned
- **Priority**: P1 - turns per-action approval into a ramp instead of an all-or-nothing gate; honest answer to agentic-AI trust
- **Effort**: M
- **Category**: AI differentiation

## Why

Every write tool call is `Suggested` and gated on dispatcher approval today, with no exception -
that is deliberate (see [ai-dispatch.md](../ai-dispatch.md)) and this doc does not propose changing
the default. But dispatchers who'd happily auto-approve routine load assignments still want a human
gate on spending money (booking load-board freight). Per-action-type trust would let a tenant opt
specific action types out of the per-call approval requirement once they have a long clean approval
streak - converting approval history into earned trust, not reintroducing an unattended mode.

## What to build

- Trust stats per write-tool per tenant, computed from existing `AgentDecision` history (approvals, rejections, streak). Write tools are the ones with `IsWrite == true` on their `AgentToolDefinition`.
- Auto-approve rule: action type auto-executes after N consecutive approvals with zero rejections (N tenant-configurable, default ~25); one rejection resets the streak and re-gates it.
- Undo window: auto-approved actions land in a "recently auto-approved" feed in `tms-portal/pages/ai-dispatch/` with one-click revert for X minutes (revert = existing un-assign/cancel commands).
- Risk tiers: `book_loadboard_load` (spends money) never auto-approves without explicit opt-in, regardless of streak.
- Decision processor change: `AgentDecisionProcessor` consults per-action trust before defaulting to `Suggested`, instead of always suggesting.

## Acceptance

A tenant with 25 straight approvals of `assign_load_to_truck` sees new assignments execute immediately (flagged auto-approved, revertible), while `create_trip` suggestions still await approval.

## Notes

_(add dated implementation notes here)_
