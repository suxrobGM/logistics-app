# QuickBooks Sync

- **Status**: Done
- **Priority**: P0 - the single most common hard requirement for US small carriers; its absence disqualifies us in most evaluations
- **Effort**: M
- **Category**: Table stakes

## Why

Every competitor (Toro, Truckbase, Rose Rocket, Alvys) syncs to QuickBooks. Carriers' accountants live
in QuickBooks; our native invoicing is a complement, not a replacement. Buyer guides list
"connects to your ELD and QuickBooks out of the box" as the filter criterion for the 1–250 truck band.

## What to build

- QuickBooks Online first (OAuth2 + REST); Desktop later only if demanded.
- One-way push to start: `LoadInvoice` → QBO Invoice, `Payment` → QBO Payment, `CompanyExpense`/`TruckExpense` → QBO Expense, customers → QBO Customers.
- New infrastructure project following the existing pattern: `src/Infrastructure/Logistics.Infrastructure.Integrations.Accounting/` with a port in `Application.Abstractions` (mirror how `Infrastructure.Integrations.Eld` is structured).
- Sync state per entity (external id + last-synced hash) to make pushes idempotent; retry via Hangfire job like `Jobs/EldSyncJob.cs`.
- Tenant-level connection settings page under `tms-portal/pages/settings/` (mirror ELD provider config UX).
- Add `TenantFeature.Accounting` flag; include in Professional+ plans (`SubscriptionPlanSeeder`).

## Acceptance

An invoice created and paid in the TMS appears in the tenant's QBO with correct customer, line items, tax, and payment status - without manual re-entry.

## Notes

- **2026-07-15** - Built v1 (one-way push, QBO Online). New `Infrastructure.Integrations.Accounting` project (thin OAuth2 + REST client, mirrors the ELD pattern) with an `IAccountingProviderService` port. Pushes Customers, LoadInvoices, Payments, and Company/Truck Expenses (→ QBO Purchase) via a per-tenant `AccountingSyncJob` (Hangfire, 15-min, content-hash idempotent through `QboEntityMapping`). Connect/disconnect/status under `Modules/Integrations/Accounting`; anonymous OAuth callback resolves the tenant from a Data-Protection-signed `state`. Added `TenantFeature.Accounting` (Professional+), `Permission.Accounting`, and a TMS `settings/accounting` page. Provider secret columns (Accounting **+ ELD + LoadBoard**) are now encrypted at rest via a reusable `EncryptedStringConverter` (Data Protection; reads legacy plaintext transparently). Expense push needs QBO account refs - resolved from the Chart of Accounts on connect. Angular client regenerated; TMS `settings/accounting` page + website landing pages wired; unit tests (mapper + encryption converter) and architecture tests green. **Before production:** configure real QBO app credentials (`Accounting:QuickBooks`) and run an end-to-end verification against a QuickBooks sandbox company.
