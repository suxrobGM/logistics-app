# AI Cost Guardrails

- **Status**: Planned
- **Priority**: P0 - do before launch; the one pricing risk that could actually hurt: Enterprise is "unlimited AI" at $169 base while DataTruck prices AI alone at $399/mo
- **Effort**: S
- **Category**: Launch hygiene

## Why

Starter includes 500 AI request-units/week at $29 base; Enterprise quota is `null` (unlimited -
`SubscriptionPlanSeeder`, `WeeklyAIRequestQuota`). If a heavy user's real LLM spend exceeds ~30% of
plan price, the plan loses money. Enterprise + MCP access + unlimited quota is also scriptable abuse
surface.

## What to build

- **Measure first**: pull real average cost/session from `LlmPricing` token accounting across existing sessions (per model tier). Decide quotas from data, not vibes. Target: weekly quota × avg cost/unit ≤ ~30% of plan price.
- **Cap Enterprise**: replace `null` quota with a high soft cap (e.g. 10,000 units/week) + metered overage via the existing Stripe overage path (`GetOverageBillingUnits`, `IsOverage` on sessions). Update seeder, website `pricing.ts` ("unlimited usage" → "10,000 requests/week, overage metered"), and plan marketing copy consistently.
- **Per-tenant spend alerting**: admin-portal report of token spend vs. subscription revenue per tenant; alert threshold when a tenant's LLM cost crosses X% of their plan price.
- **Rate-limit MCP-originated sessions** separately (already 100 req/min per key - verify it also debits quota units, not just request counts).
- If Starter economics don't work: lower Starter quota (seeder one-liner) or route Starter to the cheapest model tier via a plan→model-tier hint (currently the model is global-only - would be a small architecture change; avoid unless needed).

## Acceptance

No plan can lose money on AI at full quota utilization with the current global model; an admin can see per-tenant AI margin at a glance; "unlimited" no longer appears anywhere.

## Notes

_(add dated implementation notes here)_
