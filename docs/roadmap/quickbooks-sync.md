# QuickBooks Sync

- **Status**: Done (v2 hardening 2026-10-14; sandbox end-to-end run pending)
- **Priority**: P0 - the single most common hard requirement for US small carriers; its absence disqualifies us in most evaluations
- **Effort**: M
- **Category**: Table stakes

## Why

Every competitor (Toro, Truckbase, Rose Rocket, Alvys) syncs to QuickBooks. Carriers' accountants live
in QuickBooks; our native invoicing is a complement, not a replacement. Buyer guides list
"connects to your ELD and QuickBooks out of the box" as the filter criterion for the 1–250 truck band.

## What was built

One-way push to QuickBooks Online every 15 minutes: customers, load invoices (voided on cancel),
payments, expenses and approved payroll as bills. Setup and the field-by-field sync table:
[accounting-quickbooks-setup.md](../accounting-quickbooks-setup.md).

## Acceptance

An invoice created and paid in the TMS appears in the tenant's QBO with correct customer, line items, tax, and payment status - without manual re-entry.

## Notes

- **2026-07-15** - v1 shipped (one-way push, QBO Online).
- **2026-10-14** - Hardened for pilot carriers (reconnect handling, incremental sync, item and
  account mappings, payroll bills, sync-from date, sync errors on the settings card).
- **Remaining:** an end-to-end run against a sandbox company before the first pilot; QuickBooks
  Desktop; QBO sales tax codes (tax is sent as a TotalTax override); paid driver-pay bills are not
  marked paid (no BillPayment); nothing flows back from QBO; token refresh is serialized per API
  instance only.
