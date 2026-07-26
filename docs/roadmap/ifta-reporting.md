# IFTA Fuel Tax Reporting

- **Status**: Done
- **Priority**: P0 - quarterly legal obligation for interstate carriers; TMSs without it get filtered out immediately
- **Effort**: M
- **Category**: Table stakes

## Why

IFTA requires quarterly per-jurisdiction mileage + fuel-purchase reporting. Competitors bundle it.
We already ingest the hard half (mileage by state via ELD GPS data); fuel purchases arrive with
[fuel-card-integrations](fuel-card-integrations.md) or manual entry via existing `TruckExpense`.

## What to build

- Jurisdiction mileage: derive state/province crossings from ELD location history (`Infrastructure.Integrations.Eld` providers already pull vehicle locations). Store per-truck per-jurisdiction miles per quarter.
- Fuel purchases: extend `Entities/Expense/TruckExpense` with gallons + jurisdiction, or a dedicated `FuelPurchase` entity (`migration-creator` skill).
- Quarterly IFTA report: per-jurisdiction summary (miles, gallons, tax due) with CSV + PDF export via `Infrastructure.Documents/Pdf/` (QuestPDF, same as invoices).
- Hangfire job to snapshot quarters; reminder notifications before filing deadlines (mirror `MaintenanceReminderJob`).
- Page: `tms-portal/pages/reports/` or new `pages/ifta/`.
- Note EU angle: same mileage-by-jurisdiction engine later powers EU road-toll/km-charge reporting - keep jurisdiction logic country-agnostic.

## Acceptance

For a quarter of ELD + fuel data, the report matches per-jurisdiction miles/gallons a dispatcher would compute manually, exportable as PDF/CSV for filing.

## Notes

- **2026-07-16**: Shipped on `feat/fuel-integrations`. Location history: `TruckLocationHistory` breadcrumbs (4-yr audit retention, BRIN-indexed, purged by the quarter-close job) + `TruckJurisdictionMileage` daily rollups, both written by `TruckLocationRecorder` (odometer-delta-or-haversine distance, 90 mph teleport guard, gaps still accrue) - ELD sync and driver-app GPS paths now route through it (also fixed a pre-existing swapped lat/lng bug that made TT ELD GPS sync throw). Jurisdiction snapping: offline NetTopologySuite point-in-polygon over embedded Natural Earth admin-1 boundaries (US+CA = the IFTA jurisdictions; EU = add polygons later). `IftaReportService`: miles from rollups, gallons from fuel expenses (fuel-card imports carry jurisdiction), fleet MPG, tax due from master-DB `IftaTaxRate` (quarterly seed JSON - **recurring ops task**; missing rates are flagged, never guessed; IN/KY/VA surcharge on all taxable gallons). Quarter-close job snapshots closed quarters immutably (`IftaQuarterSnapshot`); filing reminders 30/14/7/3/1 days before Apr 30/Jul 31/Oct 31/Jan 31. UI: `reports/ifta` (quarter picker, PDF via QuestPDF, client CSV) gated on `TenantFeature.Ifta` (Professional+). Note: first fileable quarter is the first full quarter after deploy - no GPS backfill exists.
- **Trap - tax rate uniqueness is application-enforced only**: (Year, Quarter, Jurisdiction) uniqueness for `IftaTaxRate` cannot be a DB unique index because complex-type members can't participate in one (see `IftaTaxRateEntityConfiguration`). `IftaTaxRateUniqueness.FindConflictAsync` in `Modules/Compliance/Ifta/TaxRates/` is the only thing protecting the invariant, so **every write path must probe it** - `CreateIftaTaxRateHandler` and `UpdateIftaTaxRateHandler` both do (pass `excludeId` on update so a rate doesn't conflict with itself). A duplicate silently changes computed tax due, because `IftaReportService` resolves rates with `GroupBy(...).First()`.
- **Trap - `IftaQuarterSnapshot` has no Total\* columns**: `ReportJson` (the serialized `IftaReportDto`) is the sole source of truth. Totals are deliberately not denormalized into columns - nothing queries them and a second copy can only drift. Closed quarters are served verbatim from `ReportJson`; only the open quarter is computed live. Read the quarter out of the JSON rather than adding columns.
