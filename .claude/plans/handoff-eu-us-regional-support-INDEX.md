# EU/US Regional Support — Handoff Plan Index

> **Created 2026-05-01.** The codebase already has region scaffolding (`Region`/`CurrencyCode`/`DistanceUnit`/`WeightUnit`/`DateFormatType` enums, `TenantSettings`, US/EU `RegionProfile`s, EUR currency in Stripe payment intents, ISO 6346 containers, UN/LOCODE terminals, and a frontend `LocalizationService` that already maps currency/units/dates). The plans below close the remaining gaps so the platform is genuinely usable in both markets.
>
> Work them one plan per session. Each plan is self-contained: backend (domain → application → infrastructure → migration), frontend (shared lib → tms / customer / admin / website portals), and mobile (KMP driver app) where relevant.

## Already done — do NOT redo

- `Tenant.DotNumber` exists ([Tenant.cs:22](../../src/Core/Logistics.Domain/Entities/Tenant.cs#L22)) — only **MC number** and **VAT ID** are missing
- `LocalizationService` ([localization.service.ts](../../src/Client/Logistics.Angular/projects/shared/src/lib/services/localization.service.ts)) handles currency, distance, weight, date format, timezone — extend it, don't rewrite
- Mobile `Settings.kt` already declares a `Language` enum (en/ru/uz) — wire translations to it instead of adding a parallel system
- US/EU `RegionProfile` seed real data including ports, terminals, addresses — extend, don't replace
- Stripe `PaymentIntent` already passes `payment.Amount.Currency.ToLower()` — multi-currency works for one-off charges; subscriptions and Connect onboarding still need fixes

## High priority (legal / billing blockers for EU launch)

| #   | Plan                                                                        | Effort | Why                                                                                                |
| --- | --------------------------------------------------------------------------- | ------ | -------------------------------------------------------------------------------------------------- |
| 1   | [VAT & tax engine](handoff-vat-tax-engine.md)                               | L      | EU invoices legally require VAT. Cross-border B2B reverse charge. No tax logic exists today.       |
| 2   | [Stripe EU payment methods + Connect fixes](handoff-eu-payment-methods.md)  | M      | Hardcoded `Country = "US"` on employee accounts; no SEPA/iDEAL/Bancontact; no `vat_id` collection. |
| 3   | [GDPR data export, consent, retention](handoff-gdpr-compliance.md)          | M      | Required before any EU customer data is processed.                                                 |
| 4   | [EU tachograph & EC 561/2006 HOS rules](handoff-eu-tachograph-hos.md)       | XL     | All ELD providers are US FMCSA. EU drivers need different rules and providers.                     |
| 5   | [EU load board integrations (Timocom, Trans.eu)](handoff-eu-load-boards.md) | L      | DAT/Truckstop/123Loadboard are US-only.                                                            |

## Medium priority (regional usability)

| #   | Plan                                                                                        | Effort | Why                                                                                        |
| --- | ------------------------------------------------------------------------------------------- | ------ | ------------------------------------------------------------------------------------------ |
| 6   | [Multi-language i18n (UI translations)](handoff-i18n-multi-language.md)                     | L      | All Angular labels hardcoded English. Mobile already has the enum but no resource strings. |
| 7   | [Driver licensing + ADR / Hazmat](handoff-driver-licensing-and-adr.md)                      | M      | No CDL/EU category tracking; no ADR cert / Hazmat endorsement on `Employee`/`Truck`.       |
| 8   | [VIN decoder EU fallback](handoff-vin-decoder-eu.md)                                        | S      | NHTSA only knows US-market VINs.                                                           |
| 9   | [Fuel volume + temperature units](handoff-fuel-volume-temperature-units.md)                 | S      | Gallons/Liters, F/C for reefer cargo, MPG ↔ L/100km.                                       |
| 10  | [Region-aware addresses + Tenant MC/VAT](handoff-region-aware-address-and-tenant-fields.md) | M      | `Address.State` always required. Tenant missing MC number, VAT ID, EORI.                   |

## Low priority (US-side completeness, polish)

| #   | Plan                                                                         | Effort | Why                                                                                 |
| --- | ---------------------------------------------------------------------------- | ------ | ----------------------------------------------------------------------------------- |
| 11  | [IFTA fuel-tax reporting (US/Canada)](handoff-ifta-fuel-tax.md)              | L      | Quarterly fuel-by-jurisdiction reporting, US-only — but blocking some US customers. |
| 12  | [Region-aware routing & geocoding defaults](handoff-region-aware-routing.md) | S      | Mapbox biasing, country code in geocoding, optional Google fallback.                |

## Suggested order

1, 2, 3 (legal unblock) → 6 (i18n before more pages ship) → 10 (address shape touches every form) → 7, 8, 9 (domain polish) → 5 (EU load boards, well-scoped) → 4 (largest effort, partner-friendly) → 11, 12 (last).

## Conventions every plan follows

- File paths use `[name](relative/path)` markdown links
- Lists every layer: **Domain → Application → Infrastructure → EF migration → API DTO/controller → Angular shared lib → portal pages → mobile (if applicable) → tests**
- Calls out cross-references to `.claude/feature-map.md` rows so the map can be updated when the work lands
- Marks **breaking changes** explicitly (e.g., `Address.State` becoming nullable)
- Uses the `migration-creator` skill for EF migrations and `scaffold-feature` skill where domain entities are added
