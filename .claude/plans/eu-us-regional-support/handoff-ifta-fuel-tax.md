# Handoff: IFTA Fuel-Tax Reporting (US / Canada)

> **Priority:** LOW (US/CA-only; not strictly blocking but commonly requested). **Effort:** L (~1 week).
>
> IFTA (International Fuel Tax Agreement) requires US/CA carriers operating in multiple jurisdictions to report quarterly mileage and fuel purchases per state/province. The platform tracks neither today.

## Sequencing

- **Position in overall order:** 11th
- **Depends on:** Plan #9 ([fuel volume units](handoff-fuel-volume-temperature-units.md)) — IFTA aggregates `TruckExpense.Quantity` + `QuantityUnit`. Plan #1 ([VAT engine](handoff-vat-tax-engine.md)) tax-line breakdown pattern is reused.
- **Unblocks:** Larger US fleets that currently use a separate IFTA tool.
- **Why eleventh:** Genuinely US-only. Skip if the EU-launch milestone is the priority; pick up afterward when chasing US enterprise customers.

## Why now

- Larger US fleets won't switch to LogisticsX without it; today they pay separate IFTA software
- Same data (per-jurisdiction miles + fuel) feeds non-IFTA reports too (per-state revenue analysis)
- EU equivalent does not exist — fuel duty is national-level, no inter-state allocation

## Current state

- No `Trip` jurisdiction breakdown
- `TruckExpense` fuel rows have amount but no jurisdiction (after plan #9 they have quantity)
- No quarterly reporting infrastructure

## Backend

### Domain

- New entity `Entities/Ifta/IftaJurisdictionMile`:
  - `TripId`, `Jurisdiction` (US state / CA province as 2-char ISO code), `Miles` (decimal), `EnteredAt`, `ExitedAt`
- New entity `Entities/Ifta/IftaFuelPurchase`:
  - Either link to `TruckExpense` or stand-alone; `Jurisdiction`, `Gallons`, `PricePerGallon`, `TaxRatePerGallon`, `PurchaseDate`, `Vendor`, `ReceiptBlobPath`
- New entity `Entities/Ifta/IftaQuarterlyReport`:
  - `Year`, `Quarter`, `Status` (Draft/Submitted/Filed), `GeneratedAt`, `LineItems` (per-jurisdiction: miles, taxable miles, gallons consumed, gallons purchased, net tax owed)

### Application

- Service `IIftaCalculationService.CalculateQuarterlyReport(year, quarter)`:
  - Aggregate miles per jurisdiction from trips
  - Aggregate fuel purchases per jurisdiction
  - Apply tax rates (loaded from a config / external feed; rates change quarterly)
  - Compute consumed-vs-purchased differential per jurisdiction → tax owed/refund
- `Commands/Ifta/GenerateReport`, `SubmitReport`, `ExportReport`
- New job `Jobs/IftaMileageBreakdownJob` — when a trip closes, compute per-jurisdiction miles from the recorded GPS track:
  - Either with a polygon-in-polygon test (state boundaries shapefile shipped with the app — public-domain TIGER data)
  - Or via Mapbox Map Matching API and reverse-geocoding sample points
- IFTA tax rate loader: scrape the IFTA Inc. quarterly rate sheet, or build manually each quarter

### Infrastructure

- New module `Logistics.Infrastructure.Compliance.Ifta/`:
  - `IftaCalculationService` implementation
  - `IftaRateProvider` (reads from embedded resource or remote feed)
  - `IftaJurisdictionDetector` (geo-fence using state polygons — embed simplified GeoJSON ~1MB)
  - PDF generator for the quarterly report (use existing QuestPDF infrastructure — see [Documents/Pdf/](../../src/Infrastructure/Logistics.Infrastructure.Documents/Pdf/))

### Persistence / migration

- New tenant DB tables: `IftaJurisdictionMiles`, `IftaFuelPurchases` (link or denorm), `IftaQuarterlyReports`, `IftaQuarterlyReportLineItems`
- `TruckExpense.Jurisdiction` (string?) — backfill blank for existing rows; add a UI nudge to fill in for fuel category

## API

- New `IftaController`:
  - `GET /api/ifta/reports?year=&quarter=`
  - `POST /api/ifta/reports/generate?year=&quarter=`
  - `GET /api/ifta/reports/{id}/pdf`
  - `GET /api/ifta/jurisdictions/{tripId}` — show the per-jurisdiction breakdown for a single trip
- Run `bun run gen:api`

## Frontend (Angular)

### TMS portal

- New section `pages/ifta/`:
  - `pages/ifta/quarterly-reports/` — table of quarters; "Generate" button on the current open quarter
  - `pages/ifta/report-detail/` — per-jurisdiction breakdown editable before submit; "Export PDF"
  - `pages/ifta/fuel-purchases/` — list/add fuel purchases with jurisdiction
  - Show only when tenant region is US or CA — feature-flag via `TenantFeature.Ifta`
- `pages/expenses/components/truck-expense-form/` — for fuel category, prompt for jurisdiction (autopopulated from receipt OCR if possible later)
- `pages/trips/trip-detail/` — show jurisdiction breakdown for the trip's miles
- Add to feature flags ([admin-portal/pages/features/](../../src/Client/Logistics.Angular/projects/admin-portal/src/app/pages/features/))

### Mobile (driver app)

- Driver fuel-entry screen — capture jurisdiction (auto-fill from current GPS location) and quantity
- Receipt photo upload (already supported) is critical for IFTA audits

## Tests

- `IftaJurisdictionDetector` — sample of GPS tracks crossing state lines, expected mileage per state
- `IftaCalculationService` — quarterly aggregation produces correct tax-owed totals; refund case (purchased fuel in TX, consumed in CA)
- Snapshot test of generated PDF report

## Acceptance criteria

- [ ] US tenant: fuel purchase entered with jurisdiction shows up in next quarterly report
- [ ] Trip with GPS track crossing TX → NM → AZ produces three IftaJurisdictionMile rows with correct mileage
- [ ] Quarterly report PDF matches IFTA-required schedule format
- [ ] EU tenant: feature is hidden (not just disabled — IFTA is irrelevant to them)
- [ ] Editing the report before submit recomputes tax correctly

## Update [.claude/feature-map.md](../feature-map.md)

Add row under "Compliance & safety":

| IFTA reporting | `Entities/Ifta/IftaQuarterlyReport.cs`, `IftaJurisdictionMile.cs`, `IftaFuelPurchase.cs` | `Commands/Ifta/`, `IIftaCalculationService` | `Infrastructure.Compliance.Ifta/` | `IftaController.cs`, `tms-portal/pages/ifta/` |
