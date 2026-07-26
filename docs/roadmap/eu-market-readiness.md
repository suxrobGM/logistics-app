# EU Market Readiness

- **Status**: Planned
- **Priority**: P1 - uncontested ground: every major small-carrier TMS competitor is US-only, and our compliance stack is already built
- **Effort**: M (product gaps) - GTM effort separate
- **Category**: Market expansion

## Why

Already shipped and rare: EU 561/2006 HOS rule set (`RuleSetSelector`), VAT engine (`EuVatRates`,
VAT invoice PDF block), ADR/hazmat dispatch gating, GDPR tooling (data export/deletion/retention jobs),
SEPA/iDEAL/Bacs payouts (`StripeCapabilities`), international address forms, VAT/EORI validation.
European small carriers are stuck with legacy software; positioning there avoids fighting five funded
US competitors head-on.

## What to build (remaining gaps)

- **EUR pricing**: `Money` VO already carries currency - seed EUR plan prices (`SubscriptionPlanSeeder`), create Stripe Price objects per currency, currency-aware pricing page ([pricing-launch-polish](pricing-launch-polish.md)).
- **i18n**: portal translations (DE/NL/PL first - largest road-freight markets). Audit Angular i18n readiness; driver app strings in KMP.
- **EU ELD/tacho providers**: Samsara/Geotab operate in the EU, but digital tachograph data (driver-card downloads) is the EU norm - evaluate a tachograph-data provider integration in `Infrastructure.Integrations.Eld`.
- **WhatsApp driver/customer comms**: the standard channel outside the US; Telegram bot already exists (`Logistics.TelegramBot`) - add WhatsApp Business API alongside.
- **EU load boards**: TIMOCOM / Trans.eu providers in `Infrastructure.Integrations.LoadBoard` (same factory pattern as DAT/Truckstop).
- Marketing: EU-specific landing content on `website` (compliance stack is the story).

## Acceptance

A German carrier can sign up, see EUR pricing, run EU-561-compliant HOS dashboards, invoice with correct VAT, pay drivers via SEPA, and search an EU load board - in German.

## Notes

_(add dated implementation notes here)_
