# Fuel Card Integrations (WEX / EFS / Comdata)

- **Status**: Done
- **Priority**: P0 — one of the "4 must-have TMS integrations" (ELD ✓, fuel cards ✓, factoring ✗, accounting ✓)
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

- **2026-07-16**: Shipped on `feat/fuel-integrations`. New `Infrastructure.Integrations.FuelCards/` project (WEX + EFS clients, deterministic Demo provider, factory) behind `IFuelCardProviderService`/`IFuelCardProviderFactory` ports. Transactions stage in `FuelCardTransaction` (unique (provider, external ID) = idempotency; RawPayloadJson kept for disputes), match by `FuelCard` mapping → unit-number auto-match (remembers the mapping) → Pending review queue. Matched transactions materialize as **Paid** `TruckExpense`s (Category=Fuel, gallons + `PurchaseJurisdiction` → feeds IFTA). Nightly `FuelCardSyncJob` (02:00 UTC, 3-day overlap window) + manual sync command. `TenantFeature.FuelCards` (Professional+), `Permission.FuelCard.View/Manage`. UI: `tms-portal/pages/fuel-cards/` (providers + review queue with assign/ignore). `FuelCardSeeder` seeds Demo provider + 90 days of transactions. Real WEX/EFS API contracts are best-effort pending partner sandbox access — Demo provider covers dev/demo; Comdata deferred.
- **Shape worth copying for the next provider**: WEX and EFS both authenticate with a long-lived bearer API key, so the shared plumbing lives in `BearerFuelCardService<TProbeResponse>` — subclasses supply only the base URL, a credential-probe URL (`TProbeResponse` is just the probe's response shape, used to prove the credentials deserialize), and the transaction fetch/mapping. Config follows `LoadBoardOptions`/`EldOptions`: one aggregate `FuelCardsOptions` bound to the `"FuelCards"` section with a nullable entry per provider (`Wex`, `Efs`), read as `options.Value.Wex ?? new WexOptions()` so a provider that isn't configured degrades to empty credentials instead of throwing at startup.
