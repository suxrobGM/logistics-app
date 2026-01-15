# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Logistics TMS is a multi-tenant fleet management platform for trucking companies specializing in intermodal containers and vehicle transport. The system automates dispatching, GPS tracking, invoicing, and payroll.

## Build & Run Commands

### Full Stack (Recommended)

```bash
# Run all services via .NET Aspire (requires Docker)
dotnet run --project src/Aspire/Logistics.Aspire.AppHost
# Dashboard: http://localhost:7100
```

### Backend (.NET 10)

```bash
dotnet build                                           # Build all projects
dotnet test                                            # Run all tests
dotnet run --project src/Presentation/Logistics.API   # API on https://localhost:7000
dotnet run --project src/Presentation/Logistics.IdentityServer  # Auth on https://localhost:7001
dotnet run --project src/Presentation/Logistics.DbMigrator      # Run migrations
```

### Frontend (Angular 21)

```bash
cd src/Client/Logistics.OfficeApp
bun install           # Install dependencies
bun run start         # Dev server on https://localhost:7003
bun run build         # Production build
bun run lint          # ESLint
bun run format        # Prettier
bun run gen:api       # Regenerate API client from OpenAPI spec
```

### Mobile (Kotlin Multiplatform)

```bash
cd src/Client/Logistics.DriverApp
./gradlew assembleDebug    # Android APK
./gradlew iosSimulatorX64  # iOS simulator
```

### Helper Scripts

```bash
scripts/seed-databases.cmd      # Initialize databases
scripts/run-aspire.cmd          # Launch full stack
```

## Architecture

### Layer Structure (DDD + CQRS)

```text
Presentation (HTTP)     → Logistics.API, Logistics.IdentityServer, Logistics.DbMigrator
Application (Business)  → Commands/Queries (MediatR), Services, SignalR Hubs
Domain (Entities)       → Entities, Domain Events, Specifications, Value Objects
Infrastructure (Data)   → EF Core DbContext, Repositories, Unit of Work, and External Integrations
Shared (Models)         → DTOs shared between backend and frontend
```

### Multi-Tenant Database Strategy

- **Master DB**: Tenants, subscriptions, super admin users
- **Tenant DBs**: Sharded per company (isolated data per tenant)
- Connection strings: `ConnectionStrings:MasterDatabase`, `ConnectionStrings:DefaultTenantDatabase`

### Key Domain Entities

`Tenant`, `User`, `Customer`, `Load`, `Trip`, `Employee/Driver`, `Invoice`, `Payment`, `Truck`, `Document`, `Subscription`, `EldProviderConfiguration`, `DriverHosStatus`, `EldDriverMapping`

### Service Ports

| Service | Port |
|---------|------|
| API | 7000 |
| Identity Server | 7001 |
| Admin App (Blazor) | 7002 |
| Office App (Angular) | 7003 |
| Aspire Dashboard | 8100 |

## Code Patterns

### Backend Patterns

- **Commands/Queries**: `src/Core/Logistics.Application/Commands/` and `Queries/` using MediatR
- **Validation**: FluentValidation in `Behaviors/ValidationBehavior.cs`
- **Domain Events**: Entities raise events, handlers in Application layer
- **Specifications**: Reusable query filters in `Specifications/`

### Adding a New API Endpoint

1. Create Command/Query in `Logistics.Application/Commands/` or `Queries/`
2. Add validation in the handler or separate validator
3. Create Controller method in `Logistics.API/Controllers/`
4. DTOs go in `Logistics.Shared.Models/`

### Entity-to-DTO Mapping with Riok.Mapperly

Use [Riok.Mapperly](https://mapperly.riok.app/) for compile-time entity-to-DTO mapping. Mappers are located in `src/Core/Logistics.Mappings/`.

**Pattern:**

```csharp
[Mapper]
public static partial class MyEntityMapper
{
    // Basic mapping - Mapperly generates the implementation
    [MapperIgnoreSource(nameof(MyEntity.NavigationProperty))]
    [MapperIgnoreTarget(nameof(MyDto.ComputedField))]
    [MapProperty(nameof(MyEntity.Related), nameof(MyDto.RelatedName), Use = nameof(MapRelatedName))]
    public static partial MyDto ToDto(this MyEntity entity);

    // Overload with additional computed fields
    public static MyDto ToDto(this MyEntity entity, int computedValue)
    {
        var dto = entity.ToDto();
        return dto with { ComputedField = computedValue };
    }

    // Custom property mapper
    private static string? MapRelatedName(RelatedEntity? related) => related?.Name;
}
```

**Key attributes:**

- `[Mapper]` - Marks the static partial class as a Mapperly mapper
- `[MapperIgnoreSource]` - Ignore source property (navigation properties, internal fields)
- `[MapperIgnoreTarget]` - Ignore target property (computed fields set manually)
- `[MapProperty(..., Use = nameof(...))]` - Use custom mapper method for property

**Usage in handlers:**

```csharp
// Single entity
var dto = entity.ToDto();

// With computed fields
var dto = entity.ToDto(computedValue);

// Collections
var dtos = entities.Select(e => e.ToDto()).ToList();
```

**Do NOT** manually map properties in handlers. Always create or extend mappers in `Logistics.Mappings`.

### Angular Patterns (Office App)

- Described in detail in `src/Client/Logistics.OfficeApp/CLAUDE.md`

## External Integrations

- **Stripe**: Payment processing, webhooks at `/webhooks/stripe`
- **Firebase**: Push notifications to mobile
- **SignalR**: Real-time GPS tracking and notifications
- **Azure Blob Storage**: Document storage
- **Mapbox**: Maps in Office and Driver apps
- **ELD Providers**: Hours of Service (HOS) data from Samsara, Motive (KeepTruckin), webhooks at `/webhooks/eld/*`

### ELD Integration Architecture

The ELD (Electronic Logging Device) integration pulls driver Hours of Service data from external providers for FMCSA compliance.

**Key Components:**

- `IEldProviderService` - Interface for all ELD operations (in Application layer)
- `IEldProviderFactory` - Factory to get provider by type
- `SamsaraEldService`, `MotiveEldService` - Provider implementations (in Infrastructure layer)
- `EldSyncJob` - Hangfire job that syncs HOS data every 5 minutes (fallback for webhooks)

**Data Flow:**

1. Configure ELD provider credentials per tenant via API
2. Map TMS employees to external ELD driver IDs
3. Webhooks receive real-time updates, or background job polls every 5 minutes
4. HOS data stored in `DriverHosStatus` entity, displayed in Office App ELD Dashboard

**Configuration** (`appsettings.json`):

```json
"Eld": {
  "Samsara": { "BaseUrl": "https://api.samsara.com" },
  "Motive": { "BaseUrl": "https://api.keeptruckin.com/v1" }
}
```

## User Roles

`SuperAdmin` (Admin App), `Owner`, `Manager`, `Dispatcher` (Office App), `Driver` (Mobile App)

## Important Notes

- `Logistics.DriverApp.Legacy` (MAUI) is deprecated; use the Kotlin Multiplatform version
- Angular app has its own rules in `src/Client/Logistics.OfficeApp/CLAUDE.md`
- Aspire automatically runs DB migrations on startup
