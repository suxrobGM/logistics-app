---
name: migration-creator
description: Create EF Core migrations for the master or tenant database. Use when adding or changing entities, properties or relationships, or when squashing a branch's unshipped migrations.
---

# Create a migration

| Context | Holds | Output folder |
| --- | --- | --- |
| `TenantDbContext` | Per-company data (loads, trips, employees) | `Migrations/Tenant` |
| `MasterDbContext` | Tenants, subscriptions, users | `Migrations/Master` |
| `PersistedGrantDbContext` | IdentityServer grants; changes only on package upgrades | `Migrations/OperationalStore` |

All three live in `src/Infrastructure/Logistics.Infrastructure.Persistence`.

```bash
dotnet ef migrations add AddDriverLicenseExpiry --project src/Infrastructure/Logistics.Infrastructure.Persistence --context TenantDbContext -o Migrations/Tenant
```

(`scripts/add-migration.cmd` wraps this.) Name it for what it does in PascalCase. EF adds the timestamp, so no version numbers or dates. Read the generated file before committing.

## Apply locally

```bash
DOTNET_ENVIRONMENT=Development dotnet run --project src/Presentation/Logistics.DbMigrator --no-launch-profile -- --exit
```

Always pin the environment. `--exit` stops after migrate and seed. Never `dotnet ef database update` the operational store: its design-time factory ignores the connection string.

## Unshipped migrations

If the latest migration exists only on your branch, regenerate it instead of stacking a fix:

```bash
dotnet ef migrations remove --project src/Infrastructure/Logistics.Infrastructure.Persistence --context TenantDbContext --force
```

Then add it again under the same name. Never do this to a migration on `main`. The connection error `remove` prints without Postgres is harmless with `--force`.

Every migration writes a ~7,000-line `*.Designer.cs` snapshot, and GitHub's PR line count ignores `.gitattributes`. To shrink a PR, squash a branch's unshipped migrations to one per context.

## Traps

- **Complex types can't be in a unique index.** Uniqueness over a `ComplexProperty` member needs an injected checker on every write path (`IIftaTaxRateDuplicateChecker`) and a normalizing factory on the value object (`TaxJurisdiction.Create`).
- **Seeders are registered by hand** in `AddSeeders()` (`DbMigrator/Extensions/ServiceCollectionExtensions.cs`). An unregistered seeder is silently skipped.
