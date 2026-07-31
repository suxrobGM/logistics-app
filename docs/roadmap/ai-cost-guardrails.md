# AI Cost Guardrails

- **Status**: Done
- **Priority**: P0 - do before launch; the one pricing risk that could actually hurt: Enterprise was "unlimited AI" at $169 base while DataTruck prices AI alone at $399/mo
- **Effort**: S
- **Category**: Launch hygiene

## Why

Starter included 500 AI request-units/week at $29 base; Enterprise quota was `null` (unlimited -
`SubscriptionPlanSeeder`, `WeeklyAIRequestQuota`). If a heavy user's real LLM spend exceeds ~30% of
plan price, the plan loses money. Enterprise + MCP access + unlimited quota is also scriptable abuse
surface.

## What to build

- **Measure first**: pull real average cost/session from `LlmPricing` token accounting across existing sessions (per model tier). Decide quotas from data, not vibes. Target: weekly quota × avg cost/unit ≤ ~30% of plan price.
- **Cap Enterprise**: replace `null` quota with a high soft cap (5,000 units/week chosen) + metered overage via the existing Stripe overage path (`GetOverageBillingUnits`, `IsOverage` on sessions). Update seeder and plan marketing copy consistently - "unlimited" gone everywhere, tenant-facing copy stays qualitative (no quota numbers).
- **Per-tenant spend alerting**: admin-portal report of token spend vs. subscription revenue per tenant; alert threshold when a tenant's LLM cost crosses X% of their plan price.
- **Rate-limit MCP-originated sessions** separately (already 100 req/min per key - verify it also debits quota units, not just request counts).
- If Starter economics don't work: lower Starter quota (seeder one-liner) or route Starter to the cheapest model tier via a plan→model-tier hint (currently the model is global-only - would be a small architecture change; avoid unless needed).

## Acceptance

No plan can lose money on AI at full quota utilization with the current global model; an admin can see per-tenant AI margin at a glance; "unlimited" no longer appears anywhere.

## Notes

- **2026-07-30** - Implemented:
  - Quotas: Starter 300, Pro 1,500, Enterprise 5,000/week (soft cap, was unlimited) - worst-case full-quota cost stays near plan revenue.
  - Dispatch overage is real: `AIDispatchService.RunAsync` sets `IsOverage` at execution time; billed-not-blocked at $0.10/$0.20 per session. Copilot keeps its hard block.
  - `UpdatePlanAsync` + `StripeSeeder` reconcile the metered price and swap subscription items (`SyncAIOverageItemAsync`).
  - Admin margin report on tenant-quotas: revenue vs. 30-day LLM cost, highlighted ≥30%.
  - MCP left quota-free by decision (callers bring their own LLM; rate limit + feature gates bound abuse).
  - "Unlimited" gone from AI copy; quota numbers never shown to tenants.
- **2026-07-31** - Superseded by cost-based budgets: the request-unit quota and tier multipliers
  were replaced with `SubscriptionPlan.WeeklyAIBudgetUsd` (Starter $5 / Pro $25 / Enterprise $75
  per week) metered directly on `AgentSession.EstimatedCostUsd` (all statuses count). Overage now
  bills `ceil(session cost × 3 / $0.10)` Stripe units via `AIOverageBilling` - margin tracks real
  spend automatically, so the "quota economics" re-check when switching models is no longer needed.
- **2026-07-31 (later)** - Overage UX: the accrued billed amount became tenant-visible
  (`AIQuotaStatusDto.OverageChargesUsd`, shown in the quota widget and copilot notice), and owners
  got a per-tenant hard stop (`TenantSettings.BlockAIOverage`) that pauses both surfaces at the
  budget instead of billing (`AI_BUDGET_REACHED`). `AIOverageBilling` moved to
  `Application.Abstractions/Payments/Stripe/` so the quota service and Stripe metering share one
  formula.
