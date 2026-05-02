# Handoff: Stripe EU Payment Methods & Connect Fixes

> **Priority:** HIGH (billing blocker for EU customers). **Effort:** M (3–5 days).
>
> Stripe `PaymentIntent` already accepts EUR ([StripePaymentService.cs](../../src/Infrastructure/Logistics.Infrastructure.Payments/Stripe/StripePaymentService.cs)) but the Connect onboarding hardcodes US-only assumptions and EU bank-debit / wallet payment methods are not enabled.

## Sequencing

- **Position in overall order:** 2nd
- **Depends on:** Plan #1 ([VAT engine](handoff-vat-tax-engine.md)) — `tax_id_data` on Stripe Customer needs the VAT number. Plan #10 ([Tenant fields](handoff-region-aware-address-and-tenant-fields.md)) for `Tenant.VatNumber` if not done as part of #1.
- **Unblocks:** Real EU revenue collection. Nothing else strictly depends on this.
- **Why second:** Once VAT is on the invoice, the customer must be able to actually pay it via the methods they use (SEPA / iDEAL / Bancontact). Card-only is a non-starter for B2B EU.

## Why now

- [StripeConnectService.cs:63](../../src/Infrastructure/Logistics.Infrastructure.Payments/Stripe/StripeConnectService.cs#L63) hardcodes `Country = "US"` for employee Connect accounts — EU drivers cannot receive payroll
- Tenant Connect creation only requests `UsBankAccountAchPayments` capability — EU tenants can't accept SEPA Direct Debit
- No `vat_id` is collected at Stripe Customer creation, so Stripe-issued invoices for subscriptions won't have it
- Subscription plans are USD-only at the seeder level

## Current state

- [StripeConnectService.cs](../../src/Infrastructure/Logistics.Infrastructure.Payments/Stripe/StripeConnectService.cs) — tenant account uses `GetCountryFromAddress(tenant.CompanyAddress)` ✓ but employee account hardcodes `"US"` ✗
- [StripePaymentService.cs](../../src/Infrastructure/Logistics.Infrastructure.Payments/Stripe/StripePaymentService.cs) — passes `Currency = payment.Amount.Currency.ToLower()` ✓
- [StripeCustomerService.cs](../../src/Infrastructure/Logistics.Infrastructure.Payments/Stripe/StripeCustomerService.cs) — does not pass `tax_id_data`
- [StripePlanService.cs](../../src/Infrastructure/Logistics.Infrastructure.Payments/Stripe/StripePlanService.cs) — verify whether `Price.Currency` is read from plan or hardcoded
- [SubscriptionPlan.cs](../../src/Core/Logistics.Domain/Entities/Subscription/SubscriptionPlan.cs) — verify Money type usage

## Backend

### Domain

- No new entities. Plan #10 adds `Tenant.VatNumber` / `EoriNumber` — depend on it (or inline the field add)
- Add `Employee.Country` (string?) — needed because Stripe Connect employee accounts need a country and we shouldn't infer from tenant alone
- `SubscriptionPlan` — confirm pricing uses `Money` (currency-aware); if not, migrate from `decimal Price` to `Money` so EUR plans are first-class

### Application

- `Commands/Subscription/CreateSubscription` — pass tenant currency to Stripe price selection (one product, multiple prices keyed by currency)
- `Commands/StripeConnect/CreateConnectAccount` — already region-aware for tenants; nothing to change
- `Commands/Payroll/CreateEmployeePayoutAccount` (or wherever employee onboarding triggers) — pass `employee.Country` ?? `tenant.CompanyAddress.Country`

### Infrastructure (Stripe wiring)

- [StripeConnectService.CreateEmployeeConnectedAccountAsync](../../src/Infrastructure/Logistics.Infrastructure.Payments/Stripe/StripeConnectService.cs#L58) — change `Country = "US"` to `Country = employee.Country ?? GetCountryFromAddress(tenantAddress)` (pass tenant address as parameter)
- [StripeConnectService.CreateConnectedAccountAsync](../../src/Infrastructure/Logistics.Infrastructure.Payments/Stripe/StripeConnectService.cs#L25) `Capabilities` — set requested capabilities **conditionally** by tenant region:
  - EU/EEA: `SepaDebitPayments`, `BancontactPayments`, `IdealPayments`, `GiropayPayments`, `SofortPayments`, `Transfers`, `CardPayments`
  - US/CA: `UsBankAccountAchPayments`, `Transfers`, `CardPayments`
  - Add `GetCapabilitiesForCountry(string country)` helper
- [StripeCustomerService.cs](../../src/Infrastructure/Logistics.Infrastructure.Payments/Stripe/StripeCustomerService.cs) — when creating customer, set `TaxIdData` from `tenant.VatNumber` (EU), `customer.TaxId` (load customer)
- New helper `StripePaymentMethodOptions.ForRegion(Region region)` returning the `payment_method_types` array — used in PaymentIntent / Checkout / Invoice creation:
  - EU: `["card", "sepa_debit", "bancontact", "ideal", "giropay", "sofort"]`
  - US: `["card", "us_bank_account"]`
- Update [StripePaymentService.CreatePaymentIntentAsync](../../src/Infrastructure/Logistics.Infrastructure.Payments/Stripe/StripePaymentService.cs#L54) to use that helper, sourced from the **issuing tenant**, not the customer
- Multi-currency Stripe Price IDs: `StripeOptions` should hold a `Dictionary<string,string>` per plan (plan-id → price-id-by-currency); update [StripeOptions.cs](../../src/Infrastructure/Logistics.Infrastructure.Payments/Stripe/StripeOptions.cs)
- [StripePlanService.cs](../../src/Infrastructure/Logistics.Infrastructure.Payments/Stripe/StripePlanService.cs) — add `EnsurePricesExistAsync()` that creates a Stripe Price per supported currency on first run
- Webhooks: [WebhookController.cs](../../src/Presentation/Logistics.API/Controllers/WebhookController.cs) — verify the SEPA-specific events (`payment_intent.processing`, `charge.refunded` for SEPA delays) are handled; SEPA payments are **not instant**, so add a `Pending` payment status branch

### Persistence / migration

- New columns (master DB if Tenant; tenant DB if Employee/Customer):
  - `Tenant.VatNumber`, `Tenant.EoriNumber` (covered by plan #10 — coordinate)
  - `Employee.Country` (default "US" for existing rows)
  - `Customer.TaxId` (covered by plan #1 — coordinate)
- Use `migration-creator` skill once for each DbContext touched

## API

- `EmployeeDto` add `Country`
- `SubscriptionPlanDto` add `PriceByCurrency: Dictionary<string, decimal>`
- Run `bun run gen:api`

## Frontend (Angular)

### Shared

- New `<ui-payment-method-icons>` showing the configured payment methods (card, SEPA, iDEAL, etc.) by region
- Add SVG assets for SEPA / iDEAL / Bancontact / Giropay logos

### TMS portal

- `pages/employees/components/employee-form/` — add Country dropdown (defaulted from tenant address country)
- `pages/settings/billing/` — show "Accepting via: Card, SEPA, iDEAL" badges for EU tenants
- `pages/payroll/` — display SEPA pending state (different copy than ACH instant)

### Customer portal

- `pages/payment/public-payment.html` — Stripe Elements config: pass `payment_method_types` from API instead of card-only
- Test SEPA mandate display — Stripe Elements renders the mandate text but we should preview it before the user clicks

### Admin portal

- `pages/plans/` — multi-currency pricing input (USD + EUR + GBP fields per plan)
- `pages/subscriptions/` — show subscription currency in the listing

## Mobile (driver app)

- `model/CountryCode.kt` — verify enum covers EU; add missing if needed
- Stripe Connect employee onboarding URL is opened in a browser — backend already returns a country-correct link, no client change

## Tests

- Unit test `GetCapabilitiesForCountry`
- Integration test (mocked Stripe HTTP): EU tenant Connect creation → expect SEPA/iDEAL capabilities; US → expect ACH
- E2E (Playwright) — public payment page renders SEPA tab when invoice is EUR

## Acceptance criteria

- [ ] DE tenant onboards → Stripe Connect account is created with `Country=DE` and SEPA + iDEAL + card capabilities
- [ ] DE driver onboards for payroll → employee Connect account is `Country=DE`
- [ ] EUR invoice paid via SEPA → payment lands in DB as `Pending` then `Succeeded` after webhook
- [ ] US tenant unaffected — still gets ACH-only
- [ ] Stripe `tax_id_data` includes tenant `eu_vat`/`gb_vat` ID when set
- [ ] Subscription plan has separate USD and EUR Stripe Price IDs and the right one is selected on tenant signup

## Update [.claude/feature-map.md](../feature-map.md)

Update existing rows for "Stripe Connect" and "Stripe subscriptions" to mention multi-currency / region-aware capabilities. No new row.
