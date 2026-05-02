# Handoff: EU Load Board Integrations (Timocom, Trans.eu)

> **Priority:** HIGH (revenue blocker — primary customer acquisition channel in EU). **Effort:** L (~1 week per provider).
>
> Existing [load-board integration](../../src/Infrastructure/Logistics.Infrastructure.Integrations.LoadBoard/) covers DAT, Truckstop, 123Loadboard — all US-only. EU dispatchers rely on Timocom (DACH/Benelux dominant) and Trans.eu (Poland/CEE).

## Sequencing

- **Position in overall order:** 9th
- **Depends on:** Plan #6 ([i18n](handoff-i18n-multi-language.md)) — provider config screens need German/Polish copy. Plan #7 ([driver licensing + ADR](handoff-driver-licensing-and-adr.md)) — booking ADR loads pulls eligibility checks from #7's `IDispatchEligibilityService`. Plan #9 ([volume units](handoff-fuel-volume-temperature-units.md)) is helpful for distance unit handling on listings (km vs mi).
- **Unblocks:** Real EU customer acquisition.
- **Why ninth:** High value but Phase 2 (Trans.eu) is its own ~1 week. Prefer landing Timocom alone first to validate the booking workflow, then Trans.eu in a follow-up session.

## Why now

- EU tenants have no way to find loads — load board is one of the most-used pages in TMS
- Both providers offer documented APIs (Timocom via TC Truck&Cargo API; Trans.eu has REST + websocket APIs)
- Architecture already supports plug-in providers — proven pattern with 3 working US implementations

## Current state

- [LoadBoardProviderType](../../src/Core/Logistics.Domain.Primitives/Enums/LoadBoardProviderType.cs) — Dat, Truckstop, OneTwo3, Demo
- [Infrastructure.Integrations.LoadBoard/Providers/](../../src/Infrastructure/Logistics.Infrastructure.Integrations.LoadBoard/Providers/) — Dat, OneTwo3, Truckstop, DemoLoadBoardService
- [LoadBoardListing.cs](../../src/Core/Logistics.Domain/Entities/LoadBoard/LoadBoardListing.cs) — generic enough to hold EU listings (verify `Currency`, `Distance`, `Weight` are not `decimal` with assumed units)

## Backend

### Domain

- Add enum values to [LoadBoardProviderType](../../src/Core/Logistics.Domain.Primitives/Enums/LoadBoardProviderType.cs): `Timocom`, `TransEu`
- Verify `LoadBoardListing` has fields applicable to EU loads:
  - `Currency` (already on `Money` if used; check `RatePerMile` vs `RatePerKm`)
  - `LoadingMeters` (LDM, EU-specific cargo space measurement) — likely missing, add as `decimal?`
  - `EquipmentType` enum — confirm it covers EU types: `Tautliner`, `MegaTrailer`, `JumboTrailer`, `Walking Floor`, `Tipper`, `Refrigerated`
  - Customs / ADR flag on the listing
- Verify `Distance` is stored unit-agnostic (km vs mi) — if not, add `DistanceUnit` to the listing

### Application

- No new commands; existing `Commands/LoadBoard/SyncListings`, `BookLoad` work via the provider abstraction
- Add validation: tenant in EU region cannot configure Truckstop (and vice versa) — soft warning, not hard block (migration use cases)

### Infrastructure (per provider)

#### Timocom (Phase 1)

- New folder `src/Infrastructure/Logistics.Infrastructure.Integrations.LoadBoard/Providers/Timocom/`:
  - `TimocomLoadBoardService : ILoadBoardService`
  - `TimocomClient` — HTTP client, OAuth2 (client credentials grant)
  - `TimocomModels.cs` — request/response DTOs (use Timocom OpenAPI spec or hand-write)
  - `TimocomMapper.cs` — `ToListing(TimocomFreightOffer)` etc.
  - `TimocomOptions.cs`
- Endpoints we need:
  - `GET /freight-search/v3/searches` — search (filters: pickup/dropoff country + radius, vehicle, body type, length in LDM)
  - `POST /freight-bookings/v1/bookings` — book
  - Webhook for new offers matching saved search
- Register in [Registrar.cs](../../src/Infrastructure/Logistics.Infrastructure.Integrations.LoadBoard/Registrar.cs)
- Add to [LoadBoardProviderFactory](../../src/Infrastructure/Logistics.Infrastructure.Integrations.LoadBoard/) (the factory file alongside providers)

#### Trans.eu (Phase 2)

- Same structure: `Providers/TransEu/`
- Trans.eu uses API key + per-request signing; look at their developer docs (API v2)
- Worth supporting their websocket for live offers — a `TransEuOfferStream` background service that publishes to `LoadBoardListing` table on new offers

### Persistence / migration

- New columns on `LoadBoardListing`:
  - `LoadingMeters` (decimal?)
  - `IsAdr` (bool, default false)
  - `IsCustoms` (bool, default false)
  - `DistanceUnit` (enum) — or convert all incoming distances to km/mi based on tenant
- `LoadBoardConfiguration` already has provider-agnostic credentials; verify `WebhookSecret` is encrypted at rest

## API

- `LoadBoardListingDto` — add new fields
- `LoadBoardConfigurationDto` — no shape change; just allow new provider types
- Run `bun run gen:api`

## Frontend (Angular)

### TMS portal

- `pages/load-board/load-board-search/` — add filters that only show when EU provider is selected:
  - Loading meters (LDM) range
  - ADR class dropdown
  - Customs (yes/no/either)
  - EU equipment types
- `pages/load-board/loadboard-providers/` — add Timocom + Trans.eu cards with credential setup forms
- `pages/load-board/components/listing-card/` — render LDM / ADR badges when present
- For region-aware default: when tenant is EU and load board is empty, suggest connecting Timocom in an empty state

### Admin portal

- No direct changes (load boards are tenant-configured)

## Mobile (driver app)

- N/A — driver app doesn't surface load board listings (dispatcher-only feature)

## Tests

- `tests/Logistics.Infrastructure.Integrations.LoadBoard.Tests/` — add per-provider mapper tests (sample JSON fixtures saved in `Resources/`)
- Integration test with WireMock-style HTTP stub for OAuth flow

## Acceptance criteria

- [ ] DE tenant configures Timocom credentials → listings appear within 1 minute
- [ ] LDM and ADR badges show on EU listings, hidden for US listings
- [ ] Booking through Timocom creates a `Load` row and reflects status updates from webhook
- [ ] US tenant unaffected; existing DAT/Truckstop/123Loadboard tests still green
- [ ] Trans.eu websocket stream survives reconnect (test with manual disconnect)

## Update [.claude/feature-map.md](../feature-map.md)

Update existing "Load board search" row to mention EU providers, add a row only if a new entity is introduced (none in this plan).
