# Handoff: Region-Aware Routing & Geocoding Defaults

> **Priority:** LOW (polish). **Effort:** S (~1 day).
>
> [Mapbox geocoding/routing](../../src/Infrastructure/Logistics.Infrastructure.Routing/) is global so technically works in both regions, but defaults are US-biased: address autocomplete favors US results, distance computations don't pass a country filter, and there's no per-tenant fallback.

## Sequencing

- **Position in overall order:** 12th (last)
- **Depends on:** Plan #10 ([region-aware addresses](handoff-region-aware-address-and-tenant-fields.md)) — `<ui-address-form>` is the natural place to wire region-biased autocomplete.
- **Unblocks:** Nothing.
- **Why last:** Pure polish. Mapbox already works globally; this is just better defaults. Land any time after #10; safe to defer indefinitely.

## Why now

- EU users typing "Berlin" sometimes get Berlin, NY first
- No fallback if a tenant's data residency requires not-Mapbox (some EU customers have Google Maps contracts)

## Current state

- [Routing/Geocoding/MapboxGeocodingService.cs](../../src/Infrastructure/Logistics.Infrastructure.Routing/Geocoding/MapboxGeocodingService.cs) — does not pass `country` parameter
- Route optimization (Mapbox Matrix) — global; no change needed
- No alternative geocoding provider configured

## Backend

### Application

- `IGeocodingService.SearchAsync(query, country?, language?, CancellationToken)` — add optional country bias
- Existing handlers that call geocoding should pass `tenant.Settings.Region` → list of countries (US tenant biases to US/CA/MX; EU tenant biases to EU country list)
- Add `language` param for localized place names (English vs. German names of cities)

### Infrastructure

- Mapbox forward geocoding endpoint accepts `country=DE,FR,NL,...` — wire it through
- Optional new `GoogleGeocodingService` implementation (if a customer needs it):
  - Same `IGeocodingService` interface
  - Selected via `Geocoding:Provider = mapbox | google`
  - Defer until a tenant actually requires it; just make the interface clean now

### Persistence

- No change

## API

- No DTO changes; geocoding endpoints just accept extra optional params
- Run `bun run gen:api` if new params surface

## Frontend (Angular)

### Shared

- Existing geocoding service ([projects/shared/src/lib/services/](../../src/Client/Logistics.Angular/projects/shared/src/lib/services/)) — pass tenant region/language to backend search calls
- `<ui-address-autocomplete>` (or whatever exists) — already biases to user country; just make sure the query passes through

### Maps

- Mapbox map default center / zoom should follow tenant region:
  - US tenant → center on continental US (lat ~39, lng ~-98, zoom 4)
  - EU tenant → center on Central Europe (lat ~50, lng ~10, zoom 4)
- Tracking pages, dispatch map, customer-portal map — extract default into a single config keyed off region

## Mobile

- N/A directly. The driver app uses native map components (Google Maps on Android, MapKit on iOS) which already use device locale

## Tests

- Geocoding integration test: search "Springfield" with US bias → returns Springfield, IL; with DE bias → returns Springfield in some other context (no false matches)

## Acceptance criteria

- [ ] EU tenant searching "Berlin" gets Berlin, DE as first result
- [ ] US tenant searching "Paris" gets Paris, TX before Paris, France
- [ ] Tracking map defaults to a sensible center per region

## Update [.claude/feature-map.md](../feature-map.md)

Update existing "Geocoding" row description; no new row.
