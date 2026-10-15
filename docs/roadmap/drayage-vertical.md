# Drayage / Intermodal Vertical

- **Status**: In progress (first slice shipped 2026-10-14)
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

- **2026-10-14 - first slice shipped (pilot).** All of it lives on the `Container` as the current move
  (`Container.Drayage.cs`), gated by `TenantFeature.IntermodalContainers`:
  - Clocks: `LastFreeDay` + `DemurrageRatePerDay` stop at `PickedUpAt` (stamped when status leaves
    `AtPort`); `EmptyReturnDeadline` + `PerDiemRatePerDay` stop at `ReturnedAt` or `StreetTurnedAt`.
    `ContainerFreeTimeCalculator` counts calendar days on the UTC date (weekends count, the deadline
    day is free, any part of a day is a day). Shown as grey/amber/red tags on the container list,
    container details and load details.
  - Rates are per container, entered from the line's or terminal's tariff - they vary by line,
    terminal and box size, so a single per-customer or per-line rate would be wrong as often as right.
    A rate card is the follow-up if pilots ask for it.
  - Holds (customs, freight, line, other) and available-for-pickup time, manual entry.
  - Appointments: pickup and return number + window on the container move, not on `TripStop` - trip
    stops only exist once a trip is planned and are rebuilt with it, while the appointment belongs to
    the box at the terminal.
  - Chassis: number + pool (`ChassisProvider`: own, DCLI, TRAC, Flexi-Van, other) + daily rate on the
    move; days from outgate to return or street turn, both ends counted, own chassis never charged.
    Not yet linked to the `Trailer` asset (TrailerType `Chassis`) - that is the follow-up.
  - Empty return terminal; street turn (`POST /containers/{id}/streetturn`) records the export
    booking and stops per diem. No matching engine.
  - Leaving `Returned` starts a new move and clears the per-move fields (line and rates stay).
  - Billing: `POST /loads/{id}/drayagecharges` writes Demurrage / PerDiem / Chassis lines (rate x days)
    onto the load invoice and replaces them on re-run. `Storage` exists as a line type for manual entry.
  - Alerts: `ContainerDeadlineAlertJob` (daily 12:00 UTC) notifies when a running clock is due today,
    tomorrow, or overdue - repeated daily with the growing charge while overdue.
  - AI: `get_container_status` returns the clocks, holds, appointments, chassis and street turn;
    `get_unassigned_loads` carries `container_last_free_day`, `container_days_until_last_free_day` and
    `container_on_hold`, and the dispatch prompt ranks loads at one day or less first.
- **Not done yet**: terminal-system feeds (eModal, Port Optimizer and similar are partner-gated - holds,
  availability and appointments stay manual until a partnership exists), street-turn matching,
  dispatch-board countdown badges, chassis-as-asset, tenant-timezone day boundaries, and showing
  chassis/appointments in the driver app (its load screen shows only the container number and type).
