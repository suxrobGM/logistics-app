# Factoring Integration

- **Status**: Planned
- **Priority**: P0 - most small carriers factor invoices; a TMS that can't submit to the factor forces double data entry
- **Effort**: M
- **Category**: Table stakes

## Why

Factoring companies (RTS, OTR Capital, Triumph, TAFS) advance 95–98% of invoice value same-day.
Competitors auto-submit invoice + rate con + POD/BOL to the factor. We have all the documents
(`Entities/Document/`, POD/BOL PDFs via QuestPDF) - the gap is the submission pipeline.

Long-term, [same-day-pay](same-day-pay.md) attacks factoring itself; this integration is the bridge
for carriers already locked into factoring contracts.

## What to build

- "Submit to factor" action on invoices: bundles `LoadInvoice` PDF + attached POD/BOL from `Infrastructure.Documents` into the factor's intake (email-based intake first - universally supported; API integrations per-factor later).
- Factor profile per tenant (name, intake email/API creds, advance rate) under company settings.
- Invoice status extension: `SubmittedToFactor` / `Funded` states + timeline on the invoice page.
- Batch submission from `tms-portal/pages/invoices/`.
- Later: RTS/OTR APIs, funded-status webhooks via the `add-webhook-handler` skill pattern.

## Acceptance

From a delivered load, one click produces a complete factor submission (invoice + POD) with tracked status, no re-uploading documents.

## Notes

_(add dated implementation notes here)_
