---
name: scaffold-feature
description: Scaffold a full vertical slice for a new domain feature - entity, EF configuration, migration, commands/queries, controller, DTO and mapper, permissions, Angular page and feature-map row. Use when the user asks for a new top-level feature (e.g. "add a Subcontractor entity with CRUD").
---

# Scaffold a feature

Work in order; later steps depend on earlier ones. Conventions come from `.claude/rules/`; this is the wiring.

## Decide

1. **Tenant or master DB?** Almost always tenant (`ITenantEntity`). Master (`IMasterEntity`) is platform data like tenants and subscriptions. The marker is all the DbContext needs; no `DbSet`.
2. **Module:** `Operations`, `Compliance`, `Financial`, `Identity`, `Tenancy`, `AI`, `Integrations`, `Reporting` or `Website` (see [module-layout.md](../../../docs/architecture/module-layout.md)).
3. **New aggregate or child** of an existing one. Inherit `AuditableEntity` unless it's a trivial lookup.

## Backend

1. **Entity:** `src/Core/Logistics.Domain/Entities/{Module}/{Aggregate}/{Entity}.cs`, namespace `Logistics.Domain.Entities`. Enums in `Logistics.Domain.Primitives/Enums/{Module}/{Aggregate}/`.
2. **EF configuration:** `Logistics.Infrastructure.Persistence/Configurations/{Module}/{Aggregate}/{Entity}EntityConfiguration.cs`, `internal sealed`, auto-discovered. Enums need no conversion (stored as snake_case). Uniqueness over a complex-type member needs an injected checker (see `IIftaTaxRateDuplicateChecker`).
3. **Migration:** `migration-creator`, e.g. `AddSubcontractor`.
4. **Commands and queries** in `Logistics.Application/Modules/{Module}/{Feature}/Commands|Queries/{Name}/`: request, `internal sealed` handler, validator if it has real rules. Requests are flat and bound directly as the body. Delete and GetById subclass `DeleteTenantEntityHandler` / `GetTenantEntityByIdHandler` (`Common/Handlers/`). Lists use an inline `.Query()` ending in `ToPagedResultAsync(req, x => x.ToDto(), ct)`; extract an `IQueryable` extension only when a filter is reused.
5. **DTO and mapper:** `src/Shared/Logistics.Shared.Models/{Feature}/{Entity}Dto.cs`; Mapperly mapper beside the feature, ignoring navigations.
6. **Controller:** `Logistics.API/Controllers/{Module}/{Entity}Controller.cs`, plural route, `[Authorize(Policy = ...)]` on every action.
7. **Permissions:** `add-permission` (`Permission.Subcontractor.View/Manage`).
8. **Tests:** `test/Logistics.Application.Tests/{Module}/{Feature}/` for handlers with real logic.

## Frontend

1. `bun run gen:api:live` from `private/src/Client/Logistics.Angular` (API running).
2. Page via `add-angular-page`, shaped like `pages/trailers/`: `{feature}.routes.ts`, `{x}-list`, `{x}-add`, `{x}-edit`, `{x}-details`, optional `store/`. Lazy-register in `app.routes.ts`, add the sidebar item.
3. SignalR-backed data needs a `ttl: 0` rule in `cache.config.ts` before the catch-all.

## Finish

Add the feature to `.claude/feature-map.md` (unlisted features are invisible to later sessions). `dotnet build`, `bun run lint`, then exercise it end to end in the running app.
