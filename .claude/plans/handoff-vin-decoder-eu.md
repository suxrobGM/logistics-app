# Handoff: VIN Decoder EU Fallback

> **Priority:** MEDIUM. **Effort:** S (1–2 days).
>
> Existing [NhtsaVinDecoderService](../../src/Infrastructure/Logistics.Infrastructure.Documents/Vin/NhtsaVinDecoderService.cs) calls the US NHTSA `vpic` API. NHTSA only knows US-market VINs reliably. EU-market vehicles return partial data or 404.

## Sequencing

- **Position in overall order:** 7th
- **Depends on:** Nothing.
- **Unblocks:** Nothing strictly. Improves the truck-create UX for EU tenants.
- **Why seventh:** Self-contained, low-risk infrastructure change. Good "filler" between heavier plans — junior-friendly task or a 1-session quick win.

## Why now

- EU tenants adding EU-built trucks (MAN, Scania, DAF, Volvo Trucks Europe, Iveco) get blank decoder results
- Cheap to add a fallback chain — no commercial API needed for the basic case (WMI prefix decoding alone covers make + country of manufacture)

## Current state

- [IVinDecoderService](../../src/Core/Logistics.Application/Services/IVinDecoderService.cs) (verify exact path; pattern is in feature-map under Documents)
- Single implementation: NHTSA HTTP service
- VIN decoding is invoked from truck-create form (frontend hits an API endpoint, returns decoded data)

## Backend

### Domain

- No domain change

### Application

- Confirm contract `IVinDecoderService.DecodeAsync(string vin, CancellationToken)` returns DTO with `Make`, `Model`, `Year`, `BodyClass`, `Manufacturer`, `Country`
- Add property `Source` (string) to the response — populated with which decoder produced the result, useful for UI

### Infrastructure

- Refactor to a chain-of-responsibility / provider list:
  - `WmiPrefixDecoder` (always runs first, free, fast — decodes country + manufacturer from first 3 chars of VIN; static lookup table)
  - `NhtsaVinDecoderService` (existing) — runs if WMI says US/CA/MX
  - `EucarisVinDecoder` (new, optional) — paid commercial service; only if API key configured
  - `VindecoderEuService` (new, optional) — alternative paid service
- New `CompositeVinDecoderService : IVinDecoderService`:
  - Calls in order, merges results (WMI fills make/country, NHTSA/EUCARIS fills detail)
  - First successful detailed response wins after WMI
- WMI table: there are ~3000 prefixes; ship as static JSON resource embedded in assembly. Source: ISO 3779 published list. License-clean to redistribute.
- Config:

  ```yaml
  VinDecoder:
    Providers: ["wmi", "nhtsa", "eucaris"]
    Eucaris:
      ApiKey: ...
  ```

- Register the composite as `IVinDecoderService` in `Registrar.cs`; individual decoders as transient

### Persistence / migration

- No DB change

## API

- `VinDecodeResponseDto` add `Source` field
- Run `bun run gen:api`

## Frontend (Angular)

### TMS portal

- `pages/trucks/truck-form/` — keep current "Decode VIN" button; show small badge "via WMI" / "via NHTSA" / "via EUCARIS" so user knows confidence level
- If only WMI succeeded (no model/year), surface a hint that the user should fill model manually

### Mobile

- N/A (driver app doesn't decode VINs)

## Tests

- `WmiPrefixDecoder` test cases:
  - `1FUJBBCK57LX12345` (Freightliner USA) → make=Freightliner, country=US
  - `WDB9706331L123456` (Mercedes-Benz Germany) → make=Mercedes-Benz, country=DE
  - `YV2RTZ0G1HB123456` (Volvo Trucks Sweden) → make=Volvo Trucks, country=SE
- Composite: NHTSA returns 404 → fall back to WMI-only result, response not empty

## Acceptance criteria

- [ ] EU truck VIN (e.g., MAN starting with `WMA`) decodes to at least make + country with no API key
- [ ] US VIN still decodes via NHTSA with full detail
- [ ] If EUCARIS configured, EU VIN gets full detail
- [ ] Response indicates the source so support can debug "why is model missing"

## Update [.claude/feature-map.md](../feature-map.md)

Update existing "VIN decoder" row to mention WMI + EU fallback. No new row.
