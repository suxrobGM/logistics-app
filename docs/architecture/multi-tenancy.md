# Multi-Tenancy Architecture

Logistics TMS uses database-per-tenant isolation for complete data separation.

## Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        Master Database                          │
│  ┌─────────┐  ┌─────────────┐  ┌───────────────┐               │
│  │ Tenants │  │ Subscribers │  │ Super Admins  │               │
│  └─────────┘  └─────────────┘  └───────────────┘               │
└─────────────────────────────────────────────────────────────────┘

┌─────────────┐  ┌─────────────┐  ┌─────────────┐
│  Tenant A   │  │  Tenant B   │  │  Tenant C   │
│  Database   │  │  Database   │  │  Database   │
├─────────────┤  ├─────────────┤  ├─────────────┤
│ Users       │  │ Users       │  │ Users       │
│ Loads       │  │ Loads       │  │ Loads       │
│ Trips       │  │ Trips       │  │ Trips       │
│ Customers   │  │ Customers   │  │ Customers   │
│ Invoices    │  │ Invoices    │  │ Invoices    │
│ ...         │  │ ...         │  │ ...         │
└─────────────┘  └─────────────┘  └─────────────┘
```

## Database Strategy

### Master Database

Contains global data:

- **Tenants**: Company information, settings
- **Subscriptions**: Billing, plan details
- **SuperAdmins**: System administrator accounts

### Tenant Databases

Each company gets an isolated database containing:

- Users and roles
- Customers
- Loads and trips
- Invoices and payments
- Employees and drivers
- Trucks and equipment
- Documents

## Configuration

### Connection Strings

In `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "MasterDatabase": "Host=localhost;Database=master_logistics;...",
    "DefaultTenantDatabase": "Host=localhost;Database=default_logistics;..."
  },
  "TenantsDatabaseConfig": {
    "DatabaseNameTemplate": "{tenant}_logistics",
    "DatabaseHost": "localhost",
    "DatabaseUserId": "postgres",
    "DatabasePassword": "password"
  }
}
```

### Database Naming

Tenant databases follow the pattern: `{tenant_id}_logistics`

Examples:
- `default_logistics`
- `acme_trucking_logistics`
- `swift_transport_logistics`

## Tenant Resolution

### Request Flow

```
1. Request arrives with JWT token
2. Token contains tenant claim
3. TenantContext extracts tenant ID
4. DbContext uses tenant-specific connection string
5. Query executes against correct database
```

### Implementation

```csharp
public class TenantDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var connectionString = GetTenantConnectionString(_tenantContext.TenantId);
        options.UseNpgsql(connectionString);
    }
}
```

## Creating New Tenants

When a new company signs up:

1. Create tenant record in master database
2. Create new database using template
3. Run migrations on new database
4. Seed initial data (roles, settings)
5. Create owner account

```csharp
public async Task CreateTenantAsync(CreateTenantDto dto)
{
    // 1. Create tenant record
    var tenant = new Tenant { Id = dto.Id, Name = dto.Name };
    await _masterContext.Tenants.AddAsync(tenant);

    // 2. Create database
    await _databaseManager.CreateDatabaseAsync(tenant.Id);

    // 3. Run migrations
    await _migrationRunner.MigrateAsync(tenant.Id);

    // 4. Seed data
    await _seeder.SeedTenantAsync(tenant.Id);
}
```

## Benefits

### Data Isolation

- Complete separation between tenants
- No risk of data leakage
- Simplified compliance (GDPR, etc.)

### Performance

- Queries only scan tenant data
- Independent scaling per tenant
- Separate backup/restore

### Customization

- Per-tenant schema modifications (future)
- Different retention policies
- Geographic data residency

## Considerations

### Connection Pooling

With many tenants, connection pooling is important:

```json
{
  "ConnectionStrings": {
    "MasterDatabase": "...;Pooling=true;Maximum Pool Size=20;..."
  }
}
```

### Database Management

- Automated backup per tenant
- Migration coordination
- Monitoring across databases

## Next Steps

- [Domain Model](domain-model.md) - Entity structure
- [API Authentication](../api/authentication.md) - JWT and tenant claims
