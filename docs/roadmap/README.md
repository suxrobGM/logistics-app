# Product Roadmap

Features that are planned but not built. One file per feature. When you start, finish or drop one,
update its **Status** line, add a dated note in its **Notes** section, and keep this table in sync.
When a feature ships, document it in the feature docs (`docs/*.md`) and [the feature map](../../.claude/feature-map.md),
then delete its file here and move any leftovers to _Open follow-ups_ below.

**Statuses**: `Planned` · `In Progress` · `Blocked`
**Priority**: P0 = loses deals today · P1 = differentiator, build soon · P2 = strategic bet
**Effort**: S (< 1 wk) · M (1–3 wk) · L (1–2 mo) · XL (2+ mo)

| Feature | Priority | Effort | Status |
| --- | --- | --- | --- |
| [Drayage / intermodal vertical](drayage-vertical.md) | P1 | L | In Progress |
| [Car hauler load boards](car-hauler-load-boards.md) | P1 | M | Blocked (vendor access) |
| [EU market readiness](eu-market-readiness.md) | P1 | M | Planned |
| [Same-day carrier pay](same-day-pay.md) | P1 | L | Planned |
| [AI eval harness](ai-eval-harness.md) | P1 | M | Planned |
| [Predictive maintenance](predictive-maintenance.md) | P2 | L | Planned |
| [MCP client (outbound)](mcp-client-integrations.md) | P2 | L | Planned |
| [Voice driver assistant](ai-voice-driver-assistant.md) | P2 | XL | Planned |

## Open follow-ups from shipped work

- **QuickBooks Online**:
  - An end-to-end run against a sandbox company before the first pilot.
  - Not supported yet: QuickBooks Desktop, or QBO sales tax codes (tax is sent as a TotalTax override).
  - Paid driver-pay bills aren't marked paid (no BillPayment), and nothing flows back from QBO.
  - Token refresh is serialized per API instance only.
- **Fuel cards**: the WEX and EFS API contracts are best effort until a partner sandbox confirms them. Comdata is deferred.
- **Factoring**:
  - Advance and reserve bank transfers aren't pushed to QuickBooks.
  - There's no per-invoice factoring timeline (status badge only).
- **Owner-operators**:
  - There's no owner pay (draw) path.
  - The driver app has no earnings or settlement screen and no cash advance request.
  - Signup isn't self-serve: `POST /tenants` needs an admin, so `/owner-operators` routes to Request a Demo.
- **AI copilot**: the live acceptance run ("invoice load X and send a payment link") against a configured LLM key.
- **Pricing**:
  - Annual billing is deferred until real companies can test the discount.
  - Plan data is still edited by hand in three places (`SubscriptionPlanSeeder`, the website `pricing.ts`, Stripe Prices).
- **Terminals** have no coordinates, so `get_terminal_info` can't feed `calculate_distance` (see [drayage](drayage-vertical.md)).
