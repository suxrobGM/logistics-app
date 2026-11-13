---
name: add-integration-provider
description: Add a third-party integration provider - a new ELD (Samsara/Motive/Geotab-style), load board (DAT-style), fuel card (WEX/EFS) or accounting (QuickBooks) vendor, or a whole new integration family. Use when wiring an external vendor API. Covers keyed-service registration, options, secrets, and the frontend constants that break silently when only one list is updated.
---

# Add an integration provider

All families live in `src/Infrastructure/Logistics.Infrastructure.Integrations/` and share one shape. Providers are added per pilot carrier, so no list is final.

| Family | Folder | Enum | Factory |
| --- | --- | --- | --- |
| ELD / HOS | `Eld/` | `EldProviderType` | `EldProviderFactory` |
| Load board | `LoadBoard/` | `LoadBoardProviderType` | `LoadBoardProviderFactory` |
| Fuel cards | `FuelCards/` | `FuelCardProviderType` | `FuelCardProviderFactory` |
| Accounting | `Accounting/` | `AccountingProviderType` | `AccountingProviderFactory` |

Each provider is a keyed service under its enum value, resolved through `GetRequiredProvider` / `HasProvider` and registered with `AddProviderIntegration` + `AddKeyedProvider` (`Integrations/Common/`). Don't invent another pattern.

## Before you start

- The enum value may already exist without an implementation (`EldProviderType.Omnitracs`). Check.
- Push webhooks? Also use `add-webhook-handler`. Polling? The family's sync job picks the provider up.
- Per-tenant credentials (`ApiKey`, `ApiSecret`, `WebhookSecret`) live on the `{Family}ProviderConfiguration` entity, entered in the connect dialog. Only install-wide values like `BaseUrl` go in appsettings.

## Backend

1. **Enum value** in `src/Core/Logistics.Domain.Primitives/Enums/Integrations/{Family}/` (ELD: `Enums/Compliance/Eld/`). `[Description]` only when humanizing isn't enough (`TtEld` → "TT ELD").
2. **`Providers/{Name}/`** in the family folder, copying `Eld/Providers/Geotab/`: `{Name}{Family}Service.cs` (implements `I{Family}ProviderService`), `{Name}Mapper.cs`, `{Name}Models.cs`, `{Name}Options.cs`. A `{Name}Client.cs` only for non-REST vendors.
   - Reads use `TryGetFromJsonAsync` (logs and returns `default`, never throws). QuickBooks helpers throw on purpose so push failures surface; load board providers use `LoadBoardHttp` behind `ILoadBoardProviderGateway` so errors show per provider. Don't fold these together.
   - Retries come from the host resilience handler (GET only). Don't add another.
3. **Options:** add `public {Name}Options? {Name} { get; set; }` to the family options, and read it as `options.Value.{Name} ?? new {Name}Options()` so a missing section falls back to defaults.
4. **Register** in the family `Registrar.cs`. Without the keyed line the factory throws `NotSupportedException`:

   ```csharp
   services.AddHttpClient<NameEldService>();
   services.AddKeyedProvider<IEldProviderService, NameEldService>(EldProviderType.Name);
   ```

5. **Config:** non-default values go in the family's section of `Logistics.API/appsettings.json` (create `Eld` / `FuelCards` if missing). Env override: `{Section}__{Provider}__BaseUrl`.
6. **Test** the mapper against a captured vendor payload.

## Frontend

Constants in `tms-portal/src/app/pages/{family}/components/{family}.constants.ts`. Updating only one list gives a provider you can't pick, or one that shows as a raw enum string.

- **ELD:** `ELD_PROVIDER_OPTIONS` (dropdown), `ELD_PROVIDER_LABELS` (display), `ELD_WEBHOOK_PROVIDERS` (push vendors; shows the webhook URL).
- **Fuel cards:** `FUEL_CARD_PROVIDER_OPTIONS` only.
- **Load boards:** `PROVIDER_LABELS` (typed `Record`, so a gap is a type error) and `PROVIDER_OPTIONS`.

Run `bun run gen:api` (or `gen:api:live`) after the enum change, or the casts to the generated union fail. A new credential layout projects into `<app-provider-connect-dialog>`; never a standalone dialog.

Add the provider to `.claude/feature-map.md`.

## A new family

New folder `Integrations/{X}/` with its own `Registrar`, called from `AddIntegrationsInfrastructure` (no new project, so no arch-test changes). Port interfaces go in `Application.Abstractions/Integrations/`. Add a `{X}ProviderConfiguration` entity and migration (`migration-creator`) and a sync job if polled (`add-hangfire-job`).
