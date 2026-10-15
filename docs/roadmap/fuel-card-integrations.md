# Fuel Card Integrations (WEX / EFS / Comdata)

- **Status**: Done
- **Priority**: P0 - one of the "4 must-have TMS integrations" (ELD ✓, fuel cards ✓, factoring ✓, accounting ✓)
- **Effort**: M
- **Category**: Table stakes

## Why

Fuel is a carrier's #1 or #2 cost. Auto-importing card transactions eliminates manual expense entry,
feeds [IFTA](ifta-reporting.md) gallons/jurisdiction automatically, and makes our net-profit reporting
real instead of estimate-based. DataTruck ships 30+ telematics integrations and markets transaction visibility.

## What to build

- New provider-pattern project `src/Infrastructure/Logistics.Infrastructure.Integrations.FuelCards/` modeled exactly on `Infrastructure.Integrations.Eld` (factory + per-provider clients + Demo provider for local dev).
- Start with EFS/WEX (largest small-carrier share); Comdata second.
- Nightly Hangfire sync job pulling transactions → create `TruckExpense`/`FuelPurchase` records matched to truck by card/unit number.
- Unmatched-transaction review queue (shipped at `tms-portal/pages/fuel-cards/`).
- Provider config UI mirroring ELD provider settings.
- `TenantFeature.FuelCards` flag.

## Acceptance

A fuel transaction made on a connected card appears as a truck expense with gallons + location within 24h, no manual entry, and flows into IFTA totals.

## Notes

- **2026-07-16** - Shipped: WEX, EFS and a Demo provider; nightly sync plus manual sync; matched
  transactions become paid fuel truck expenses that feed IFTA, unmatched ones wait in the review
  queue at `tms-portal/pages/fuel-cards/`.
- **Remaining:** the real WEX/EFS API contracts are best effort until a partner sandbox confirms
  them; Comdata is deferred.
