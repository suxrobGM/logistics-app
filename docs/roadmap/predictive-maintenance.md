# Predictive Maintenance

- **Status**: Planned
- **Priority**: P2 - competitors log maintenance; nobody closes the loop with dispatch
- **Effort**: L
- **Category**: Wedge product

## Why

We uniquely hold DVIR defects (`DvirReport`/`DvirDefect`), ELD telematics (odometer, engine data from
Samsara/Motive/Geotab), maintenance history (`Entities/Maintenance/`), and truck expenses in one schema.
"This truck's defect pattern precedes a breakdown - service it before assigning that 800-mile load"
prevents stranded loads, which is the expensive outcome.

## What to build

- Phase 1 (rules, no ML): risk score per truck from recurring DVIR defect categories, overdue maintenance intervals, mileage since last service, expense spike patterns. Computed by a Hangfire job; stored on/near the Truck entity.
- Surface in dispatch: `get_available_trucks` tool output includes maintenance risk; `AIDispatchSystemPrompt` rule to deprioritize high-risk trucks for long hauls. `CheckDispatchEligibilityTool` gains a soft maintenance warning.
- UI: risk badge on `tms-portal/pages/trucks/` + panel in `pages/maintenance/` explaining contributing factors (explainability is the trust feature).
- Phase 2: LLM pattern analysis over defect narratives ("driver noted brake softness 3 inspections in a row"); Phase 3: actual failure-prediction model if fleet data volume ever justifies it.
- Extend `MaintenanceReminderJob` to fire on risk threshold, not just calendar/mileage.

## Acceptance

A truck with three consecutive brake-related DVIR defects shows elevated risk, appears with a warning in agent tool output, and triggers a maintenance notification - before a breakdown, not after.

## Notes

_(add dated implementation notes here)_
