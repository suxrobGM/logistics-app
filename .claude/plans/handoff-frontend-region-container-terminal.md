# Handoff: Frontend Phases 6–8 for Region + Container + Terminal Foundation

## Context

The backend foundation for European market expansion + intermodal Container/Terminal entities is **fully shipped** across three commits on `main`:

- **`2c611657`** — Phase 1+2: Domain (entities, enums, state machine), Persistence (EF configurations, migrations Tenant `Version_0004` + Master `Version_0003`), Permissions
- **`2e3a0487`** — Phase 3+4+5: Application (DTOs, mappers, CQRS, Load command extensions), Controllers (`ContainerController`, `TerminalController`), AI dispatch prompt update
- **(uncommitted, in working tree)** — Endpoint refactor to REST-friendly verbs + paths (see "Refactor Already Applied" below)

Reference plan: `C:\Users\admin\.claude\plans\don-t-make-it-complicate-quizzical-flurry.md`

## Backend Surface Available to Consume

The API exposes these new endpoints (all protected by `Container` / `Terminal` / `Load` permissions, granted to Owner/Manager/Dispatcher roles):

**Containers** ([src/Presentation/Logistics.API/Controllers/ContainerController.cs](src/Presentation/Logistics.API/Controllers/ContainerController.cs))

- `GET /containers` — paged, supports `Search`, `OrderBy`, `Status`, `IsoType`, `CurrentTerminalId` filters
- `GET /containers/{id}`
- `POST /containers`
- `PUT /containers/{id}` — non-status fields only
- `PUT /containers/{id}/status` — body `{ targetStatus, terminalId? }`. Single command dispatches to all 6 lifecycle transitions; `terminalId` required when transitioning to `AtPort` or `Returned`
- `PUT /containers/{id}/terminal` — body `{ terminalId }`. Pure location update, no status change

**Terminals** ([src/Presentation/Logistics.API/Controllers/TerminalController.cs](src/Presentation/Logistics.API/Controllers/TerminalController.cs))

- `GET /terminals` — paged, supports `Search`, `OrderBy`, `Type`, `CountryCode` filters
- `GET /terminals/{id}`
- `POST /terminals`
- `PUT /terminals/{id}`

**Load (extended)** ([src/Presentation/Logistics.API/Controllers/LoadController.cs](src/Presentation/Logistics.API/Controllers/LoadController.cs))

- `PUT /loads/{loadId}/container` — body `{ containerId }`. Sets `Load.ContainerId` (FK lives on Load). New endpoint added in the refactor.
- All existing `POST /loads` and `PUT /loads/{id}` endpoints now accept additional fields: `Source`, `RequestedPickupDate`, `RequestedDeliveryDate`, `Notes`, `ContainerId`, `OriginTerminalId`, `DestinationTerminalId`
- `POST /loads/{id}/proximity` body changed from `{ canConfirmPickUp?, canConfirmDelivery? }` to single `{ isInProximity }` — `canConfirmPickUp` / `canConfirmDelivery` survive on the response DTO as computed values

**LoadDto new fields:** `IsInProximity`, `Source`, `RequestedPickupDate`, `RequestedDeliveryDate`, `Notes`, `ContainerId`, `ContainerNumber`, `ContainerIsoType`, `OriginTerminalId`, `OriginTerminalName`, `OriginTerminalCode`, `DestinationTerminalId`, `DestinationTerminalName`, `DestinationTerminalCode`. Legacy `CanConfirmPickUp` / `CanConfirmDelivery` are still present (now server-computed from `IsInProximity` + `Status`).

**TenantSettings new field:** `Region` (`Us` | `Eu`), defaults to `Us`.

**New enums available client-side after `bun run gen:api`:** `Region`, `LoadSource`, `ContainerIsoType`, `ContainerStatus`, `TerminalType`. `LoadType` and `TruckType` have new values added.

## Refactor Already Applied (uncommitted)

While prepping the handoff, the controller endpoints were tightened to follow the project's `api-design.md` (no hyphens, custom actions as sub-resources, REST verbs):

- `POST /containers/{id}/status` → **`PUT /containers/{id}/status`**
- `POST /containers/{id}/move-to-terminal` → **`PUT /containers/{id}/terminal`** (renamed action: `SetContainerTerminal`)
- `POST /containers/{id}/link-to-load` → **moved to LoadController as `PUT /loads/{loadId}/container`** (renamed action: `SetLoadContainer`). FK lives on Load, so the relationship is owned there.

Internal command names left as-is (`MoveContainerToTerminalCommand`, `LinkContainerToLoadCommand`) since they're internal contracts — only the routes/verbs/action names changed. **Commit this with phases 6–8 work or as its own small commit** before continuing.

## What's Left (Phases 6–8)

### Phase 6 — TMS Portal (Angular)

Working dir: `src/Client/Logistics.Angular/projects/tms-portal/src/app/`

**6.1 — Regenerate API client first.** From the workspace root: `bun run gen:api`. This is the unblocking step — every subsequent file needs the new generated DTOs/services for `Container`, `Terminal`, and the extended `Load`. Verify `projects/shared/src/lib/api/generated/` now contains `ContainerService`, `TerminalService`, and updated `LoadService` with the `setLoadContainer` / proximity changes.

**6.2 — Sidebar navigation.** Edit [shared/layout/sidebar/sidebar-items.ts](src/Client/Logistics.Angular/projects/tms-portal/src/app/shared/layout/sidebar/sidebar-items.ts) — inside the `dispatch` section, after the existing `loadboard` item, append a new expandable "Intermodal" item:

```typescript
{
  id: "intermodal",
  label: "Intermodal",
  icon: "pi pi-anchor",
  // feature: "intermodal",  // omitted in v1 — no feature gating yet
  children: [
    { id: "intermodal-containers", label: "Containers", route: "/containers" },
    { id: "intermodal-terminals",  label: "Terminals",  route: "/terminals"  },
  ],
},
```

**6.3 — `pages/containers/` feature folder.** Mirror `pages/loads/` structure:

- `container.routes.ts` — lazy routes for list, add, edit
- `containers-list/containers-list.ts` + `.html` — paginated table (PrimeNG `p-table`), filter controls for `Status` (ContainerStatus enum) and `IsoType` (ContainerIsoType enum), columns: Number, ISO type, Status, Current Terminal Code, Laden, Created
- `container-add/container-add.ts` + `.html` — reactive form: number (length-11 validator), ISO type dropdown, seal/booking/BOL inputs, gross weight, laden toggle, current terminal autocomplete (calls `GET /terminals`)
- `container-edit/container-edit.ts` + `.html` — same form bound to existing container; status changes go through the dedicated `PUT /containers/{id}/status` action button or a small modal that calls `setStatus(targetStatus, terminalId?)`
- `store/containers-list.store.ts` — `@ngrx/signals` store mirroring `loads-list.store.ts`

**6.4 — `pages/terminals/` feature folder.** Same shape as containers:

- `terminal.routes.ts`
- `terminals-list/terminals-list.ts` + `.html` — table columns: Code, Name, Country, Type, City; filters by `Type` and `CountryCode`
- `terminal-add/terminal-add.ts` + `.html` — name, code (length-5 validator), country code dropdown, type dropdown, address (use existing address-autocomplete if available)
- `terminal-edit/terminal-edit.ts` + `.html`
- `store/terminals-list.store.ts`

**6.5 — Register new routes** in [app.routes.ts](src/Client/Logistics.Angular/projects/tms-portal/src/app/app.routes.ts) — two lazy-loaded route blocks: `containers` and `terminals`. Mirror the `loads` registration pattern.

**6.6 — Extend `pages/loads/load-add/` and `load-edit/`.** Add to the existing reactive form:

- New "Schedule" section with `RequestedPickupDate` and `RequestedDeliveryDate` PrimeNG calendar pickers (server enforces delivery >= pickup)
- New "Container" section: optional container picker (autocomplete from `/containers` matching ISO number prefix)
- New origin/destination Terminal pickers (autocomplete from `/terminals`)
- New Source dropdown (default `Manual`)
- New Notes textarea (max 2000 chars)

The submit handler passes new fields through `CreateLoadCommand` / `UpdateLoadCommand` (the generated client already has them).

**6.7 — Optional `loads-list/` enhancements.** Add an optional Container # column (off by default behind a column toggle), and a Source filter. Low priority — skip if time-constrained.

**6.8 — Country dropdown filtering helper.** New file `src/Client/Logistics.Angular/projects/tms-portal/src/app/shared/utils/region-countries.ts` — mirror the backend `RegionCountries` set:

```typescript
import { Region } from "@logistics/shared/api/models";

const US_COUNTRIES = ["US"];
const EUROPEAN_COUNTRIES = [
  // EU-27
  "AT",
  "BE",
  "BG",
  "HR",
  "CY",
  "CZ",
  "DK",
  "EE",
  "FI",
  "FR",
  "DE",
  "GR",
  "HU",
  "IE",
  "IT",
  "LV",
  "LT",
  "LU",
  "MT",
  "NL",
  "PL",
  "PT",
  "RO",
  "SK",
  "SI",
  "ES",
  "SE",
  // EEA / EFTA
  "IS",
  "LI",
  "NO",
  "CH",
  // United Kingdom
  "GB",
  // Western Balkans
  "AL",
  "BA",
  "ME",
  "MK",
  "RS",
  "XK",
  // Eastern Europe (selected)
  "MD",
  "UA",
  // Microstates
  "AD",
  "MC",
  "SM",
  "VA",
];

export function regionAllowedCountries(region: Region): readonly string[] {
  return region === Region.Us ? US_COUNTRIES : EUROPEAN_COUNTRIES;
}
```

Apply this filter to the Country dropdown in: `customer-add` / `customer-edit`, `terminal-add` / `terminal-edit`, `employee-add` (if present), `tenant company settings` form, and the address inputs on `load-add` / `load-edit`. Pull current tenant region from the existing tenant-context store/service (look at `core/services/tenant.service.ts`).

**6.9 — Region-aware default map center.** [shared/components/maps/direction-map/direction-map.ts:50](src/Client/Logistics.Angular/projects/tms-portal/src/app/shared/components/maps/direction-map/direction-map.ts#L50) currently hardcodes `defaultCenter: LngLatLike = [-95, 35]` and `defaultZoom = 3`. Make region-aware:

- Inject the tenant-context service
- Compute defaults from `tenant.settings.region`:
  - `Region.Us` → center `[-95, 35]`, zoom `3` (existing)
  - `Region.Eu` → center `[10, 50]`, zoom `4` (central Europe)
- Apply at component init **and** in the `reset()` method that currently calls `this.center.set(this.defaultCenter)` (line 344)

Only `direction-map.ts` has a hardcoded center — `geolocation-map.ts` renders provided coordinates. Single change covers it.

### Phase 7 — Admin Portal (Angular)

Working dir: `src/Client/Logistics.Angular/projects/admin-portal/src/app/`

**7.1 — Regenerate admin-portal API client.** Same `bun run gen:api` (admin portal shares the workspace; check whether it re-emits or has a separate generation target).

**7.2 — Region dropdown in tenant-form.** [shared/components/tenant-form/tenant-form.ts](src/Client/Logistics.Angular/projects/admin-portal/src/app/shared/components/tenant-form/tenant-form.ts) — add a Region field (US / EU) bound to `tenant.settings.region`. Reuse the generated `Region` enum.

**7.3 — Pass Region through update payload** in [pages/tenants/tenant-edit/tenant-edit.ts](src/Client/Logistics.Angular/projects/admin-portal/src/app/pages/tenants/tenant-edit/tenant-edit.ts).

**7.4 — Region column in tenants-list.** [pages/tenants/tenants-list/tenants-list.ts](src/Client/Logistics.Angular/projects/admin-portal/src/app/pages/tenants/tenants-list/tenants-list.ts) — add a column displaying `tenant.settings.region`.

### Phase 8 — DriverApp (Kotlin Multiplatform)

Working dir: `src/Client/Logistics.DriverApp/composeApp/src/commonMain/kotlin/com/logisticsx/driver/`

**8.1 — Regenerate Kotlin OpenAPI client** (`./gradlew generateOpenApi` or whatever the project uses — check `composeApp/build.gradle.kts`). This picks up the new `Container` / `Terminal` types, the extended `LoadDto`, the proximity request shape change, and the new `setLoadContainer` endpoint.

**8.2 — `ui/screens/LoadDetailScreen.kt`** — when present, render: container number + ISO type, origin/destination terminal name+code, requested pickup/delivery dates.

**8.3 — `ui/components/LoadCard.kt`** — show container number badge if `loadDto.containerNumber != null`.

**8.4 — `viewmodel/LoadDetailViewModel.kt`** — proximity update sends single `isInProximity` field on the request body. The existing `canConfirmPickUp` / `canConfirmDelivery` UI paths keep working unchanged because those values are computed server-side and still on the response DTO.

## Verification (end-to-end)

After each phase:

1. `dotnet run --project src/Aspire/Logistics.Aspire.AppHost` — bring up backend stack
2. `bun run start:tms` — launch TMS portal at `https://localhost:7003`
3. Smoke flow:
   - Admin portal → create or edit a tenant → set Region = EU → save and reload
   - As that tenant in TMS portal → `/terminals` → create one (e.g. `BEANR` Antwerp Gateway) → list shows it
   - `/containers` → create a container linked to the terminal → status `AtPort` → list shows it
   - `/loads/add` → fill Schedule + Container + Origin Terminal sections → save → load detail shows everything
   - `PUT /loads/{id}/container` linkage works (e.g. via container detail's "link to load" action)
   - Status transition: container `Empty` → `Loaded` (200), `Empty` → `Delivered` (400 illegal)
4. DriverApp build + smoke: open a load with a container, verify detail screen renders new fields
5. `dotnet test` — existing test suite still green

## Tips for the Next Session

- **Start with `bun run gen:api`** — without the regenerated client, every Angular file you write will lack the new types and you'll re-do imports later
- The existing `pages/loads/` Angular folder is the closest reference for new feature folders. `pages/customers/` is even simpler if you want a smaller template
- For the Container status modal, a single PrimeNG `p-dialog` with a status dropdown plus a conditional terminal autocomplete (shown only when target is `AtPort` or `Returned`) is enough — mirrors the backend's single command shape
- All form fields should use `<ui-labeled-field>` per the workspace `CLAUDE.md`
- Server-side address-region validation was **deliberately not implemented** — the country-dropdown filter in step 6.8 is the only enforcement. If a future requirement needs hard server enforcement, the right place is a `SaveChangesInterceptor` on `TenantDbContext` checking addresses against the resolved tenant region (one place, runs once per commit, no per-validator cost)

## Total Remaining Effort

Rough estimate: 30–50 new/modified files. Phase 6 is the largest by far (~80% of the work). Phase 7 is small (3 files). Phase 8 depends on how many DriverApp screens need touching.
