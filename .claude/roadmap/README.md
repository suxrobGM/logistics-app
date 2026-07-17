# Product Roadmap

Feature backlog from the July 2026 competitive review (vs. Alvys, DataTruck, Truckbase, Toro, Rose Rocket).
One file per feature. When you start/finish/drop a feature, update its **Status** line and add dated notes
in its **Notes** section. Keep this index's Status column in sync.

**Statuses**: `Planned` · `In Progress` · `Done` · `Dropped`
**Priority**: P0 = loses deals today · P1 = differentiator, build soon · P2 = strategic bet
**Effort**: S (< 1 wk) · M (1–3 wk) · L (1–2 mo) · XL (2+ mo)

## Phase 1 — Table stakes (US small-carrier deal-breakers)

| Feature                                             | Priority | Effort | Status  |
| --------------------------------------------------- | -------- | ------ | ------- |
| [QuickBooks sync](quickbooks-sync.md)               | P0       | M      | Done    |
| [IFTA fuel tax reporting](ifta-reporting.md)        | P0       | M      | Done    |
| [Fuel card integrations](fuel-card-integrations.md) | P0       | M      | Done    |
| [Factoring integration](factoring-integration.md)   | P0       | M      | Planned |
| [Broker credit check](broker-credit-check.md)       | P0       | S      | Done    |
| [EDI integration](edi-integration.md)               | P2       | XL     | Planned |

## Phase 2 — AI differentiation (extend the existing agent platform)

| Feature                                                | Priority | Effort | Status  |
| ------------------------------------------------------ | -------- | ------ | ------- |
| [AI preference flywheel](ai-preference-flywheel.md)    | P1       | M      | Planned |
| [AI exception sentinel](ai-exception-sentinel.md)      | P1       | L      | Planned |
| [Graduated autonomy](ai-graduated-autonomy.md)         | P1       | M      | Planned |
| [AI eval harness](ai-eval-harness.md)                  | P1       | M      | Planned |
| [Container & terminal AI tools](ai-container-tools.md) | P1       | S      | Planned |
| [TMS-wide AI copilot](ai-tms-copilot.md)               | P1       | L      | Planned |
| [AI rate negotiation](ai-rate-negotiation.md)          | P2       | L      | Planned |
| [Voice driver assistant](ai-voice-driver-assistant.md) | P2       | XL     | Planned |
| [MCP client (outbound)](mcp-client-integrations.md)    | P2       | L      | Planned |

## Phase 3 — Wedge products (attack competitor economics)

| Feature                                              | Priority | Effort | Status  |
| ---------------------------------------------------- | -------- | ------ | ------- |
| [Same-day carrier pay](same-day-pay.md)              | P1       | L      | Planned |
| [AI customer update agent](customer-update-agent.md) | P1       | M      | Planned |
| [Predictive maintenance](predictive-maintenance.md)  | P2       | L      | Planned |

## Phase 4 — Market expansion

| Feature                                              | Priority | Effort | Status  |
| ---------------------------------------------------- | -------- | ------ | ------- |
| [EU market readiness](eu-market-readiness.md)        | P1       | M      | Planned |
| [Drayage / intermodal vertical](drayage-vertical.md) | P1       | L      | Planned |
| [Owner-operator (solo) mode](owner-operator-mode.md) | P1       | M      | Planned |

## Launch hygiene

| Feature                                         | Priority | Effort | Status  |
| ----------------------------------------------- | -------- | ------ | ------- |
| [AI cost guardrails](ai-cost-guardrails.md)     | P0       | S      | Planned |
| [Pricing page polish](pricing-launch-polish.md) | P1       | S      | Planned |

## Competitive cheat sheet

- **Alvys** (~$150+/mo load-based): 120+ integrations, native EDI, carrier+broker hybrid. Weakness: analytics reliability, support complaints.
- **DataTruck** ($99–499/mo by truck count): AI Dispatcher +$399/mo, AI Updater +$99/mo, TruckGPT load entry, broker credit vetting. Weakness: AI is expensive add-ons; narrower core than ours.
- **Truckbase** (~$290/mo): simple, <50 trucks, outgrown quickly.
- **Toro TMS**: 50–300 trucks, QuickBooks push, no real AI.
- **Rose Rocket**: hybrid carrier/broker, strong customer portal.
- **Our uncontested ground**: MCP server, EU compliance stack (EU 561/2006, VAT, ADR, GDPR, SEPA/iDEAL), intermodal entities, native Stripe Connect payments, AI included in base plans.
