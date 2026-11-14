# Query parameter standard

camelCase on the wire, one name per concept. No aliases for old names; every client is updated in the same change. The convention is in `.claude/rules/backend/api-design.md`.

## Steps

- [x] OpenAPI transformer camelCases query parameter names and drops query duplicates of route values
- [x] Regenerate the Angular and driver app clients; camelCase every call site and sort key
- [x] Rename: `IntervalQuery` → `DateRangeQuery`, `PagedIntervalQuery` → `PagedDateRangeQuery`, `StartDate`/`EndDate` → `FromDate`/`ToDate` (queries, portal requests)
- [x] Rename: `OnlyActiveLoads`/`OnlyActiveTrips` → `ActiveOnly`; truck's `OnlyActiveLoads` → `ActiveLoadsOnly`
- [x] Rename: `UserId` → `DriverId` on loads, daily and monthly grosses
- [x] Invoices: `InvoiceType` → `Type`; `CustomerName`/`EmployeeName` fold into `Search`
- [x] Trips: `SearchableQuery`; `Name`/`TruckNumber` fold into `Search` (trip search was ignored before)
- [x] Time entries: `Type` becomes the enum; drop unused `IncludeLinkedToPayroll`
- [x] Loads: drop unused `LoadAllPages`; expenses: use `SearchableQuery`
- [x] Arch test banning the old names + rule in `.claude/rules/backend/api-design.md`
- [ ] Drop `?? undefined` / `|| undefined` on Angular query params where the request builder already skips the value

## Later

- Sort keys resolve against entity paths; an allowlist per query would decouple them from the schema.
- `DateRangeQuery` defaults both ends to `UtcNow`, so an omitted range is empty.
- Multipart form fields (document upload) are still PascalCase.
