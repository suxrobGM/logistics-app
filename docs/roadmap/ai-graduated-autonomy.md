# Graduated Autonomy (per-action trust ladder)

- **Status**: Planned
- **Priority**: P1 - turns the HumanInTheLoop → Autonomous leap into a ramp; honest answer to agentic-AI trust
- **Effort**: M
- **Category**: AI differentiation

## Why

`AIDispatchMode` is binary. Dispatchers who'd happily auto-approve routine load assignments still
want a human gate on spending money (booking load-board freight). Per-action-type trust converts
approval history into earned autonomy - and differentiates against competitors overselling full autonomy.

## What to build

- Trust stats per write-tool per tenant, computed from existing `AIDispatchDecision` history (approvals, rejections, streak). Write tools are enumerated in `AIDispatchDecisionProcessor.WriteTools`.
- Auto-approve rule: action type auto-executes after N consecutive approvals with zero rejections (N tenant-configurable, default ~25); one rejection resets the streak and re-gates it.
- Undo window: auto-approved actions land in a "recently auto-approved" feed in `tms-portal/pages/ai-dispatch/` with one-click revert for X minutes (revert = existing un-assign/cancel commands).
- Risk tiers: `book_loadboard_load` (spends money) never auto-approves without explicit opt-in, regardless of streak.
- Decision processor change: instead of mode-wide branching, consult per-action trust when mode is HumanInTheLoop.

## Acceptance

A tenant with 25 straight approvals of `assign_load_to_truck` sees new assignments execute immediately (flagged auto-approved, revertible), while `create_trip` suggestions still await approval.

## Notes

_(add dated implementation notes here)_
