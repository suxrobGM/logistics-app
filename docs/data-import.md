# Data Import

Carriers moving off Datatruck, AscendTMS, TruckingOffice, Rose Rocket, Alvys, Tailwind or a
spreadsheet bring their records in from **Settings > Data import** in the TMS portal. The onboarding
checklist links there too. It needs `Permission.Import.Manage` (Owner and Manager).

## Flow

The import is stateless: the browser re-sends the file with each step and the server stores nothing
between them. The upload is parsed in memory and never written to disk or blob storage.

| Step        | Endpoint                        | What happens                                                                     |
| ----------- | ------------------------------- | -------------------------------------------------------------------------------- |
| Template    | `GET /imports/templates/{type}` | Blank CSV whose headers map back to every field                                  |
| Upload      | `POST /imports/preview`         | Returns the headers, the suggested mapping, the field list and the first 10 rows |
| Map columns | (client)                        | The user fixes the mapping; required fields must be mapped                       |
| Validate    | `POST /imports/validate`        | Dry run: what each row would do (create / update / skip / error), nothing saved  |
| Import      | `POST /imports`                 | Writes the valid rows and returns the same per-row summary, downloadable as CSV  |

Files: `.csv` or `.xlsx` (first sheet; header row is the first non-blank row), up to 10 MB and 5,000
rows. The extension and the browser's content type must agree. CSV is read as UTF-8 (a BOM is
fine); quoted fields with commas, quotes and line breaks work. XLSX numbers come through in
invariant culture and dates as ISO dates.

Rows that match an existing record by natural key are **skipped** unless "update existing" is on;
updates only overwrite the columns the file provides. Rows with errors are reported and left out;
the rest still import.

## Entities and columns

Import in this order, because later types look earlier ones up: customers, drivers, trucks,
trailers, loads. `*` marks a required field.

| Entity    | Natural key      | Fields                                                                                                                                                                       |
| --------- | ---------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Customers | name             | name*, email, phone, tax ID, MC number (appended to notes, there is no MC column), notes, address                                                                            |
| Drivers   | email            | email*, first + last name (or one full-name column, "Doe, Jane" or "Jane Doe"), phone, hire date, address                                                                    |
| Trucks    | truck number     | number*, type, VIN, make, model, year, plate, plate state, driver (name or email), vehicle capacity (required for car haulers), status                                       |
| Trailers  | trailer number   | number*, type (defaults to dry van), VIN, make, model, year, plate, plate state, length, status, notes                                                                       |
| Loads     | source reference | reference*, customer* (by name), rate*, status, load type, truck (by number) or driver (uses that driver's truck), pickup + delivery address*, pickup + delivery date, notes |

The exact keys, labels and header synonyms live in
`src/Core/Logistics.Application/Modules/Platform/DataImport/Constants/ImportFieldCatalog.cs`.

Notes per entity:

- **Drivers** become Driver-role employees straight away, so they can be put on trucks and loads.
  Each gets a password-less user account; no email is sent unless "email each new driver an
  invitation" is ticked. An invitation sent later from the Employees page works the same way: on
  accepting it the driver sets a password on that account. An email that already belongs to another
  company's user is rejected.
- **Trucks** respect the plan's truck limit and the vehicle-transport feature gate, like the truck
  form. A driver can be the main driver of one truck only.
- **Trailer length** is stored in metres. A header naming the unit wins ("Length (ft)",
  "Length m"); otherwise US tenants are read as feet and EU tenants as metres.
- **Loads** are created with `Source = Import` and the reference in `ExternalSourceId`, which is the
  match key on re-import. The importing user is the dispatcher, so they must be an employee of the
  tenant. Historical statuses are set without notifying drivers, dated from the file's pickup and
  delivery dates. Addresses are geocoded only on import (validation makes no geocoding calls), so a
  row can still fail at import with "could not locate the address". Re-importing with "update
  existing" updates truck, dates, notes and status; it does not change the rate or the addresses.

## Values

- **Addresses**: street / line 2 / city / state / zip / country columns, or one "full address"
  column parsed best-effort ("123 Main St, Dallas, TX 75201, USA", "Invalidenstr. 1, 10115 Berlin").
  Separate columns win over the parsed one. Each address is checked with `AddressValidator` (street,
  city, state, zip and a 2-letter country are required) and must be in the tenant's region. A blank
  country means the company's own country.
- **Dates** follow the tenant's date format for slash dates (03/04/2026 is March 4 for US-format
  tenants, 3 April for European); ISO dates always work.
- **Numbers** accept currency symbols and thousands separators ("$1,250.00", "1 250,50 €").
- **Enums** (types, statuses) match our name, its display text, or an alias list next to each
  importer ("V" and "Van" are dry van, "RGN" is lowboy, "Completed" and "Invoiced" are delivered).

## Header mapping

Each column is suggested a field from the synonym table in `ImportFieldCatalog`. Headers compare
after normalisation (lower case, letters and digits only, `#` read as "number"), so "Unit #",
"unit_number" and "UNIT NUMBER" are the same header. A field is suggested for the leftmost matching
column only. When a pilot's export uses a spelling we miss, add it to the field's synonyms and a case
to `ImportHeaderMapperTests`.

## Code map

- Parsing: `Application.Abstractions/Documents/ISpreadsheetReader.cs`, implemented by
  `Infrastructure.Documents/Spreadsheets/SpreadsheetReader.cs` (ExcelDataReader, MIT).
- Mapping, values and addresses: `Modules/Platform/DataImport/Services/`.
- One importer per entity: `Modules/Platform/DataImport/Importers/` - each derives from
  `EntityImporter<TDraft>`, which owns parse, in-file duplicate check, lookup, the
  create/update/skip decision and the write.
- Handlers: `Commands/RunImport/` (validate and import), `Queries/PreviewImport/`,
  `Queries/GetImportTemplate/`; controller `ImportController.cs`.
- UI: `tms-portal/pages/settings/data-import/`, results table in
  `pages/settings/components/import-results/`.
