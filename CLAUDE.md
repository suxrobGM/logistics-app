# CLAUDE.md

Multi-tenant fleet management platform for trucking companies (intermodal containers, vehicle transport).

## Build & Run

```bash
# Full stack (recommended) - requires Docker
dotnet run --project src/Aspire/Logistics.Aspire.AppHost  # Dashboard: http://localhost:7100

# Backend only
dotnet build                                              # Build all
dotnet test                                               # Run all tests
dotnet test --filter "ClassName"                          # Filter by class
dotnet run --project src/Presentation/Logistics.API      # API: https://localhost:7000

# Frontend (Angular + Demo Video) - uses bun workspaces
bun install                                               # Install all workspaces
bun start:tms                                             # TMS Portal dev server

# Demo Video (Remotion) - see /remotion-best-practices skill
bun dev:video                                             # Remotion Studio
bun build:video                                           # Render MP4

# Mobile (Kotlin Multiplatform)
cd src/Client/Logistics.DriverApp && ./gradlew assembleDebug
```

## Service Ports

| Service          | Port |
| ---------------- | ---- |
| API              | 7000 |
| Admin Portal     | 7002 |
| Identity Server  | 7001 |
| TMS Portal       | 7003 |
| Customer Portal  | 7004 |
| Website          | 7005 |
| Aspire Dashboard | 7100 |

## Architecture

- **DDD + CQRS**: Commands/Queries via MediatR in `src/Core/Logistics.Application/`
- **Multi-Tenant**: Master DB (tenants, subscriptions) + Tenant DBs (sharded per company)
- **Lazy Loading**: EF Core lazy loading enabled - no `.Include()` needed
- **Clean Architecture**: Application layer references only interfaces; all implementations in Infrastructure layer
- **Modular Infrastructure**: Split into focused projects (Persistence, Communications, Integrations, etc.)

### Infrastructure Projects

| Project                                             | Purpose                                                                                    |
| --------------------------------------------------- | ------------------------------------------------------------------------------------------ |
| **Logistics.Infrastructure.Persistence**            | EF Core DbContexts, repositories, migrations, multi-tenancy                                |
| **Logistics.Infrastructure.Communications**         | SignalR hubs, real-time services, email, push notifications                                |
| **Logistics.Infrastructure.Integrations.Eld**       | ELD provider integrations (Samsara, Motive)                                                |
| **Logistics.Infrastructure.Integrations.LoadBoard** | Load board integrations (DAT, Truckstop, 123Loadboard)                                     |
| **Logistics.Infrastructure.Payments**               | Stripe payment processing and Stripe Connect                                               |
| **Logistics.Infrastructure.Documents**              | PDF generation, document storage, VIN decoder                                              |
| **Logistics.Infrastructure.Routing**                | Trip optimization, route planning, geocoding                                               |
| **Logistics.Infrastructure.Storage**                | Azure Blob Storage and file-based storage                                                  |
| **Logistics.Infrastructure.AI**                     | Multi-provider LLM dispatch agent (Anthropic, OpenAI, DeepSeek), tool registry, agent loop |

### Presentation Projects

| Project                      | Purpose                                                                                  |
| ---------------------------- | ---------------------------------------------------------------------------------------- |
| **Logistics.API**            | REST API, controllers, middleware, Hangfire jobs                                         |
| **Logistics.IdentityServer** | Duende IdentityServer, JWT auth, user management                                         |
| **Logistics.DbMigrator**     | EF Core migrations runner for master and tenant DBs                                      |
| **Logistics.McpServer**      | MCP server exposing dispatch tools to external AI clients (Claude Desktop, Cursor, etc.) |

### Test Projects

| Project                             | Tests For                       | Key Packages                      |
| ----------------------------------- | ------------------------------- | --------------------------------- |
| `Logistics.Application.Tests`       | Application layer (handlers)    | xUnit, NSubstitute                |
| `Logistics.Infrastructure.AI.Tests` | AI agent, quota, tools, prompts | xUnit, NSubstitute, MockQueryable |

## Coding Patterns

### Enum Display Names

Use `GetDescription()` from `Logistics.Domain.Primitives.Enums.EnumExtensions` for display strings. It auto-humanizes enum names (e.g., `PickedUp` → "Picked Up"), so `[Description]` is only needed when the display differs significantly:

- Acronyms: `Eld` → "ELD / HOS"
- Special formatting: `OnDutyNotDriving` → "On Duty (Not Driving)"
- Domain-specific: `Driving11Hour` → "11-Hour Driving Limit"

```csharp
// Most enums - no attribute needed
public enum LoadStatus { Draft, PickedUp, InTransit, Delivered }
// GetDescription() returns: "Draft", "Picked Up", "In Transit", "Delivered"

// Only use [Description] when auto-humanization isn't enough
public enum TenantFeature
{
    Dispatch,                              // → "Dispatch" (auto)
    [Description("ELD / HOS")] Eld,        // → "ELD / HOS" (custom)
    [Description("Safety & Compliance")] Safety
}
```

### API Query Sorting

Use `-FieldName` prefix for descending order, plain `FieldName` for ascending. Do NOT use "FieldName desc" or "FieldName asc" syntax.

```typescript
// Good
OrderBy: "-CreatedAt"; // Descending
OrderBy: "Name"; // Ascending

// Bad
OrderBy: "CreatedAt desc";
OrderBy: "Name asc";
```

## Key Entities

`Tenant`, `User`, `Customer`, `Load`, `Trip`, `Employee/Driver`, `Invoice`, `Payment`, `Truck`, `Document`, `DispatchSession`, `DispatchDecision`, `ApiKey`

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
- **LLM Providers**: AI dispatch agent with pluggable providers (Anthropic Claude, OpenAI GPT, DeepSeek). Tiered model access by plan (Base/Premium/Ultra), multiplier-based request quotas (1x/5x/10x)
- **MCP Server**: Streamable HTTP endpoint at `/mcp` exposing dispatch tools to external AI clients (Claude Desktop, Cursor, Windsurf). Authenticated via tenant-specific API keys (`logsx_{tenantId}_{random}`), rate-limited at 100 req/min per key. Tools are generated dynamically from `DispatchToolRegistry` — single source of truth shared with the AI dispatch agent. Project: `src/Presentation/Logistics.McpServer/`
