---
name: add-integration-provider
description: Add a third-party integration provider - a new ELD (Samsara/Motive/Geotab-style), load board (DAT/Truckstop), fuel card (WEX/EFS), or accounting (QuickBooks) vendor, or a whole new integration family. Use when wiring an external vendor API into the platform. Covers the ProviderFactoryBase pattern, options binding, secrets, and the two frontend constant lists that silently break if you update only one.
---

# Add an integration provider

Four families exist today, all on the same shape:

| Family     | Project                                     | Enum                     | Factory                     |
| ---------- | ------------------------------------------- | ------------------------ | --------------------------- |
| ELD / HOS  | `Logistics.Infrastructure.Integrations.Eld` | `EldProviderType`        | `EldProviderFactory`        |
| Load board | `...Integrations.LoadBoard`                 | `LoadBoardType`          | `LoadBoardProviderFactory`  |
| Fuel cards | `...Integrations.FuelCards`                 | `FuelCardProviderType`   | `FuelCardProviderFactory`   |
| Accounting | `...Integrations.Accounting`                | `AccountingProviderType` | `AccountingProviderFactory` |

Every factory derives from `ProviderFactoryBase<TService, TProviderType>` in
`Logistics.Infrastructure.Integrations.Common`. Every registrar calls
`AddProviderIntegration<TOptions, TFactoryService, TFactoryImpl>(configuration, sectionName)`.
**Do not invent a fifth pattern.**

## Decide first

- **Which family?** If none fits, you're adding a _new family_ - see the last section.
- **Does the enum value already exist?** Several do without an implementation (`EldProviderType`
  already has `Omnitracs` and `PeopleNet`). Check before adding one; if it exists, skip step 1.
- **Does the vendor push webhooks, or do we poll?** Push → also use the `add-webhook-handler` skill.
  Poll → the family's existing sync job picks it up, nothing extra.
- **What are the credentials?** Only non-secret, install-wide values (`BaseUrl`) go in `appsettings`.
  Per-tenant `ApiKey` / `ApiSecret` / `WebhookSecret` live on the `{Family}ProviderConfiguration`
  entity and are entered through the connect dialog.

## Backend

### 1. Enum value

`src/Core/Logistics.Domain.Primitives/Enums/{Family}ProviderType.cs`. Add `[Description]` only when
`GetDescription()`'s humanization isn't enough (`TtEld` → `"TT ELD"`).

### 2. Provider implementation

`Providers/{Name}/` inside the family project. The four-file convention (see `Providers/Geotab/`):

- `{Name}EldService.cs` - implements the family's `I{Family}ProviderService`
- `{Name}Mapper.cs` - vendor payload → domain
- `{Name}Models.cs` - vendor DTOs
- `{Name}Options.cs` - this provider's config shape

Add a separate `{Name}Client.cs` only when the vendor isn't a plain REST/JSON API (Geotab does this).

Use `TryGetFromJsonAsync` from `Integrations.Common` for reads - it never throws, it logs and returns
`default`. Do **not** copy it into the family project. (`Integrations.Accounting`'s QuickBooks helpers
deliberately _do_ throw so push failures surface. That is not a duplicate - don't fold them together.)

### 3. Options aggregate

Add the property to the family's options root, e.g. `EldOptions.cs`:

```csharp
public GeotabOptions? Geotab { get; set; }
```

### 4. Factory map

Add the arm to the `ProviderMap` dictionary in `{Family}ProviderFactory.cs`:

```csharp
[EldProviderType.Geotab] = typeof(GeotabEldService),
```

### 5. DI registration

In the family's `Registrar.cs`:

```csharp
services.AddHttpClient<NameEldService>();          // plain REST provider
// or, when the vendor needs its own client:
services.AddHttpClient<NameClient>();
services.AddScoped<NameEldService>();
```

### 6. Configuration

`src/Presentation/Logistics.API/appsettings.json`, the family's section (`"Eld"`, `"Accounting"`;
`"LoadBoard"` and `"FuelCards"` don't exist yet and must be created). Env-var override form is
`{Section}__{Provider}__BaseUrl`.

> Known gap to not repeat: `GeotabOptions` exists in `EldOptions` and in the factory map, but has **no
> `appsettings` section**. Options bind to null and the provider fails at first call with a confusing
> error. Add the section.

## Frontend

### 7. Provider constants — both lists

`projects/tms-portal/src/app/pages/{family}/components/{family}.constants.ts` holds **two** exports:

- `{FAMILY}_PROVIDER_OPTIONS` - what the dropdown offers
- `{FAMILY}_PROVIDER_LABELS` - how a stored value is displayed

Update **both**. Updating only the labels gives a provider that renders but can't be picked (this is
the current state of TtEld); updating only the options gives a pickable provider that shows as a raw
enum string everywhere.

### 8. Regenerate the API client

```bash
bun run gen:api:live      # requires the API running; or gen:api against a fresh swagger.json
```

The constants cast snake_case strings to the **generated** union (`"demo" as EldProviderType`). Skip
the regen after a C# enum change and the cast is a type error.

### 9. Connect dialog

Only if the credential fields differ from the family's existing dialog. It must project into
`<app-provider-connect-dialog>` (`tms-portal/src/app/shared/components/integrations/provider-connect-dialog/`)

- that component owns the dialog chrome, the `[formRoot]`, the submit and the footer. Keep the typed
  form in the feature and reset from `(opened)`. Do not build a fourth standalone dialog.

## Checklist

- [ ] Enum value exists (added, or confirmed already present)
- [ ] `Providers/{Name}/` with service + mapper + models + options
- [ ] Reads go through `TryGetFromJsonAsync`, not a hand-rolled HTTP helper
- [ ] Property added to `{Family}Options`
- [ ] Arm added to the factory's `ProviderMap`
- [ ] `AddHttpClient` / `AddScoped` in the family `Registrar.cs`
- [ ] `appsettings.json` section present (create the family section if missing)
- [ ] **Both** `_PROVIDER_OPTIONS` and `_PROVIDER_LABELS` updated
- [ ] `bun run gen:api` run after the enum change
- [ ] Webhook receiver added via `add-webhook-handler` (push vendors only)
- [ ] Unit test for the mapper against a captured vendor payload
- [ ] `feature-map.md` row updated

## Adding a whole new family

Roadmap items like factoring and EDI will need this. On top of the above:

1. New `src/Infrastructure/Logistics.Infrastructure.Integrations.{X}/` project + solution entry.
2. Port interfaces (`I{X}ProviderService`, `I{X}ProviderFactory`) in
   `src/Core/Logistics.Application.Abstractions/{X}/` - **not** in the infrastructure project.
3. Factory deriving from `ProviderFactoryBase`; registrar using `AddProviderIntegration`.
4. **Architecture tests:** the csproj rule discovers the project off disk automatically, but the
   IL-level boundary rule needs an anchor in `AssemblyAnchors.AllInfrastructure` plus a
   `ProjectReference` in the arch-tests csproj. Without both, the new project is silently unchecked.
5. A `{X}ProviderConfiguration` tenant entity + EF configuration + migration (`migration-creator`).
6. A sync job if the vendor is polled (`add-hangfire-job`).

## Related

- `add-webhook-handler` - inbound webhooks from the vendor
- `add-hangfire-job` - the nightly/interval sync
- `.claude/rules/backend/api-design.md`, `.claude/rules/backend/csharp-conventions.md`
