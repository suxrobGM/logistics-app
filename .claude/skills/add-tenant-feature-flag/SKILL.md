---
name: add-tenant-feature-flag
description: Add a new plan-gated tenant feature flag (e.g. "ContainerTracking", "AdvancedAnalytics") that platform admins control per tenant through presets and per-feature overrides, gated by subscription plan tier. Use when adding a feature that should be locked for some plans, tied to a company type preset, or admin-overridable. Walks through the resolution chain and the preset catalog.
---

# Add a Tenant Feature Flag

Only platform admins change a tenant's features. Tenants read them and cannot toggle them.

`FeatureService.IsFeatureEnabledAsync(tenantId, feature)` returns the first rule that decides:

1. **Admin lock** - a `TenantFeatureConfig` row with `IsAdminLocked = true` returns its `IsEnabled`. This beats the plan.
2. **Plan gating** - a feature the tenant's plan does not grant via `PlanFeature` is off.
3. **Tenant config** - a `TenantFeatureConfig` row returns its `IsEnabled`.
4. **Default** - with no row, a tenant with `IsSubscriptionRequired = false` gets the feature. Every other tenant uses `DefaultFeatureConfig` (a missing default counts as on).

Tenant config rows come from **presets**. `ApplyPresetFeaturesAsync` writes one row per feature from
`TenantPresetCatalog.Resolve` when a tenant is created and when an admin changes its presets. It
skips admin-locked rows. Rows are a snapshot, so a feature added later has no row on existing
tenants and resolves through rule 4.

## When to use this skill

Don't use it for:

- Roles/permissions - those go through `Permission` constants and policy authorization
- Code-level kill switches - use a config flag instead
- A/B experiments - use a different mechanism

## Files that must change

1. `src/Core/Logistics.Domain.Primitives/Enums/Tenant/TenantFeature.cs` - enum value
2. Master DB migration - adds a `default_feature_configs` row, so existing tenants get the intended default
3. `SubscriptionPlanSeeder` - `PlanFeature` rows for the tiers that grant it
4. `src/Core/Logistics.Domain/Entities/Feature/TenantPresetCatalog.cs` - only if a preset should control it
5. Backend: `[RequiresFeature]` on **every** command AND query in the module
6. Backend: any Hangfire job touching the feature - jobs bypass the pipeline and must check explicitly
7. Frontend: feature gate in route guards, components, or services

The admin portal's tenant features page lists every enum value, so it needs no change.

## Step-by-step

### 1. Add the enum value

```csharp
public enum TenantFeature
{
    // existing values
    [Description("ELD / HOS")] Eld,
    ContainerTracking,
}
```

`GetDescription()` auto-humanizes - only add `[Description]` for acronyms or special formatting.

### 2. Migration: add default config

Use the `migration-creator` skill. Insert the platform default in snake_case, which is what
`SnakeCaseEnumConverter` writes:

```csharp
migrationBuilder.Sql("""
    INSERT INTO default_feature_configs (id, feature, is_enabled_by_default)
    VALUES (gen_random_uuid(), 'container_tracking', true)
    ON CONFLICT (feature) DO NOTHING
    """);
```

### 3. Plan gating (if tier-restricted)

Add the feature to the right tier array in `SubscriptionPlanSeeder`. The seeder syncs `PlanFeature`
rows on every run. Enterprise takes every enum value automatically.

### 4. Presets

Decide how the preset catalog treats the feature. Every feature resolves to on unless the catalog
says otherwise:

- **Cargo feature** (only for one company type) - add it to `CargoFeatures` with its preset, like `VehicleTransport` → `CarHauler`.
- **Team feature** a one-person carrier never uses - add it to `SoloExcludedFeatures`.
- Otherwise leave the catalog alone. The plan still limits it.

A new company type needs a `TenantPreset` value, a catalog entry, and the admin portal's preset
options.

### 5. Backend: gate the API

Put `[RequiresFeature]` on the command/query itself. `FeatureCheckBehaviour` enforces it in the
MediatR pipeline:

```csharp
[RequiresFeature(TenantFeature.ContainerTracking)]
public class CreateContainerCommand : ICommand<Result<Guid>>
{
    // ...
}
```

**Gate the queries too, not just the commands.** A half-gated module still serves the data to a
tenant whose plan excludes it.

When the feature is a field **value** on a shared request (a load type, a truck type), the attribute
cannot express it. Check it in the shared write path through a guard that calls
`IFeatureService.CheckFeatureAsync`, as `IVehicleTransportGuard` does from `LoadService`. That keeps the
error codes the same as `[RequiresFeature]`, so the client still shows the upgrade prompt.

### 5b. Backend: gate the jobs

Hangfire jobs **bypass the MediatR pipeline**, so `[RequiresFeature]` is inert there:

```csharp
var featureService = scope.ServiceProvider.GetRequiredService<IFeatureService>();
if (!await featureService.IsFeatureEnabledAsync(tenant.Id, TenantFeature.ContainerTracking))
{
    return;
}
```

Keep the check inside the job body, not in `TenantJobRunner.ForEachTenantAsync`: a job may need part
of its work to run unflagged (`IftaQuarterCloseJob` gates the snapshot but not the breadcrumb purge).

### 6. Frontend: gate the UI

```typescript
const features = inject(FeatureService);
protected readonly canSeeContainers = computed(() => features.isEnabled("container_tracking"));
```

For route-level guards, use `featureGuard` from `@logistics/shared`.

## Verification checklist

- [ ] Enum value added with description if needed
- [ ] Master migration adds the `default_feature_configs` row
- [ ] (If tier-restricted) plan seeder updated
- [ ] Preset catalog decision made
- [ ] `[RequiresFeature]` on every command **and** query in the module
- [ ] Every Hangfire job that touches the feature checks `IsFeatureEnabledAsync`
- [ ] Frontend guards on `FeatureService`
- [ ] Test: a tenant on a plan without the feature is blocked end-to-end
- [ ] Test: an admin lock with `IsEnabled = true` grants it outside the plan

## Common mistakes

- **Forgetting the default config row** - existing tenants have no row for the new feature, so they fall back to the default.
- **Gating only in the UI** - the API still serves the data.
- **Half-gating a module** - writes blocked, reads still serving. Gate every request type.
- **Forgetting the jobs** - a downgraded tenant keeps getting nightly syncs.
- **Expecting a catalog change to reach existing tenants** - rows are a snapshot. Only new tenants and re-applied presets pick it up.

## Related

- `feature-map.md` → Feature flags and Tenant presets entries
- `add-hangfire-job` - if the feature has a background job, the gate goes in the job body
