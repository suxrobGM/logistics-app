# QuickBooks Sync

- **Status**: Done (v2 hardening 2026-10-14; sandbox end-to-end run pending)
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
- **2026-10-14** - Production hardening for pilot carriers. Connection: the OAuth `state` now binds tenant + user and expires after 10 minutes; the callback always redirects back to the portal (`status=success|error|denied`); refresh stores the rolled refresh-token expiry and an `invalid_grant` flags the connection **Reconnect needed** and pauses sync; connecting a different QBO company clears the old defaults, mappings and every `QboEntityMapping`; disconnect is no longer plan-gated; `/accounting/quickbooks/disconnected` serves as the Intuit Disconnect URL; QBO errors carry the fault detail and `intuit_tid`. Sync moved into `AccountingSyncService`: checks `TenantFeature.Accounting`, loads only rows edited since their last push (500 per type per run, batched lookups), backs failed rows off for an hour, retries 429s through `RateLimitRetryHandler`, re-reads and retries once on a stale SyncToken, and sends a deterministic `requestid` on creates. Payloads gained invoice TxnDate/DueDate and a per-line item (default item + per-line-type overrides), payment TxnDate and deposit account, expense category → account mapping, a configurable paid-from account (card vs bank decides the payment type) and a vendor payee, customer linking by display name and deactivation, invoice voiding on cancel, and approved payroll → QBO Bill owed to a per-employee Vendor. Tenants set all of it, plus a "sync from" date that keeps old history out, on the settings card, which also has Sync now and a sync-errors table with retry. Setup guide with the Intuit production checklist: [accounting-quickbooks-setup.md](../accounting-quickbooks-setup.md). **Remaining:** QuickBooks Desktop; QBO sales tax codes (tax is sent as a TotalTax override); paid driver-pay bills are not marked paid (no BillPayment); nothing flows back from QBO; refresh is serialized per API instance only. Still needs an end-to-end run against a sandbox company before the first pilot.
