# Handoff: Fuel Volume + Temperature Units

> **Priority:** MEDIUM. **Effort:** S (1–2 days). **Status:** Done 2026-05-11 (Load reefer + mobile + C# converters deferred — see below).
>
> Distance and weight units already exist; volume (gallons vs liters) and temperature (F vs C) do not. Reefer cargo is shipped at temperature — wrong unit means wrong cargo. Fuel expense reporting needs gallons or liters.

## Status (2026-05-11)

Shipped:

- Domain enums `VolumeUnit { Gallons, Liters }` and `TemperatureUnit { Fahrenheit, Celsius }` under `Logistics.Domain.Primitives.Enums`.
- `TenantSettings.VolumeUnit` (default Gallons) and `TenantSettings.TemperatureUnit` (default Fahrenheit) — Master DB migration `Version_0009` adds the columns with backfill defaults.
- US/EU `RegionProfile` set the new units; `DemoTenantsSeeder.ApplyRegionalSettings` propagates them.
- `TruckExpense.Quantity (decimal?)` + `TruckExpense.QuantityUnit (VolumeUnit?)` — Tenant DB migration `Version_0009` adds nullable columns; cascaded through `CreateTruckExpenseCommand`/Handler/Validator, `UpdateExpenseCommand`/Handler, `ExpenseDto`, `ExpenseMapper`.
- Angular `Converters` gains `convertVolume`, `convertTemperature`, `convertFuelEfficiency` plus three new unit type aliases.
- `LocalizationService` gains `getVolumeUnit/Label/Name`, `formatVolume`, `getTemperatureUnit/Label/Name`, `formatTemperature`, `getFuelEfficiencyUnit`, `formatFuelEfficiency`.
- TMS portal `expense-add` / `expense-edit` show Quantity + Unit inputs only when the truck-expense category is Fuel; defaults the unit from `LocalizationService`.
- TMS portal `expense-detail` renders fuel quantity formatted in the tenant's preferred unit (with conversion when stored in a different unit).
- `openapi.json` extended and Angular API client regenerated (`bun run gen:api`); 558 generated files refreshed.

Deferred (out of scope of this handoff — call out in next planning round):

- **Load reefer fields.** `Load` has no `TemperatureSetpoint` today. Adding both fields plus DTO, mapper, create/update commands, validators, and load forms is a larger slice. The `formatTemperature` helper is in place; just wire fields when reefer becomes priority.
- **Mobile / Kotlin driver app.** No fuel-entry or reefer screens exist yet. Add `VolumeUnit`/`TemperatureUnit` to `UserSettings.kt` when those flows ship.
- **Server-side C# conversion helpers.** No equivalents exist for distance/weight today; conversion stays a presentation concern. Add when fuel-report PDF generation needs it.
- **Fuel efficiency dashboard widget** + **fuel report PDF** — plan only added primitives. Build the widgets in a follow-up.

## Sequencing

- **Position in overall order:** 8th
- **Depends on:** Nothing.
- **Unblocks:** Plan #11 ([IFTA](handoff-ifta-fuel-tax.md)) — IFTA fuel-purchase aggregation needs `TruckExpense.Quantity` + `QuantityUnit` columns added here.
- **Why eighth:** Small, isolated, mirrors the existing distance/weight unit pattern. Pair with #7 in a single session if time allows — both are domain-extension work that doesn't touch shared infrastructure.

## Why now

- Fuel expense rows currently store amount of money but not quantity in unit-aware form
- Reefer load setpoint is set as a single number — assumed Fahrenheit today
- Conversion utilities (MPG ↔ L/100km) needed for fleet efficiency dashboards

## Current state

- `TenantSettings`: `DistanceUnit`, `WeightUnit` — no `VolumeUnit`, no `TemperatureUnit`
- `LocalizationService`: getDistanceUnit, getWeightUnit — extend with volume/temperature
- Fuel expense entity (under [Expense/](../../src/Core/Logistics.Domain/Entities/Expense/)) — verify whether it stores `Quantity` + `Unit`
- Load entity — has `TemperatureSetpoint`? Check

## Backend

### Domain

- New enums in `Logistics.Domain.Primitives.Enums.Tenant`:
  - `VolumeUnit { Gallons = 1, Liters = 2 }`
  - `TemperatureUnit { Fahrenheit = 1, Celsius = 2 }`
- Extend `TenantSettings`:
  - `VolumeUnit Volume { get; set; } = VolumeUnit.Gallons` (US default)
  - `TemperatureUnit Temperature { get; set; } = TemperatureUnit.Fahrenheit`
- Region profiles ([UsRegionProfile](../../src/Presentation/Logistics.DbMigrator/Regions/UsRegionProfile.cs), [EuRegionProfile](../../src/Presentation/Logistics.DbMigrator/Regions/EuRegionProfile.cs)) — set defaults (US: gallons/F, EU: liters/C)
- Add to `TruckExpense` (or wherever fuel rows live): `Quantity` (decimal?) + `QuantityUnit` (VolumeUnit?)
- Add to `Load` (if reefer-related fields exist): `TemperatureUnit` next to `TemperatureSetpoint`

### Application / Infrastructure

- Conversion helpers in `Logistics.Domain.Primitives.Conversions`:
  - `gallons ↔ liters` (1 US gal = 3.78541 L)
  - `°F ↔ °C`
  - `MPG ↔ L/100km` (`L100km = 235.215 / mpg`)
- Used by:
  - Fuel report generation (always normalize to tenant unit)
  - Load detail PDF
  - Mobile app display

### Persistence / migration

- New tenant DB columns on `TruckExpense.Quantity`, `TruckExpense.QuantityUnit`
- New on `Load`: `TemperatureUnit` (only for reefer subtypes)
- `TenantSettings` complex type new properties

## API

- `TruckExpenseDto` new fields, `LoadDto.TemperatureUnit`
- `TenantSettingsDto` `Volume`, `Temperature`
- Run `bun run gen:api`

## Frontend (Angular)

### Shared

- Extend [LocalizationService](../../src/Client/Logistics.Angular/projects/shared/src/lib/services/localization.service.ts):
  - `getVolumeUnit(): 'gal' | 'L'`
  - `getVolumeUnitName(): 'Gallons' | 'Liters'`
  - `getTemperatureUnit(): 'F' | 'C'`
  - `formatVolume(amount, fromUnit?)` — converts to tenant unit and formats
  - `formatTemperature(value, fromUnit?)`
  - `formatFuelEfficiency(value, fromUnit?)` — handles MPG ↔ L/100km
- Update existing `converters` util ([utils/converters.ts](../../src/Client/Logistics.Angular/projects/shared/src/lib/utils/)) to add `convertVolume` / `convertTemperature` / `convertFuelEfficiency`

### TMS portal

- `pages/expenses/components/truck-expense-form/` — fuel category gets quantity + unit input (default unit from tenant)
- `pages/expenses/components/expense-detail/` — render fuel quantity in tenant unit
- `pages/loads/load-form/` (reefer section) — temperature input with unit suffix
- `pages/dashboard/` fuel efficiency widget — render in tenant unit (MPG for US, L/100km for EU)
- `pages/reports/` — fuel report

### Customer portal

- Reefer load tracking page — show setpoint in tenant unit

## Mobile (driver app)

- `model/UserSettings.kt` — add `VolumeUnit` and `TemperatureUnit` enums alongside existing `DistanceUnit`
- `util/UnitConverters.kt` (new) — pure-Kotlin conversion helpers
- `ui/screens/FuelEntryScreen.kt` (or wherever fuel expense input lives) — unit-aware input
- Reefer-trip screen shows temperature in driver's preferred unit

## Tests

- Conversion round-trip property tests (`gallons → liters → gallons`)
- Boundary case: -40°F == -40°C — both should display as -40 in their unit
- Localization tests: en-US tenant displays "5.5 gal", de-DE displays "20,8 L"

## Acceptance criteria

- [ ] EU tenant entering 100 L of diesel sees "100 L" everywhere; switches to gallons → "26.4 gal"
- [ ] Fuel efficiency widget shows MPG for US tenant, L/100km for EU tenant — value computes correctly from `Quantity`
- [ ] Reefer load setpoint -10°C displays as "14°F" for US viewer

## Update [.claude/feature-map.md](../feature-map.md)

No new row needed. Update conventions section to mention volume/temperature units alongside distance/weight.
