# EDI Integration (204/214/210/990)

- **Status**: Planned
- **Priority**: P2 - required for direct shipper contracts and enterprise brokers; not needed for load-board carriers
- **Effort**: XL
- **Category**: Table stakes (upper market)

## Why

EDI unlocks direct shipper relationships (load tenders 204, status updates 214, invoices 210).
Alvys ships native EDI as a headline feature; DataTruck charges $499/connection. Carriers below
~20 trucks rarely need it - hence P2, revisit when customers start losing shipper bids over it.

## What to build

- Decide build-vs-buy first: an EDI VAN/API partner (Orderful, Stedi) turns X12 into JSON webhooks - that fits our existing `WebhookController` + `Application/Modules/Integrations/Webhooks/Commands/` pattern (use the `add-webhook-handler` skill) and avoids writing an X12 parser. Recommend buy.
- Inbound 204 tender → draft `Load` + accept/decline workflow (map to existing load creation commands).
- Outbound 214 status updates driven by existing load status domain events (`Domain/Events/`).
- Outbound 210 invoice from `LoadInvoice`.
- Per-trading-partner config entity; start with one anchor customer's shipper.

## Acceptance

A 204 tender from a test trading partner becomes an acceptable draft load; accepting it emits a 990 and subsequent status changes emit 214s automatically.

## Notes

_(add dated implementation notes here)_
