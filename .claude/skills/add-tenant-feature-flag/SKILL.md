---
name: add-tenant-feature-flag
description: Add a plan-gated tenant feature flag (e.g. "ContainerTracking") that platform admins control per tenant through presets and overrides. Use when a feature should be locked for some plans, tied to a company-type preset, or admin-overridable. Not for permissions, kill switches or experiments.
---

# Add a tenant feature flag

Only platform admins change features. `FeatureService.IsFeatureEnabledAsync` returns the first rule that decides:

1. **Admin lock**: a `TenantFeatureConfig` with `IsAdminLocked` returns its `IsEnabled`, overriding the plan.
2. **Plan**: a feature the plan doesn't grant (`PlanFeature`) is off.
3. **Tenant config row**: returns its `IsEnabled`.
4. **Presets**: with no row, `TenantPresetCatalog.Resolve(tenant.Presets)`.

Rows are a snapshot written from presets at tenant creation and on preset changes, so existing tenants resolve a new feature through rule 4.

## Steps

1. **Enum:** `TenantFeature` (`src/Core/Logistics.Domain.Primitives/Enums/Tenancy/Tenant/`). `[Description]` only for acronyms. The admin features page lists every value automatically.
2. **Plans** (if tier-restricted): add it to the tier arrays in `SubscriptionPlanSeeder`. Enterprise gets everything.
3. **Presets:** features default to on. A cargo-specific feature goes in `CargoFeatures` (like `VehicleTransport` → `CarHauler`); one a solo operator never uses goes in `SoloExcludedFeatures`.
4. **API:** `[RequiresFeature(TenantFeature.X)]` on **every command and query** in the module. Gating only writes still serves the data. When the feature is a field value on a shared request (a load type), check it in the shared write path with `IFeatureService.CheckFeatureAsync` (see `IVehicleTransportFeatureChecker`) so the error code still triggers the upgrade prompt.
5. **Jobs:** Hangfire bypasses the pipeline. Every job touching the feature checks `IsFeatureEnabledAsync` in its body (see `add-hangfire-job`).
6. **UI:** `features.isEnabled("x")` from `FeatureService`; routes use `featureGuardFromData` (`@logistics/shared/guards`) with `data.feature`.

## Verify

A tenant on a plan without the feature is blocked in the API and the UI; an admin lock with `IsEnabled = true` grants it outside the plan.
