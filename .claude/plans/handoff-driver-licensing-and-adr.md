# Handoff: Driver Licensing + ADR / Hazmat Compliance

> **Priority:** MEDIUM. **Effort:** M (3–4 days).
>
> Today [Employee.cs](../../src/Core/Logistics.Domain/Entities/Employee.cs) has no driver-license fields. US needs CDL class + endorsements + Hazmat; EU needs categories C/CE/D + ADR (Accord Dangereux Routier) certification + ADR-equipped vehicle.

## Sequencing

- **Position in overall order:** 6th
- **Depends on:** Plan #6 ([i18n](handoff-i18n-multi-language.md)) for license-class label translations. Plan #10 ([address forms](handoff-region-aware-address-and-tenant-fields.md)) for the country-aware `IssuingCountry` selector.
- **Unblocks:** Plan #4 ([tachograph HOS](handoff-eu-tachograph-hos.md)) — `IDispatchEligibilityService` is consumed by HOS-aware dispatch. Plan #5 ([EU load boards](handoff-eu-load-boards.md)) — booking ADR loads will check eligibility.
- **Why sixth:** Adds two domain entities (`DriverLicense`, `AdrEquipment`) but does not change shared infrastructure. Safe to land in parallel with plans #8 / #9 if multiple sessions run.

## Why now

- Without license tracking: dispatchers can't filter "drivers qualified for ADR loads", drivers expire without warning
- Hazmat/ADR mismatches between driver and load are a common dispatch error
- Truck-side ADR equipment requirements (orange plates, fire extinguishers) need vehicle-level tracking

## Current state

- `Employee` has no license fields
- `Truck` ([Truck.cs](../../src/Core/Logistics.Domain/Entities/Truck.cs)) has `LicensePlate`, `LicensePlateState` but no Hazmat/ADR equipment flags
- `Load` likely has Hazmat boolean (verify under [Entities/Load/](../../src/Core/Logistics.Domain/Entities/Load/)) but no ADR class linkage

## Backend

### Domain

#### Driver licensing

- New entity `Entities/Employee/DriverLicense`:
  - `EmployeeId`, `LicenseNumber`, `IssuingCountry` (ISO 3166), `IssuingRegion` (state/province for US/CA)
  - `LicenseClass` (string enum-ish): US `A` / `B` / `C`; EU `C` / `CE` / `D` / `DE` / `C1` / `C1E`
  - `Endorsements` (flags or string list): US `H` (hazmat), `N` (tanker), `T` (doubles/triples), `P` (passenger); EU `ADR`, `ADR Tanks`, `ADR Class 1` (explosives), `ADR Class 7` (radioactive)
  - `IssuedDate`, `ExpiresAt`, `MedicalCertExpiresAt` (US: DOT medical card)
  - `DocumentBlobPath` (scan of the license)
- `Employee` gains nav `virtual List<DriverLicense> Licenses { get; }` (history)
- New enum `LicenseClass` with `[Description]` overrides per region
- New enum `Endorsement` (flag enum)

#### ADR / Hazmat (vehicle side)

- New value object on `Truck`: `AdrEquipment` (record):
  - `IsAdrCertified`, `AdrCertExpiresAt`, `AllowedClasses` (flag enum: Class1..Class9), `OrangePlateNumber`
- US side: `Truck.IsHazmatPlacarded` (bool)

#### Load side

- Confirm `Load.IsHazmat` exists; add `HazmatClass` (enum 1..9 with subdivisions) and `UnNumber` (string, e.g., "UN1203" for petrol)
- Add ADR-class compatibility check when assigning trip → truck → driver

### Application

- `Commands/Employee/CreateOrUpdateDriverLicense`
- `Queries/Employee/GetExpiringLicensesQuery` — used by a notification job
- New domain service `IDispatchEligibilityService.CanAssignAsync(Truck, Driver, Load)`:
  - Returns reasons why an assignment is invalid (license class mismatch, ADR class missing, expired cert)
  - Used by `AssignTripCommand` and surfaced in the AI dispatch tool registry
- New tool for AI dispatch: `CheckDispatchEligibility` — see [add-dispatch-tool skill](.claude/skills/add-dispatch-tool/) — exposes the service to the LLM

### Infrastructure

- New job `Jobs/LicenseExpiryReminderJob` (Hangfire daily) — generates notifications 60/30/7 days before expiry; handles both DOT medical and ADR/CDL
- Document upload path uses existing `IBlobStorageService`

### Persistence / migration

- New tenant tables: `DriverLicenses`, `LicenseEndorsements` (or store as flag column)
- New columns on `Truck`: `AdrEquipment_*`, `IsHazmatPlacarded`
- New columns on `Load`: `HazmatClass`, `UnNumber`

## API

- `DriverLicenseDto`, `EmployeeDto.Licenses`
- New endpoints:
  - `GET /api/employees/{id}/licenses`
  - `POST /api/employees/{id}/licenses`
  - `GET /api/dispatch/eligibility?truckId=&driverId=&loadId=` — returns issues/warnings
- Run `bun run gen:api`

## Frontend (Angular)

### TMS portal

- `pages/employees/components/driver-licenses-tab/` — table of licenses, upload scan, expiry warnings
- `pages/employees/employee-form/` — primary license fields (class, country, expiry)
- `pages/trucks/truck-form/` — ADR equipment section (only for EU tenant) / Hazmat placard checkbox (US)
- `pages/loads/load-form/` — Hazmat class + UN number; show real-time eligibility warning when assigning truck/driver
- `pages/dashboard/` — widget: "Licenses expiring in 30 days"
- `pages/notifications/` — license-expiry notification type rendered with link to driver

### Customer portal

- N/A

### Admin portal

- N/A directly; superadmin sees expiring licenses across tenants only if useful (skip)

## Mobile (driver app)

- New screen `screens/MyLicensesScreen.kt` — driver self-views their licenses, sees expiry warnings, uploads renewed scan
- Push notifications for expiry (already infra exists)

## Tests

- `IDispatchEligibilityService` exhaustive tests:
  - US driver no Hazmat endorsement + Hazmat load → blocked
  - EU driver ADR Class 3 + ADR Class 7 load → blocked, suggests qualified drivers
  - Expired ADR cert → blocked with reason
- License expiry job — generates exactly one notification per driver per threshold

## Acceptance criteria

- [ ] Driver gets push notification 30 days before ADR cert expiry
- [ ] Dispatcher cannot save Trip assignment when truck not ADR-equipped for the load's class — UI shows the reason
- [ ] AI dispatch agent returns "X is not qualified for ADR Class 7" when relevant
- [ ] Licenses tab shows expiry status with color coding
- [ ] US workflows unchanged: CDL Class A + Hazmat endorsement still works as before

## Update [.claude/feature-map.md](../feature-map.md)

Add to "Compliance & safety":

| Driver licensing | `Entities/Employee/DriverLicense.cs` | `Commands/DriverLicense/`, `Services/DispatchEligibility/` | - | `EmployeeController` (extended), `tms-portal/pages/employees/driver-licenses-tab/`, mobile `screens/MyLicensesScreen` |
| ADR / Hazmat | `Truck.AdrEquipment`, `Load.HazmatClass` | `IDispatchEligibilityService` | - | (under Loads / Trucks pages) |
