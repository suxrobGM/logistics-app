---
name: migration-creator
description: Creates EF Core migrations for master or tenant databases. Use when adding new entities, modifying existing ones, or changing relationships. Follow the workflow to ensure correct migration generation and application.
---

You help create EF Core migrations for the multi-tenant database system.

## Database Types

- **Master DB**: Contains Tenants, Subscriptions, SuperAdmin users
  - DbContext: `MasterDbContext`
  - Project: `Logistics.Infrastructure.Persistence`

- **Tenant DB**: Contains all tenant-specific data (Loads, Trips, Employees, etc.)
  - DbContext: `TenantDbContext`
  - Project: `Logistics.Infrastructure.Persistence`

## Creating Migrations

Use the helper script:

```bash
scripts/add-migration.cmd
```

Or manually:

**For Master DB:**

```bash
dotnet ef migrations add {MigrationName} --project src/Infrastructure/Logistics.Infrastructure.Persistence --context MasterDbContext -o Migrations/Master
```

**For Tenant DB:**

```bash
dotnet ef migrations add {MigrationName} --project src/Infrastructure/Logistics.Infrastructure.Persistence --context TenantDbContext -o Migrations/Tenant
```

## Workflow

1. Identify which database the entity change affects (Master or Tenant)
2. Review the entity changes to understand the migration scope
3. Generate the migration with a **concise descriptive name** (PascalCase) that summarizes what changed
4. Review the generated migration file for correctness

## Migration Naming

Use a short, descriptive PascalCase name that says what the migration does — EF Core auto-prefixes the timestamp, so do **not** add `Version_NNNN`, numeric suffixes, or a date.

- Good: `InitialSchema`, `AddDriverLicenseExpiry`, `RenameLoadStatus`, `AddTenantVatNumber`, `DropLegacyDispatchTable`
- Bad: `Version_0042`, `Migration_5`, `Update3`, `Changes_2026_05_19`

The first migration for a database is conventionally named `InitialSchema` (or `InitialCreate`).

## Amending a migration that hasn't shipped

If the migration is only on your branch (`git branch -a --contains <sha>` — not on `main` means no
real DB has applied it) and is the **latest** for that context, regenerate it rather than stacking
a corrective one:

```bash
dotnet ef migrations remove --project src/Infrastructure/Logistics.Infrastructure.Persistence --context TenantDbContext --force
dotnet ef migrations add {SameName} --project src/Infrastructure/Logistics.Infrastructure.Persistence --context TenantDbContext -o Migrations/Tenant
```

The timestamp changes, so anyone who applied the old one locally must drop and re-migrate. The
connection error `remove` prints when Postgres is unreachable is just the applied-check; `--force`
removes the files anyway. Never do this to a migration already on `main`.

## Traps

- **Complex types cannot be part of a unique index.** If uniqueness spans a `ComplexProperty`
  member, EF can't express it and there is no DB guard — every write path must probe in code
  (`IftaTaxRateUniqueness.FindConflictAsync`), and the value object needs a normalizing factory
  (`TaxJurisdiction.Create`) so casing or empty-vs-null can't defeat the comparison.
- Seeders in `src/Presentation/Logistics.DbMigrator/Seeders/` are **explicitly registered** —
  adding the file is not enough.
