# CLAUDE.md

Multi-tenant fleet management platform for trucking companies (intermodal containers, vehicle transport).

## Build & Run

```bash
# Full stack (recommended) - requires Docker
dotnet run --project src/Aspire/Logistics.Aspire.AppHost  # Dashboard: http://localhost:7100

# Backend only
dotnet build                                              # Build all
dotnet test                                               # Run tests
dotnet run --project src/Presentation/Logistics.API      # API: https://localhost:7000

# Frontend (Angular) - see /angular-workspace skill
cd src/Client/Logistics.Angular && bun install && bun run start:tms

# Mobile (Kotlin Multiplatform)
cd src/Client/Logistics.DriverApp && ./gradlew assembleDebug
```

## Service Ports

| Service              | Port |
| -------------------- | ---- |
| API                  | 7000 |
| Identity Server      | 7001 |
| TMS Portal           | 7003 |
| Customer Portal      | 7004 |
| Website              | 7005 |
| Aspire Dashboard     | 7100 |

## Architecture

- **DDD + CQRS**: Commands/Queries via MediatR in `src/Core/Logistics.Application/`
- **Multi-Tenant**: Master DB (tenants, subscriptions) + Tenant DBs (sharded per company)
- **Lazy Loading**: EF Core lazy loading enabled - no `.Include()` needed

## Key Entities

`Tenant`, `User`, `Customer`, `Load`, `Trip`, `Employee/Driver`, `Invoice`, `Payment`, `Truck`, `Document`

## User Roles

`SuperAdmin`, `Admin`, `Owner`, `Manager`, `Dispatcher`, `Driver`, `Customer`

## External Integrations

- **Stripe**: Payments (`/webhooks/stripe`)
- **Firebase**: Push notifications
- **SignalR**: Real-time GPS tracking
- **Azure Blob**: Document storage
- **ELD Providers**: Samsara, Motive (`/webhooks/eld/*`)
