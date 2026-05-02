# Handoff: Region-Aware Addresses + Tenant MC / VAT / EORI Fields

> **Priority:** MEDIUM (touches every form — do before adding more pages). **Effort:** M (3 days).
>
> [Address.cs](../../src/Core/Logistics.Domain.Primitives/ValueObjects/Address.cs) requires `State` for every address — true for US/CA/MX/AU but not for many EU countries (Germany Bundesland is optional, Netherlands has no state at all). Tenant has `DotNumber` already but is missing other regulatory IDs (MC number, VAT, EORI for customs).

## Sequencing

- **Position in overall order:** 5th
- **Depends on:** Plan #6 ([i18n](handoff-i18n-multi-language.md)) ideally — country/state labels and validation messages should be born translated. If you want to ship address forms before i18n, leave English-only and translate later.
- **Unblocks:** Plan #1 ([VAT engine](handoff-vat-tax-engine.md)) needs `Tenant.VatNumber` and `Customer.TaxId` — these are added here. Plan #2 ([payment methods](handoff-eu-payment-methods.md)) reuses `Tenant.VatNumber` for Stripe `tax_id_data`. Note: if you ran #1 / #2 first, you may have already added the Tenant/Customer fields — only the `Address.State` nullability change remains.
- **Why fifth:** Address shape is structural and breaking. Doing it after i18n but before licensing/units means the new `<ui-address-form>` is the canonical form pattern that every later plan adopts.

## Why now

- Forms today require users to invent a "State" value for non-US addresses, polluting data
- VAT IDs and EORI numbers are needed for customs declarations and Stripe Tax (plan #1)
- US carriers are required to display MC number on documents

## Verified existing state

- [Tenant.cs](../../src/Core/Logistics.Domain/Entities/Tenant.cs):
  - `DotNumber` ✓ already exists (line 22)
  - `BillingEmail`, `PhoneNumber`, `LogoPath` ✓
  - **Missing**: `McNumber`, `VatNumber`, `EoriNumber`, `CompanyRegistrationNumber` (Companies House / Handelsregister)
- [Address.cs](../../src/Core/Logistics.Domain.Primitives/ValueObjects/Address.cs):
  - `Line1`, `City`, `ZipCode`, `State`, `Country` are all `required` strings
  - **Issue**: `State` cannot be empty/null — breaks NL/IS/MT addresses
- [Truck.LicensePlateState](../../src/Core/Logistics.Domain/Entities/Truck.cs#L48) — same problem; license plate is registered to a country/state but state may be null in EU

## Backend

### Domain — Address shape change ⚠️ BREAKING

- Make `Address.State` **nullable** (`string?`) — most pragmatic fix
- Frontend will use country to decide whether to require it (US/CA/AU/MX/BR/IN states required; most EU optional)
- Document affected entities: anywhere using `Address` value object — handlers should not assume `State` is set
- Validation moves to FluentValidation: `RuleFor(a => a.State).NotEmpty().When(a => StateRequiredCountries.Contains(a.Country))`

### Domain — Tenant new fields

- `Tenant`:
  - `string? McNumber` (US Motor Carrier)
  - `string? VatNumber` (EU VAT or UK VAT)
  - `string? EoriNumber` (EU/UK customs ID)
  - `string? CompanyRegistrationNumber` (Companies House, Handelsregister, RCS, etc.)
  - `string? TaxResidencyCountry` (defaults to `CompanyAddress.Country`)
- All optional — existing US tenants don't need to fill them in

### Application

- `Commands/Tenant/UpdateTenant` — accept new fields
- Validators: VAT format check (`^[A-Z]{2}[0-9A-Z]+$` per country pattern); MC number format; EORI format
- (Optional) `IVatValidator.ValidateAsync(string vatNumber)` calling VIES SOAP for EU VAT (free EU service); cache results

### Infrastructure

- `IVatValidator` implementation in new file `Logistics.Infrastructure.Communications/Validation/ViesVatValidator.cs` (or under a new module). VIES has rate limits — add caching (memory + 24h TTL is fine)
- Stripe Customer creation needs to pass `TaxIdData` from these fields (covered by plan #2)

### Persistence / migration

- Tenant master DB: new nullable columns
- ⚠️ Address change: regenerate migration; `State` becomes nullable. Make sure migration uses `AlterColumn` with `IsRequired = false`
- Existing rows: `State` might be empty strings (`""`) in some places — backfill to `NULL`

## API

- `TenantDto`: `McNumber`, `VatNumber`, `EoriNumber`, `CompanyRegistrationNumber`
- `AddressDto.State` becomes optional
- Run `bun run gen:api`

## Frontend (Angular)

### Shared

- New `<ui-address-form>` component (replace ad-hoc address inputs across portals):
  - Country select first
  - Region/State field label and required-ness driven by selected country:
    - `US` → "State", required, dropdown of 50 + DC
    - `CA` → "Province", required, dropdown
    - `AU` → "State", required, dropdown
    - `DE` → "Bundesland", optional, free-text
    - `NL`, `BE`, `IE` → no state field at all
    - Default → "Region/County", optional
  - Postal code format validation per country
- Country list as constant in [shared/constants/](../../src/Client/Logistics.Angular/projects/shared/src/lib/constants/)
- State requirements as constant: `STATE_REQUIRED_COUNTRIES = ['US', 'CA', 'AU', 'MX', 'BR', 'IN']`

### TMS portal

- Replace every place that builds an address from individual `<input>`s with `<ui-address-form>`:
  - Customer form, employee form, truck form, load route form, tenant settings, expense form, accident report
- `pages/settings/company/` — add new tenant fields (MC, VAT, EORI, Reg #) with validators; "Validate VAT" button calls VIES
- Loadboard/dispatch flows that show address inline — verify they use a presenter that handles missing State

### Customer portal

- Customer signup / address forms — same `<ui-address-form>`

### Admin portal

- `pages/tenants/tenant-form/` — surface new fields (admin can edit these on behalf of tenant)
- Display tenant MC/VAT/EORI on the tenant detail page

### Website

- Demo request form / contact form — country field but no state (consumer signup)

## Mobile (driver app)

- Address display — handle missing state gracefully (`"$City, $State $ZipCode"` → `"$City${state ? ", $State" : ""} $ZipCode"`)
- Addresses are mostly read-only in mobile (delivery destinations, terminals) — no form changes typically needed
- If driver edits their own profile address: same conditional logic

## Tests

- Address validator tests for 10+ countries, asserting state required vs not
- Migration test: existing US row migrates cleanly; existing row with empty-string state becomes NULL
- VAT validator: real VIES integration test (use a known-valid Apple VAT for stable test)

## Acceptance criteria

- [ ] Saving a Dutch (`NL`) address without a state succeeds
- [ ] Saving a US address without a state fails with clear error
- [ ] Tenant settings shows MC + VAT + EORI fields; VAT validation button works against VIES
- [ ] Existing tenant data unchanged
- [ ] Form errors are localized (depends on plan #6 i18n)
- [ ] Address display on PDF / emails handles missing state gracefully ("Amsterdam 1011 AB, NL", not "Amsterdam, , 1011 AB, NL")

## Update [.claude/feature-map.md](../feature-map.md)

No new row. Update Tenant settings row description to reflect new fields. Note in conventions that addresses must use `<ui-address-form>`.
