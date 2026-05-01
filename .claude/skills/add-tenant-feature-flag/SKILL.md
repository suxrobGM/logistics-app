---
name: add-tenant-feature-flag
description: Add a new plan-gated tenant feature flag (e.g. "ContainerTracking", "AdvancedAnalytics") that can be toggled per-tenant and gated by subscription plan tier. Use when adding a feature that should be: locked for some plans, opt-in per tenant, or admin-overridable. Walks through the four-tier resolution chain.
---

# Add a Tenant Feature Flag

LogisticsX uses a layered feature-flag system that resolves in this priority order:

1. **Admin-locked override** (super admin sets `IsAdminLocked = true`)
2. **Plan gating** (the tenant's subscription plan grants the feature via `PlanFeature`)
3. **Tenant config** (the tenant has explicitly enabled/disabled the feature)
4. **Default config** (`DefaultFeatureConfig` for the platform)

`FeatureService` walks this chain and returns the effective value.

## When to use this skill

Use this skill if the new feature should:

- Be enabled only for certain plan tiers (Starter / Professional / Enterprise)
- Be toggleable per tenant (so an Enterprise customer can opt out)
- Be admin-overridable (super admin can lock or unlock for one tenant)

Don't use this skill for:

- Roles/permissions — those go through `Permission` constants and policy authorization
- Code-level kill switches — use a config flag instead
- A/B experiments — use a different mechanism

## Files that must change

1. `src/Core/Logistics.Domain.Primitives/Enums/Tenant/TenantFeature.cs` — enum value
2. Master DB migration — adds row to `DefaultFeatureConfig` table for the new feature
3. (Optional) Update `SubscriptionPlan` seeders / `PlanFeature` rows to grant the feature to specific tiers
4. Frontend: feature gate in route guards, components, or services
5. Admin portal: feature toggles UI (usually picks up the new enum value automatically)
6. TMS portal AI Settings or other surfaces: respect the gate

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

`GetDescription()` auto-humanizes — only add `[Description]` for acronyms or special formatting.

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

If the feature is universally available, **skip this step** — the `DefaultFeatureConfig` row from step 2 will resolve true for every tenant.

### 4. Backend: gate the API

Inject `IFeatureService` in the handler/controller and check before executing:

```csharp
public class CreateContainerHandler(
    IFeatureService featureService,
    ITenantUnitOfWork tenantUow) : IRequestHandler<CreateContainerCommand, DataResult<ContainerDto>>
{
    public async Task<DataResult<ContainerDto>> Handle(CreateContainerCommand cmd, CancellationToken ct)
    {
        if (!await featureService.IsEnabledAsync(TenantFeature.ContainerTracking, ct))
            return DataResult<ContainerDto>.CreateError("ContainerTracking is not enabled for this tenant");
        // ...
    }
}
```

For controllers, prefer guarding at the handler level — controllers stay focused on auth + validation.

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

The admin portal's tenant feature-config page reads the `TenantFeature` enum and shows a toggle for each value. New enum values are picked up automatically — verify by opening the page and confirming the new toggle is visible.

### 7. Update default disabled flag (optional)

Some features should default to **off** at the platform level. Set `IsEnabled = false` in step 2's INSERT. Tenants then opt in either via plan gating or per-tenant override.

## Resolution chain reference

`FeatureService.IsEnabledAsync(feature)` walks:

```text
1. Is there a TenantFeatureConfig with IsAdminLocked=true?
   → return its IsEnabled value

2. Does the tenant's plan grant this feature via PlanFeature?
   AND no negative TenantFeatureConfig override exists?
   → return true

3. Is there a TenantFeatureConfig (not admin-locked) for this tenant + feature?
   → return its IsEnabled value

4. Fall back to DefaultFeatureConfig.IsEnabled
```

Non-subscription tenants (`Tenant.IsSubscriptionRequired = false`) **bypass plan gating entirely** — they get whatever the tenant config or default says, without checking `PlanFeature`.

## Verification checklist

- [ ] Enum value added with description if needed
- [ ] Master migration adds `DefaultFeatureConfig` row
- [ ] (If tier-restricted) `PlanFeature` rows added for the right plans
- [ ] Backend handler/controller guards on `IFeatureService.IsEnabledAsync`
- [ ] Frontend guards on `FeatureService` (template + route guard)
- [ ] Admin portal shows the new toggle
- [ ] Test: tenant on a plan without the feature gets blocked end-to-end
- [ ] Test: super admin can unlock by setting `IsAdminLocked = true; IsEnabled = true`
- [ ] Test: non-subscription tenant gets the feature based on default + tenant config (skips plan check)

## Common mistakes

- **Forgetting the default config row** — `FeatureService` falls through to a missing config and either throws or returns false unexpectedly.
- **Gating only in the UI** — the API still serves the data, so a sophisticated client can bypass. Always gate at the handler level.
- **Plan gate without a tenant override path** — Enterprise customers sometimes want to disable a feature; the `TenantFeatureConfig` row is the way out.
- **Ignoring `IsSubscriptionRequired = false` tenants** — internal/demo tenants don't go through plan gating, so a feature gated only by `PlanFeature` won't work for them.

## Related

- Auto-memory note: _FeatureService resolution: admin-locked > plan gating > tenant config > defaults; non-subscription tenants bypass all gating_
- `feature-map.md` → Identity & access → Feature flags row
