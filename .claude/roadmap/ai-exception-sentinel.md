# AI Exception Sentinel (event-driven agent)

- **Status**: Planned
- **Priority**: P1 - flips the agent from "runs when asked" to "notices problems before the customer does"; exception handling is the real dispatch pain
- **Effort**: L
- **Category**: AI differentiation

## Why

Today the agent runs on demand or via `AIDispatchSessionJob` schedule. Real money is lost on
exceptions: a driver running out of HOS mid-trip, a late pickup, detention building at a receiver.
All the trigger data already flows through us (ELD webhooks, GPS tracking, HOS violations).

## What to build

- Trigger layer: subscribe to domain events / webhook processing (`ProcessEldWebhook` stamps HOS data; `TripTrackingService` has positions) and detect exception conditions: HOS shortfall vs. remaining planned route, ETA slip beyond appointment window, dwell time exceeding threshold at a stop.
- Scoped agent session type: reuse `AIDispatchService.RunAsync` with `Instructions` describing the specific exception (field already exists on `AIDispatchRequest`), constrained to the affected trip.
- Output = suggestions via the existing decision approve/reject flow + a drafted customer notification (hook into [customer-update-agent](customer-update-agent.md)).
- Debounce/dedupe per trip (don't spawn a session per GPS ping); respect tenant quota; new `TenantFeature` flag, Professional+.
- Notify dispatcher via existing `NotificationHub` + Firebase push: "AI flagged trip #123: driver will run out of hours 40 mi short. Proposed swap ready for review."

## Acceptance

Simulated HOS shortfall on an active trip produces, without human prompting, a dispatcher notification containing a viable replan suggestion and a drafted delay notice - within minutes of the triggering event.

## Notes

_(add dated implementation notes here)_
