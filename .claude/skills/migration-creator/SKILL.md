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
