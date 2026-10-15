# Factoring Integration

- **Status**: Done (email submission v1; factor APIs not started)
- **Priority**: P0 - most small carriers factor invoices; a TMS that can't submit to the factor forces double data entry
- **Effort**: M
- **Category**: Table stakes

## Why

Factoring companies (RTS, OTR Capital, Triumph, TAFS) advance 95–98% of invoice value same-day.
Competitors auto-submit invoice + rate con + POD/BOL to the factor. We have all the documents
(`Entities/Document/`, POD/BOL PDFs via QuestPDF) - the gap was the submission pipeline.

Long-term, [same-day-pay](same-day-pay.md) attacks factoring itself; this integration is the bridge
for carriers already locked into factoring contracts.

## What was built

- **Factor profiles** (`FactoringCompany`, Settings > Factoring): name, submission email, phone,
  remit-to address, recourse or non-recourse, advance rate and fee. The reserve is what the two
  leave over.
- **Factored customers**: `Customer.FactoringCompanyId`. Their invoice PDFs carry a notice of
  assignment with the factor's remit-to address (and no carrier payment state), the invoice email
  carries the notice instead of a payment link, and the customer portal shows "Pay to {factor}" and
  refuses card checkout.
- **Schedule of accounts** (`FactoringBatch` + `FactoringBatchItem`): select invoices on Load
  Invoices > **Submit to factor**; the factor is the one their customers are factored with. Each must be issued/sent/overdue, unpaid, not already submitted,
  billed to a customer factored with that factor, and have a POD or BOL on its load. The factor gets
  one email (through Resend, with attachments) carrying the schedule PDF, every invoice PDF and the
  loads' POD, BOL and rate confirmation (`DocumentType.Contract`). If the email fails, the schedule
  is removed.
- **Funding**: per invoice, the advance and fee the factor paid (prefilled from its rates) or a
  rejection. Funding records a payment for the invoice total, so the invoice is paid in the
  carrier's books; the reserve is tracked as owed by the factor until released. Schedule status is
  derived: Submitted, Partially funded, Funded, Rejected.
- **Reserve release** and **chargeback**: a chargeback removes the sale payment, puts the invoice
  back to sent/overdue (owed by the customer) and marks it charged back.
- **AR**: the invoice dashboard splits open receivables into owed by customers and owed by the
  factor (awaiting funding plus reserve held).
- **QuickBooks**: with a bank-type factoring clearing account and a fee expense account set, the
  sale payment deposits to the clearing account and the fee posts as a purchase paid from it. A
  chargeback voids the pushed sale payment.

## Not built

- Factor APIs (Triumph, RTS, OTR, Apex are partner-gated) and funded-status webhooks via the
  `add-webhook-handler` skill pattern. No provider seam exists yet; email is the only intake.
- Pushing the advance/reserve bank transfers to QuickBooks.
- A per-invoice timeline on the invoice page (status badge only).

## Acceptance

From a delivered load, one click produces a complete factor submission (invoice + POD) with tracked
status, no re-uploading documents.

## Notes

- 2026-10-14: v1 shipped as described above.
