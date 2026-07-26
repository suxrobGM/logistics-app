# Drayage / Intermodal Vertical

- **Status**: Planned
- **Priority**: P1 - Alvys/DataTruck/Truckbase/Toro all ignore drayage; we already have the entities (ISO 6346 containers, UN/LOCODE terminals)
- **Effort**: L
- **Category**: Market expansion

## Why

Drayage carriers (port/rail container moves) are poorly served: they juggle demurrage/per-diem
deadlines, terminal appointments, and chassis - none of which generic small-carrier TMSs model.
We're halfway there: `Entities/Container/`, `Entities/Terminal/`, ContainerTruck type, and
load↔container↔terminal links already exist.

## What to build

- **Demurrage & per-diem clocks**: last-free-day and container-return deadlines on `Container`/load; countdown badges in `tms-portal/pages/containers/` and on the dispatch board; escalating notifications (existing notification infra) as deadlines approach.
- **Deadline-aware dispatch**: agent priority rule - loads nearing last-free-day jump the queue (`AIDispatchSystemPrompt` + expose deadlines in `get_unassigned_loads` output). Pairs with [ai-container-tools](ai-container-tools.md).
- **Terminal appointments**: appointment window entity per load stop; later, terminal-system integrations (eModal etc.) - start with manual entry + reminders.
- **Chassis tracking**: chassis as an asset (owned/pool/rented, per-diem rate) linked to container moves.
- **Street-turn matching**: detect when an import empty can cover a nearby export booking instead of returning to the terminal - quick-win optimization drayage dispatchers love; good agent tool candidate.
- Billing: per-diem/demurrage/chassis charges as invoice line items (existing `InvoiceTaxLine`/totals recalc handles the math).

## Acceptance

A drayage tenant sees last-free-day countdowns on every container, gets escalating warnings, and the AI agent prioritizes a container one day from demurrage over a load with slack - citing the deadline in its reasoning.

## Notes

_(add dated implementation notes here)_
