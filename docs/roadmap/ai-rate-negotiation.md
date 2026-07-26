# AI Rate Negotiation

- **Status**: Planned
- **Priority**: P2 - head-to-head answer to DataTruck's flagship AI Dispatcher ($399/mo add-on); bundled in our plans
- **Effort**: L
- **Category**: AI differentiation

## Why

DataTruck's AI Dispatcher "negotiates rates and books loads in seconds." We already search and book
load boards (`SearchLoadBoardTool`, `BookLoadBoardLoadTool`) and compute profitability
(`CalculateAssignmentMetricsTool`). The missing piece is the negotiation loop with a floor price.

## What to build

- Floor computation: rate-per-mile floor from assignment metrics (deadhead, HOS cost, tenant-configured minimum RPM). Dispatcher sets bounds per lane or globally in ai-dispatch settings.
- Negotiation channel: email first (broker emails are on load-board listings; we have Resend + Fluid templates in `Infrastructure.Communications/Email/`). Agent drafts counter-offer; replies parsed into the session.
- Gate on [broker-credit-check](broker-credit-check.md) - never negotiate with a broker below the credit threshold.
- Every outbound offer is a **write action** through the decision approval flow until the tenant graduates it via [ai-graduated-autonomy](ai-graduated-autonomy.md). Spending-money actions default to human-gated.
- Negotiation thread UI inside the dispatch session view (`tms-portal/pages/ai-dispatch/`).

## Acceptance

For a load-board listing below floor, the agent produces a credit-checked, dispatcher-approved counter-offer email; an accepting reply books the load via the existing booking tool.

## Notes

_(add dated implementation notes here)_
