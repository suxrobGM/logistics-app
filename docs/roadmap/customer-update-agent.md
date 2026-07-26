# AI Customer Update Agent

- **Status**: Planned
- **Priority**: P1 - DataTruck sells this as "AI Updater" at $99/mo; for us it's mostly wiring of existing pieces
- **Effort**: M
- **Category**: Wedge product

## Why

Check calls and "where's my load?" emails eat dispatcher hours. We already have live GPS tracking
(`TrackingHub`, `TripTrackingService`), public tracking links (`Entities/TrackingLink.cs`), a customer
portal, email infra (Resend + Fluid templates), and load status domain events. An agent that narrates
the journey to the customer automatically is assembly, not invention.

## What to build

- Event-driven notifications: load status domain events (picked up, in transit, delayed, delivered) → templated email/SMS to the customer contact with the public tracking link. Template-first (no LLM) for routine updates - cheap and deterministic.
- LLM layer for the non-routine: delay explanations drafted from context (ETA slip, reason) - reviewed via decision approval flow until trusted ([ai-graduated-autonomy](ai-graduated-autonomy.md)); pairs with [ai-exception-sentinel](ai-exception-sentinel.md) drafts.
- Customer portal chat: "where's my load?" answered by a read-only agent grounded in tracking + load data, scoped strictly to that customer's loads (CustomerUser identity), using restricted registry tools.
- Per-customer notification preferences (events, channel, frequency) on the customer entity.
- SMS channel: add a Twilio-style provider in `Infrastructure.Communications/` (new port in Abstractions).
- `TenantFeature.CustomerUpdates` flag; marketing line: "included, not a $99/mo add-on."

## Acceptance

Customer receives an automatic pickup confirmation with tracking link at dispatch, a delay notice when ETA slips past the appointment window, and gets a correct portal-chat answer to "where is my shipment?" - with zero dispatcher involvement.

## Notes

_(add dated implementation notes here)_
