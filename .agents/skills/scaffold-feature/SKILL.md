---
name: scaffold-feature
description: Scaffold a full vertical slice for a new domain feature in LogisticsX — entity, EF configuration, migration, command/query handlers, controller, DTO/mapper, Angular page, and feature-map row. Use when the user asks to add a new top-level feature (e.g., "add a Subcontractor entity with CRUD"). Outputs an ordered checklist tailored to the LogisticsX folder structure.
---

# Scaffold a Feature (Vertical Slice)

Adds a new top-level feature end-to-end. Run through the steps in order — earlier steps gate later ones (e.g., the migration depends on the entity + EF config).

## Decide first

Before writing any code:

1. **Master DB or tenant DB?** Master = platform-level data shared across tenants (Tenant, Subscription, BlogPost). Tenant = per-company operational data (Load, Trip, Customer). Most new features are tenant-scoped.
2. **Aggregate root or child entity?** A new aggregate gets its own folder under `Entities/{Feature}/`. A child entity belongs to an existing aggregate's folder.
3. **Is it auditable?** Almost always yes — inherit `AuditableEntity`. Use `Entity` only for very simple lookup tables.
4. **Which marker interface?** `IMasterEntity` (master DB) or `ITenantEntity` (tenant DB). The DbContext picks up the entity automatically based on this — **no manual `DbSet<T>` needed**.

## Step-by-step

### 1. Domain entity

`src/Core/Logistics.Domain/Entities/{Feature}/{Entity}.cs`

```csharp
using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

public class Subcontractor : AuditableEntity, ITenantEntity
{
    public required string Name { get; set; }
    public string? ContactEmail { get; set; }
    public SubcontractorStatus Status { get; set; } = SubcontractorStatus.Active;
    // value objects: Address, Money, etc.
}
```

If status enum needed: `src/Core/Logistics.Domain.Primitives/Enums/{Feature}/{Status}.cs`. Use `GetDescription()` for display — `[Description]` only when humanization isn't enough (acronyms, special formatting).

### 2. EF configuration

`src/Infrastructure/Logistics.Infrastructure.Persistence/Configurations/{Feature}/{Entity}EntityConfiguration.cs`

```csharp
internal sealed class SubcontractorEntityConfiguration : IEntityTypeConfiguration<Subcontractor>
{
    public void Configure(EntityTypeBuilder<Subcontractor> builder)
    {
        builder.ToTable("subcontractors");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Status).HasConversion<string>();
        // builder.ComplexProperty(s => s.Address); for value objects
    }
}
```

The `MasterDbContext` / `TenantDbContext` discover configurations via `ApplyConfigurationsFromAssembly` — you don't need to register them.

### 3. Migration

Use the `migration-creator` skill or run:

```bash
# Tenant
dotnet ef migrations add Version_{N} \
  --project src/Infrastructure/Logistics.Infrastructure.Persistence \
  --context TenantDbContext \
  -o Migrations/Tenant
```

Replace `{N}` with the next sequential number. Inspect the generated SQL before committing.

### 4. Specifications (if list queries need filtering)

`src/Core/Logistics.Domain/Specifications/{Feature}/{Entity}Specs.cs`

Reuse pattern: `Specification<T>` with `Query.Where(...)` + `OrderBy(...)`.

### 5. Commands and queries

`src/Core/Logistics.Application/Commands/{Feature}/`:

- `Create{Entity}/Create{Entity}Command.cs` — `record Create{Entity}Command(Create{Entity}Dto Dto) : IRequest<DataResult<{Entity}Dto>>`
- `Create{Entity}/Create{Entity}Handler.cs` — `internal sealed class`, primary-constructor DI
- `Create{Entity}/Create{Entity}Validator.cs` — FluentValidation
- Same pattern for `Update{Entity}/`, `Delete{Entity}/`

`src/Core/Logistics.Application/Queries/{Feature}/`:

- `Get{Entity}ById/Get{Entity}ByIdQuery.cs` + handler
- `Get{Entities}/Get{Entities}Query.cs` + handler — paged list

### 6. DTOs and Mapperly mapper

`src/Core/Logistics.Shared.Models/{Entity}Dto.cs` for the public DTO.

`src/Core/Logistics.Mappings/{Entity}Mapper.cs`:

```csharp
[Mapper]
public static partial class SubcontractorMapper
{
    [MapperIgnoreSource(nameof(Subcontractor.Loads))]  // ignore navigation properties
    public static partial SubcontractorDto ToDto(this Subcontractor entity);
}
```

Never write manual mapping in handlers.

### 7. Controller

`src/Presentation/Logistics.API/Controllers/{Entity}Controller.cs`

```csharp
[Route("subcontractors")]
[Produces("application/json")]
[Authorize]
public class SubcontractorController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permission.Subcontractors.View)]
    [ProducesResponseType<PagedResponse<SubcontractorDto>>(200)]
    public async Task<IActionResult> GetList([FromQuery] GetSubcontractorsQuery query, CancellationToken ct)
        => Ok(await mediator.Send(query, ct));

    // GET {id}, POST, PUT {id}, DELETE {id}
}
```

REST conventions:

- Plural lowercase nouns (`/subcontractors`, not `/subcontractor`)
- Path params for IDs
- Custom actions as sub-resources (`POST /subcontractors/{id}/archive`)
- Sort syntax: `OrderBy=-CreatedAt` (NOT `"CreatedAt desc"`)

### 8. Permissions

Add `Subcontractors.View` and `Subcontractors.Manage` to the `Permission` constants (location: shared identity project). Wire to `TenantRole` claims via the seeder or admin UI.

### 9. Tests

`tests/Logistics.Application.Tests/Commands/{Feature}/Create{Entity}HandlerTests.cs` — xUnit + NSubstitute. Field name `sut`.

```csharp
public class CreateSubcontractorHandlerTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly CreateSubcontractorHandler sut;

    public CreateSubcontractorHandlerTests() => sut = new CreateSubcontractorHandler(tenantUow);

    [Fact]
    public async Task Handle_ValidInput_CreatesEntity() { /* ... */ }
}
```

### 10. Frontend

#### Angular API regen first

```bash
cd src/Client/Logistics.Angular
bun run gen:api:live   # regenerates from running API
```

This produces typed clients in `projects/shared/src/lib/api/generated/`. Update the barrel (`models.ts`) if a new DTO needs to be exported.

#### Page

`src/Client/Logistics.Angular/projects/tms-portal/src/app/pages/subcontractors/`:

- `subcontractors.ts` (list page) + `.html` template
- `subcontractor-edit.ts` (form) + `.html`
- `subcontractor-store.ts` if state is non-trivial (`@ngrx/signals`)

Conventions: standalone components, signals (`signal()`, `computed()`), `input()`/`output()` (not decorators), native control flow (`@if`, `@for`), `<ui-form-field>` for inputs.

#### Routing

Add a route entry in the portal's `app.routes.ts`. Add a sidebar menu entry if user-facing.

#### HTTP cache

Default cache TTL is 2 min. If the feature gets real-time updates via SignalR, add a `ttl: 0` rule in `projects/shared/src/lib/api/cache.config.ts` **before** the catch-all.

### 11. Update feature-map.md

Add a row under the appropriate domain section in `.claude/feature-map.md`:

```markdown
| Subcontractors | `Entities/Subcontractor.cs` | `Commands/Subcontractor/`, `Queries/Subcontractor/` | - | `SubcontractorController.cs`, `tms-portal/pages/subcontractors/` |
```

## Verification checklist

- [ ] Entity inherits `AuditableEntity`/`Entity` and implements `IMasterEntity`/`ITenantEntity`
- [ ] EF configuration created (auto-discovered by DbContext)
- [ ] Migration generated and inspected
- [ ] Commands + queries + validators + handlers created (handlers `internal sealed`)
- [ ] DTO + Mapperly mapper, no manual mapping
- [ ] Controller with `[Authorize(Policy = ...)]` on each action
- [ ] Permissions added
- [ ] Unit tests for handlers
- [ ] Angular API client regenerated
- [ ] Angular page + route added
- [ ] Cache rule added if real-time
- [ ] **Row added to `.claude/feature-map.md`**
- [ ] `dotnet build` passes
- [ ] `bun run lint` passes
- [ ] Feature reachable end-to-end in the running app

## Common mistakes

- Forgetting the `IMasterEntity` / `ITenantEntity` marker — entity is invisible to both DbContexts.
- Manual mapping inside handlers — Mapperly should own all mapping.
- Missing `[Authorize(Policy = ...)]` — controller falls back to default auth, leaks data across tenants.
- Updating Angular `models.ts` barrel manually after regen but forgetting to commit it — breaks the build.
- Forgetting the feature-map.md row — feature becomes invisible to future sessions.

## Related skills

- `migration-creator` — generates the EF migration in step 3
- `add-dispatch-tool` — if the new feature should be agent-callable
- `simplify` — run after scaffolding to prune any boilerplate

## Related rules

- `.claude/rules/backend/csharp-conventions.md`
- `.claude/rules/backend/api-design.md`
- `.claude/rules/backend/mapperly.md`
- `.claude/rules/backend/security.md`
- `.claude/rules/backend/testing.md`
- `.claude/rules/frontend/angular-conventions.md`
