# AI Rate Negotiation

- **Status**: Done (email channel)
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

### 2026-08-21 - Email channel shipped

See [broker-email-negotiation.md](../broker-email-negotiation.md) for the pipeline, the threat model
and the Resend setup.

What landed:

- `TenantFeature.AIRateNegotiation`, Professional and above. New `Permission.Negotiation.View/Manage`.
- Per-lane floors (`LaneRateFloor`) from day one rather than a global number, resolved exact lane →
  origin-state-to-anywhere → anywhere-to-destination-state → `TenantSettings.DefaultRateFloorPerMile`.
  No floor means the agent refuses to negotiate.
- Tools `get_rate_floor`, `get_negotiation_thread`, `propose_counter_offer`; `book_loadboard_load`
  gained `negotiated_total_rate`.
- Replies arrive on `offer-{token}@{sender domain}` and route through a master-DB `InboundEmailRoute`
  row, so no tenant id is exposed in an address and a thread can be revoked.
- Three rounds per listing, a 48-hour reply window per outbound message, and an hourly sweep that
  expires lapsed threads and revokes their reply addresses.

Deliberately out of scope: voice/phone negotiation, attachment ingestion (rate confirmations), and
LLM extraction of the broker's number into `ProposedTotalRate` - the amount is read by a human from
the thread today. Auto-approval of offers stays gated on
[ai-graduated-autonomy](ai-graduated-autonomy.md).
