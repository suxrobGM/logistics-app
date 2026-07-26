# Same-Day Carrier Pay (embedded finance)

- **Status**: Planned
- **Priority**: P1 - attacks the 2–4% factoring tax every small carrier hates; competitors can't copy quickly because none own the payment rails
- **Effort**: L
- **Category**: Wedge product

## Why

Factoring companies advance invoices at 2–4% per invoice. We natively own the money flow:
Stripe Connect accounts per tenant AND per employee (`StripeConnectService`, country-aware
capabilities), payment links, checkout, webhooks. "Invoice paid → driver paid, same day" is a
product no bolt-on-factoring competitor can match. DataTruck's Fintruck is a $99/mo add-on.

## What to build

- Phase 1 (low risk, pure plumbing): instant payout flow - when `invoice.paid` lands via Stripe webhook (`ProcessStripEvent` already handles it), trigger same-day driver settlement via Stripe Connect instant payouts instead of the payroll cycle. Tenant opt-in per driver.
- Phase 2 (regulated, needs partner/capital): invoice advances - pay the carrier X% of a verified invoice (delivered + POD on file) before the shipper pays. Start with Stripe Capital / a lending partner rather than our own balance sheet. **Do legal/lending-license review before building.**
- Fee model: undercut factoring (e.g. ~1–1.5%) - becomes a revenue line pricing pages can headline.
- Risk controls: only advance invoices with POD documents attached (`Entities/Document/`), customer payment history score, per-tenant advance limits.
- Cross-link: [factoring-integration](factoring-integration.md) serves carriers locked into factoring contracts; this replaces factoring for the rest.

## Acceptance

Phase 1: a customer paying an invoice via payment link results in the driver's cut arriving in their Stripe Connect account the same day, no payroll-run wait.

## Notes

_(add dated implementation notes here)_
