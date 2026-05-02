# Handoff: VAT & Tax Engine

> **Priority:** HIGH (legal blocker for EU billing). **Effort:** L (~1 week).
>
> EU invoicing legally requires per-line VAT, VAT IDs on the issuer/customer, and reverse-charge handling for cross-border B2B. The codebase has zero tax logic today — `InvoiceLineItemType.Tax` exists as an enum value but nothing computes it.

## Sequencing

- **Position in overall order:** 1st
- **Depends on:** Plan #10 ([region-aware addresses + Tenant MC/VAT/EORI](handoff-region-aware-address-and-tenant-fields.md)) for the `Tenant.VatNumber` and `Customer.TaxId` columns. Do plan #10 first, **or** include those column adds inline in this plan and skip them in #10.
- **Unblocks:** Plan #2 ([EU payment methods](handoff-eu-payment-methods.md)) — Stripe Customer creation needs `tax_id_data` from VAT field. Plan #11 ([IFTA](handoff-ifta-fuel-tax.md)) reuses the `Invoice` totals breakdown for tax-line reporting.
- **Why first:** EU tenants legally cannot send invoices without VAT — every other EU work item depends on tenants being legally allowed to bill.

## Why now

- Without VAT on invoices, EU tenants cannot legally invoice their customers
- US sales tax (destination-based) varies per state — same engine can serve both regions
- Stripe Tax handles both VAT and US sales tax with one integration; we should use it instead of building our own rate tables

## Current state

- [InvoiceLineItem.cs](../../src/Core/Logistics.Domain/Entities/Invoice/InvoiceLineItem.cs) — has `Amount` (Money) and `Quantity` only, no tax fields
- [Invoice.cs](../../src/Core/Logistics.Domain/Entities/Invoice/Invoice.cs) — has `Total` (gross) only, no `Subtotal`/`TaxTotal`/`TaxRate` breakdown
- [Customer.cs](../../src/Core/Logistics.Domain/Entities/Customer.cs) — no `TaxId` / VAT number field
- [Tenant.cs](../../src/Core/Logistics.Domain/Entities/Tenant.cs) — no `VatNumber` / `TaxId` / `EoriNumber` field
- [InvoicePdfService.cs](../../src/Infrastructure/Logistics.Infrastructure.Documents/Pdf/InvoicePdfService.cs) — does not render a VAT block
- Plan #10 covers Tenant tax-ID fields (do that one first or in parallel)

## Backend

### Domain

- Add `TaxId` (string?) and `IsVatExempt` (bool) to `Customer` ([Customer.cs](../../src/Core/Logistics.Domain/Entities/Customer.cs))
- Extend `InvoiceLineItem` with: `TaxRatePercent` (decimal), `TaxAmount` (decimal), `TaxCode` (string? — Stripe Tax tax code or local code)
- Add to `Invoice`:
  - `Subtotal` (Money) — sum of line items pre-tax
  - `TaxTotal` (Money)
  - `TaxBreakdown` (JSON column / new `InvoiceTaxLine` child entity: rate, base amount, tax amount, jurisdiction)
  - `TaxBehavior` enum: `Inclusive`, `Exclusive`, `ReverseCharge`
- Add `TaxJurisdiction` value object (country code + region/state) — used to determine applicable rate
- Domain method `Invoice.RecalculateTotals()` that recomputes `Subtotal`/`TaxTotal`/`Total` from line items; raise `InvoiceTotalsRecalculatedEvent`

### Application

- New service contract `Logistics.Application.Services.Tax.ITaxCalculator`:
  - `CalculateAsync(InvoiceLineItem[], Address customerAddress, Address tenantAddress, string? customerTaxId, CancellationToken)` → returns line-level rate + breakdown
- `Commands/Invoice/CreateLoadInvoice` and `Commands/Invoice/AddInvoiceLineItem` — call `ITaxCalculator` before persisting
- `Commands/Invoice/SendInvoice` — block sending if EU tenant has no `VatNumber`
- New query `GetInvoiceTaxBreakdownQuery` for the UI summary
- FluentValidation: `Customer.TaxId` required when tenant `Region == Eu` and `Address.Country` is in EU list

### Infrastructure

- New project: `src/Infrastructure/Logistics.Infrastructure.Tax/` (mirror existing module pattern)
- Two implementations of `ITaxCalculator`:
  - `StripeTaxCalculator` (default) — calls `Stripe.TaxCalculationService.Create` with line items + customer address; map line-level breakdown back
  - `ManualTaxCalculator` (fallback) — reads tenant-configured rates from a new `TenantTaxRate` master entity (jurisdiction + percent + effective dates) for tenants who can't enable Stripe Tax
- Selected via config `Tax:Provider` = `stripe` | `manual`, default `stripe`
- Register in `Registrar.cs` of the new project
- `InvoicePdfService` — add VAT block: per-line `Net | Rate | VAT | Gross`; footer with `VAT total`, `Reverse charge — VAT to be accounted by recipient` notice when applicable; tenant VAT number in header
- `StripeServiceBase` — pass `automatic_tax: { enabled: true }` on `PaymentIntent`/`Invoice` create when `Tax:Provider = stripe`

### Persistence / migration

- Use the `migration-creator` skill on the **tenant** DbContext
- New columns: `Customer.TaxId`, `Customer.IsVatExempt`, `Invoice.Subtotal_Amount`, `Invoice.Subtotal_Currency`, `Invoice.TaxTotal_Amount`, `Invoice.TaxTotal_Currency`, `Invoice.TaxBehavior`, `InvoiceLineItem.TaxRatePercent`, `InvoiceLineItem.TaxAmount`, `InvoiceLineItem.TaxCode`
- Backfill: for existing invoices set `Subtotal = Total`, `TaxTotal = 0`, `TaxBehavior = Exclusive`
- New table `InvoiceTaxLines` (or JSON column on `Invoice` — pick JSON to avoid the child collection round-trip; lazy loading is enabled)
- New master-DB table `TenantTaxRates` (only used by manual calculator)

## API

- Update `InvoiceController` request/response DTOs in [Logistics.Shared.Models](../../src/Shared/Logistics.Shared.Models/) to include the new tax fields
- New endpoint `POST /api/invoices/{id}/preview-tax` — returns calculated breakdown without saving (used by the create-invoice form)
- New endpoint `GET /api/tax/jurisdictions` — list of jurisdictions for the manual rate config UI
- Run `bun run gen:api` after API changes

## Frontend (Angular)

### Shared

- [LocalizationService](../../src/Client/Logistics.Angular/projects/shared/src/lib/services/localization.service.ts) — add `formatTaxRate(percent)` and `getTaxLabel()` (returns `"VAT"` for EU tenants, `"Sales Tax"` for US, `"GST"` for AU/CA)
- New `<ui-money-with-tax>` component in shared — renders `Net + VAT @ X% = Gross`

### TMS portal

- `pages/invoices/components/invoice-form/` — add Tax behavior selector (Inclusive/Exclusive/Reverse charge), per-line tax-rate field, live-recalc using `previewTax`
- `pages/invoices/components/invoice-detail/` — show breakdown table, render reverse-charge notice
- `pages/customers/components/customer-form/` — add `TaxId` field (required when EU)
- `pages/settings/billing/` — surface tenant VAT number (from plan #10) and Stripe Tax onboarding link
- New `pages/settings/tax-rates/` — master-rate table for tenants on the manual calculator

### Customer portal

- `pages/invoices/` invoice detail — render VAT breakdown identically to PDF
- `pages/payment/public-payment.html` — show VAT line in payment summary

### Admin portal

- No direct changes; subscription invoices flow through the same `Invoice` table so they benefit automatically

## Mobile (driver app)

- N/A directly. Drivers don't see customer invoices. Skip.

## Tests

- `tests/Logistics.Application.Tests/Invoice/` — handler tests for `Subtotal + TaxTotal == Total` invariant
- New `tests/Logistics.Infrastructure.Tax.Tests/` project — manual calculator unit tests, Stripe calculator integration test (mocked HTTP)
- Snapshot test for `InvoicePdfService` rendering VAT block

## Acceptance criteria

- [ ] EU tenant creates an invoice with two line items at 21% and 9% VAT — `Subtotal`, `TaxTotal`, `Total` correct, PDF shows breakdown
- [ ] Cross-border B2B (DE seller, FR customer with VAT ID) — invoice marked reverse-charge, no VAT calculated, notice on PDF
- [ ] US tenant in TX selling to a customer in CA — Stripe Tax computes correct destination-based sales tax
- [ ] Customer without VAT ID + EU tenant — frontend blocks save with validation message
- [ ] Existing US invoices unchanged after migration (Subtotal = Total, TaxTotal = 0)

## Update [.claude/feature-map.md](../feature-map.md)

Add row under "Financial":

| Tax engine | `ITaxCalculator`, `TenantTaxRate.cs`, `Invoice.TaxBehavior` | `Services/Tax/` | `Infrastructure.Tax/` (Stripe, Manual) | `tms-portal/pages/settings/tax-rates/` |
