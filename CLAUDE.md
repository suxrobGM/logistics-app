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
| Admin Portal         | 7002 |
| Identity Server      | 7001 |
| TMS Portal           | 7003 |
| Customer Portal      | 7004 |
| Website              | 7005 |
| Aspire Dashboard     | 7100 |

## Architecture

- **DDD + CQRS**: Commands/Queries via MediatR in `src/Core/Logistics.Application/`
- **Multi-Tenant**: Master DB (tenants, subscriptions) + Tenant DBs (sharded per company)
- **Lazy Loading**: EF Core lazy loading enabled - no `.Include()` needed
- **Clean Architecture**: Application layer references only interfaces; all implementations in Infrastructure layer
- **Modular Infrastructure**: Split into focused projects (Persistence, Communications, Integrations, etc.)

### Infrastructure Projects

| Project | Purpose |
|---------|---------|
| **Logistics.Infrastructure.Persistence** | EF Core DbContexts, repositories, migrations, multi-tenancy |
| **Logistics.Infrastructure.Communications** | SignalR hubs, real-time services, email, push notifications |
| **Logistics.Infrastructure.Integrations.Eld** | ELD provider integrations (Samsara, Motive) |
| **Logistics.Infrastructure.Integrations.LoadBoard** | Load board integrations (DAT, Truckstop, 123Loadboard) |
| **Logistics.Infrastructure.Payments** | Stripe payment processing and Stripe Connect |
| **Logistics.Infrastructure.Documents** | PDF generation, document storage, VIN decoder |
| **Logistics.Infrastructure.Routing** | Trip optimization, route planning, geocoding |
| **Logistics.Infrastructure.Storage** | Azure Blob Storage and file-based storage |

## Coding Patterns

### Enum Display Names

Use `[Description]` attribute on enum values and the `GetDescription()` extension method from `Logistics.Domain.Primitives.Enums.EnumExtensions` for display strings. Do NOT create manual switch expressions for enum-to-string mappings.

```csharp
// Good - use Description attribute
EventTypeDisplay = entity.EventType.GetDescription()

// Bad - manual switch expression
EventTypeDisplay = eventType switch
{
    EventType.Foo => "Foo Display",
    _ => "Unknown"
}
```

## Key Entities

`Tenant`, `User`, `Customer`, `Load`, `Trip`, `Employee/Driver`, `Invoice`, `Payment`, `Truck`, `Document`

### Invoice System

Three invoice types using Table Per Hierarchy (TPH):

- **LoadInvoice**: Auto-created when a Load is created, linked to Customer
- **PayrollInvoice**: For employee salary payments
- **SubscriptionInvoice**: Platform subscription payments

Related entities:

- **InvoiceLineItem**: Individual line items on invoices (BaseRate, FuelSurcharge, Detention, etc.)
- **PaymentLink**: Shareable public payment links with expiration
- **Payment**: Records of payments against invoices (supports partial payments)

### Stripe Connect Integration

Payments flow directly to trucking company bank accounts via Stripe Connect:

- **Destination Charges**: Funds go to connected account with optional platform fee
- **Express Accounts**: Simplified onboarding for trucking companies
- Commands: `CreateConnectAccountCommand`, `GetOnboardingLinkQuery`, `GetConnectStatusQuery`
- Service: `IStripeConnectService` (separate from subscription `IStripeService`)

## User Roles

`SuperAdmin`, `Admin`, `Owner`, `Manager`, `Dispatcher`, `Driver`, `Customer`

## External Integrations

- **Stripe**: Payments (`/webhooks/stripe`)
- **Stripe Connect**: Direct company payments (destination charges)
- **Firebase**: Push notifications
- **SignalR**: Real-time communication (`/hubs/tracking`, `/hubs/chat`, `/hubs/notification`)
- **Azure Blob**: Document storage
- **ELD Providers**: Samsara, Motive (`/webhooks/eld/*`)
- **Load Boards**: DAT, Truckstop, 123Loadboard
- **Mapbox**: Geocoding, route optimization
- **NHTSA**: VIN decoder
