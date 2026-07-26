---
name: add-tenant-feature-flag
description: Add a new plan-gated tenant feature flag (e.g. "ContainerTracking", "AdvancedAnalytics") that can be toggled per-tenant and gated by subscription plan tier. Use when adding a feature that should be: locked for some plans, opt-in per tenant, or admin-overridable. Walks through the four-tier resolution chain.
---

# Add a Tenant Feature Flag

`FeatureService.IsEnabledAsync(feature)` walks four tiers and returns the first that decides:

1. **Admin-locked override** - super admin set `IsAdminLocked = true`
2. **Plan gating** - the tenant's plan grants it via `PlanFeature` **and** no negative `TenantFeatureConfig` override exists
3. **Tenant config** - the tenant explicitly enabled/disabled it
4. **Default config** - `DefaultFeatureConfig` for the platform

Tenants with `IsSubscriptionRequired = false` (internal, demo) **bypass plan gating entirely** - a
feature gated only by `PlanFeature` will not work for them.

## When to use this skill

Don't use it for:

- Roles/permissions - those go through `Permission` constants and policy authorization
- Code-level kill switches - use a config flag instead
- A/B experiments - use a different mechanism

## Files that must change

1. `src/Core/Logistics.Domain.Primitives/Enums/Tenant/TenantFeature.cs` - enum value
2. Master DB migration - adds row to `DefaultFeatureConfig` table for the new feature
3. (Optional) Update `SubscriptionPlan` seeders / `PlanFeature` rows to grant the feature to specific tiers
4. Backend: `[RequiresFeature]` on **every** command AND query in the module
5. Backend: any Hangfire job touching the feature - jobs bypass the pipeline and must check explicitly
6. Frontend: feature gate in route guards, components, or services
7. Admin portal: feature toggles UI (usually picks up the new enum value automatically)
8. TMS portal AI Settings or other surfaces: respect the gate

## Step-by-step

### 1. Add the enum value

`src/Core/Logistics.Domain.Primitives/Enums/Tenant/TenantFeature.cs`

```csharp
public enum TenantFeature
{
    // existing values
    Dispatch,
    [Description("ELD / HOS")] Eld,
    [Description("Safety & Compliance")] Safety,
    // ← new
    ContainerTracking,
}
```

`GetDescription()` auto-humanizes - only add `[Description]` for acronyms or special formatting.

### 2. Migration: add default config

Use the `migration-creator` skill. The migration should INSERT a row into `default_feature_configs` with the new feature's platform default (typically `IsEnabled = true`). Pattern:

```csharp
migrationBuilder.Sql("""
    INSERT INTO default_feature_configs (id, feature, is_enabled)
    VALUES (gen_random_uuid(), 'ContainerTracking', true)
""");
```

Run against **master DB**.

### 3. Plan gating (if tier-restricted)

If only certain plans should grant the feature, add `PlanFeature` rows. This is a master-DB many-to-many between `SubscriptionPlan` and `TenantFeature`. The simplest path is updating the plan seeder:

```csharp
// In the SubscriptionPlan seeder
new PlanFeature { PlanId = enterprisePlanId, Feature = TenantFeature.ContainerTracking },
```

Or via SQL in a migration if seeding is not run idempotently.

If the feature is universally available, **skip this step** - the `DefaultFeatureConfig` row from step 2 will resolve true for every tenant.

### 4. Backend: gate the API

Put `[RequiresFeature]` on the command/query itself. `FeatureCheckBehaviour` enforces it in the
MediatR pipeline - no injection, no per-handler branch:

```csharp
[RequiresFeature(TenantFeature.ContainerTracking)]
public class CreateContainerCommand : ICommand<Result<Guid>>
{
    // ...
}
```

**Gate the queries too, not just the commands.** A half-gated module still serves the data to a
tenant whose plan excludes it, and the gap is invisible - every request type in the module should
carry the attribute.

### 4b. Backend: gate the jobs

Hangfire jobs **bypass the MediatR pipeline**, so `[RequiresFeature]` is inert there - the job must
ask `IFeatureService` itself:

```csharp
private async Task SyncTenantAsync(IServiceScope scope, Tenant tenant, CancellationToken ct)
{
    var featureService = scope.ServiceProvider.GetRequiredService<IFeatureService>();
    if (!await featureService.IsFeatureEnabledAsync(tenant.Id, TenantFeature.ContainerTracking))
    {
        return;
    }
    // ...
}
```

Signature is `IsFeatureEnabledAsync(Guid tenantId, TenantFeature feature)` - tenant id, no `ct`.

Keep the check inside the body, not in `TenantJobRunner.ForEachTenantAsync`: a job may need part of
its work to run unflagged (`IftaQuarterCloseJob` gates the snapshot but not the breadcrumb purge,
since breadcrumbs are written on every ELD ping regardless).

### 5. Frontend: gate the UI

In Angular, `feature.service.ts` (or equivalent) exposes the resolved features as signals. Pattern:

```typescript
const features = inject(FeatureService);

// In a component
protected readonly canSeeContainers = computed(() => features.isEnabled('ContainerTracking'));

// In template
@if (canSeeContainers()) {
  <a routerLink="/containers">Containers</a>
}
```

For route-level guards, use a `CanActivateFn` that calls `FeatureService` and redirects if false.

### 6. Admin portal toggles

The admin portal's tenant feature-config page reads the `TenantFeature` enum and shows a toggle for each value. New enum values are picked up automatically - verify by opening the page and confirming the new toggle is visible.

## Verification checklist

- [ ] Enum value added with description if needed
- [ ] Master migration adds `DefaultFeatureConfig` row
- [ ] (If tier-restricted) `PlanFeature` rows added for the right plans
- [ ] `[RequiresFeature]` on every command **and** query in the module (no half-gating)
- [ ] Every Hangfire job that touches the feature checks `IFeatureService.IsFeatureEnabledAsync`
- [ ] Frontend guards on `FeatureService` (template + route guard)
- [ ] Admin portal shows the new toggle
- [ ] Test: tenant on a plan without the feature gets blocked end-to-end
- [ ] Test: super admin can unlock by setting `IsAdminLocked = true; IsEnabled = true`
- [ ] Test: non-subscription tenant gets the feature based on default + tenant config (skips plan check)

## Common mistakes

- **Forgetting the default config row** - `FeatureService` falls through to a missing config and either throws or returns false unexpectedly.
- **Gating only in the UI** - the API still serves the data, so a sophisticated client can bypass. Always gate at the handler level.
- **Plan gate without a tenant override path** - Enterprise customers sometimes want to disable a feature; the `TenantFeatureConfig` row is the way out.
- **Half-gating a module** - this has actually happened: writes blocked, reads still serving. Gate every request type.
- **Forgetting the jobs** - a downgraded tenant keeps getting nightly syncs writing into their books.

## Related

- `feature-map.md` → Identity & access → Feature flags row
- `add-hangfire-job` - if the feature has a background job, the gate goes in the job body
