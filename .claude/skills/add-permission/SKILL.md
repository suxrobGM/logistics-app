---
name: add-permission
description: Add a new permission (a Permission.X.View / .Manage pair) and wire it to roles, controllers and the Angular guards. Use when a feature needs its own access control or an existing module needs a Review/Sync verb. Covers the reflection contract in Permissions.cs and the hand-mirrored TypeScript copy, both of which fail silently.
---

# Add a permission

Permissions are `"Permission.{Module}.{Verb}"` strings. `PermissionPolicyProvider` builds policies on demand, so nothing is registered, and **a typo'd policy is a runtime 403, not a startup error**.

## 1. Constants

`src/Shared/Logistics.Shared.Identity/Policies/Permissions.cs`:

```csharp
public static class Thing
{
    public const string View = $"{nameof(Permission)}.{nameof(Thing)}.View";
    public const string Manage = $"{nameof(Permission)}.{nameof(Thing)}.Manage";
}
```

The static constructor reflects only over **`public static class`** members holding **`const`** fields. A non-static class or a `static readonly` field compiles and is silently dropped from `GetAll()`, so not even SuperAdmin gets it. Add `Review` / `Sync` only for a real third boundary.

## 2. Roles

- `TenantRolePermissions.cs`: add `list.AddRange(Permission.GeneratePermissions(nameof(Permission.Thing)));` to each role that needs it. Use `nameof`: a wrong string throws `KeyNotFoundException` at seed time.
- `AppRolePermissions.GetAdminPermissions()` if Admin needs it. SuperAdmin gets everything.
- Re-seed (`TenantRoleSeeder` / `AppRoleSeeder` in the DbMigrator). **Existing tenants don't get new permissions until re-seeded.**

## 3. TypeScript mirror

`projects/shared/src/lib/models/permissions.ts` is a hand-kept copy that nothing checks:

```ts
Thing: { View: "Permission.Thing.View", Manage: "Permission.Thing.Manage" },
```

## 4. Use it

- API: `[Authorize(Policy = Permission.Thing.View)]` on every action.
- Route: `canActivate: [authGuard]`, `data: { permission: Permission.Thing.View }`.
- Template: `<ui-permission-guard [permissions]="..." mode="any|all">` (a component, not a directive).

Guards only shape the UI; the API is the boundary. Verify against an **existing** tenant, as a role that should **not** have the permission.
