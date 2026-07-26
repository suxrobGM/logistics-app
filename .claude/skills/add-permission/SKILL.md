---
name: add-permission
description: Add a new permission (a Permission.X.View / .Manage pair) and wire it to roles, controllers, and the Angular guard. Use when a new feature needs its own access control, or an existing one needs a Review/Sync verb. Covers the reflection contract in Permissions.cs and the hand-mirrored TypeScript copy - both fail silently when broken.
---

# Add a permission

Permissions are `"Permission.{Module}.{Verb}"` strings. There is **no policy registration**:
`PermissionPolicyProvider` (`src/Presentation/Logistics.API/Authorization/`) builds a policy on demand
for any name starting with `Permission`, backed by `PermissionRequirement` + `PermissionHandler`. So
`[Authorize(Policy = Permission.Thing.View)]` just works.

The corollary is the main hazard: **a typo'd policy string is not a startup error.** It yields a
policy nobody can satisfy - a 403 at runtime with no clue why.

## 1. Declare the constants

`src/Shared/Logistics.Shared.Identity/Policies/Permissions.cs`:

```csharp
public static class Thing
{
    public const string View = $"{nameof(Permission)}.{nameof(Thing)}.View";
    public const string Manage = $"{nameof(Permission)}.{nameof(Thing)}.Manage";
}
```

`View` / `Manage` is the default pair. Add `Review` or `Sync` only when a real third role boundary
exists (see the existing modules that have them).

**The static constructor reflects over these, and the filter is exact:**

- nested types must be `IsClass && IsSealed && IsAbstract` - i.e. a `public static class`
- fields must be `IsLiteral && !IsInitOnly` - i.e. `const`, **not** `static readonly`

A non-static nested class, or a `static readonly string`, compiles fine, reads fine, and is **silently
dropped from `GetAll()`**. SuperAdmin then never receives it and the permission is unusable by anyone.

## 2. Grant it to roles

`src/Shared/Logistics.Shared.Identity/Policies/TenantRolePermissions.cs` - add to each of
`GetOwnerPermissions` / `GetManagerPermissions` / `GetDispatcherPermissions` /
`GetDriverPermissions` / `GetCustomerPermissions` that should have it:

```csharp
list.AddRange(Permission.GeneratePermissions(nameof(Permission.Thing)));
```

`GeneratePermissions` does a bare `Modules[module]` lookup, so a module name that isn't a registered
static class throws `KeyNotFoundException` **at role-seed time**, not at compile time. Using
`nameof(Permission.Thing)` rather than a string literal is what keeps this compiler-checked.

`AppRolePermissions.cs` covers the app-level roles: `SuperAdmin` gets `Permission.GetAll` automatically;
`Admin` needs an explicit line in `GetAdminPermissions()`.

## 3. Re-seed

Role claims are written by `TenantRoleSeeder` / `AppRoleSeeder`
(`src/Presentation/Logistics.DbMigrator/Seeders/Infrastructure/`), and by
`TenantDatabaseService` when a tenant is provisioned.

**New permissions do not retroactively appear on already-provisioned tenants.** Existing tenants need
a re-seed run, or every user keeps 403ing on a permission the code says they have.

## 4. Mirror it in TypeScript

`projects/shared/src/lib/models/permissions.ts` is a **hand-maintained copy** of the C# tree, and
`PermissionValue` derives from it. Nothing checks that the two agree - this is the most likely drift
point in the repo. Add the same module and verbs:

```ts
Thing: { View: "Permission.Thing.View", Manage: "Permission.Thing.Manage" },
```

The upside of the derived type: a route referencing a missing key fails to compile.

## 5. Use it

**Backend** - on the controller action:

```csharp
[Authorize(Policy = Permission.Thing.View)]
```

**Angular route** - `data: { permission: Permission.Thing.View }` plus `canActivate: [authGuard]`.

**Angular template** - the `<ui-permission-guard [permissions]="..." mode="any|all">` **component**
(`projects/shared/src/lib/permission/permission-guard.ts`; it is not a structural directive), backed
by `PERMISSION_CHECKER` / `PermissionService`.

Route guards and template guards shape the UI. **They are not a security boundary** - the API
re-checks. A page with a guard but an unauthorized endpoint is a vulnerability.

## Checklist

- [ ] Nested type is a `public static class`; every field is `const` (not `static readonly`)
- [ ] Constants built with the `nameof` interpolation, matching the surrounding style
- [ ] Added to the right role lists in `TenantRolePermissions.cs` via `nameof(Permission.X)`
- [ ] `AppRolePermissions.GetAdminPermissions()` updated if Admin needs it
- [ ] Mirrored in `projects/shared/src/lib/models/permissions.ts`
- [ ] Controller actions carry `[Authorize(Policy = ...)]`
- [ ] Angular routes carry `data.permission` + `authGuard`
- [ ] Re-seeded, and verified against an **existing** tenant, not just a fresh one
- [ ] Tested as a role that should **not** have it

## Related

- `scaffold-feature` step 8 delegates here
- `add-angular-page` - the route side of the wiring
- `.claude/rules/backend/api-design.md` - controller auth conventions
