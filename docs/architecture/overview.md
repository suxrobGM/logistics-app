# Architecture Overview

Logistics TMS follows Domain-Driven Design (DDD) with CQRS pattern.

## System Architecture

![Architecture Diagram](diagrams/project_architecture.jpg)

## Layer Structure

```text
┌─────────────────────────────────────────────────────────────────┐
│                     Presentation Layer                          │
│  Logistics.API │ Logistics.IdentityServer │ Logistics.DbMigrator │
└─────────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────────┐
│                     Application Layer                           │
│         Commands │ Queries │ Services (Interfaces)              │
└─────────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────────┐
│                 Application Contracts Layer                     │
│      Service Interfaces │ Realtime Abstractions                 │
└─────────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────────┐
│                       Domain Layer                              │
│        Entities │ Value Objects │ Domain Events │ Specs         │
└─────────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────────┐
│                    Infrastructure Layer                         │
│    8 Focused Projects: Persistence, Communications,            │
│    Integrations (ELD, LoadBoard), Payments, Documents,         │
│    Routing, Storage                                             │
└─────────────────────────────────────────────────────────────────┘
```

## Project Structure

```text
src/
├── Aspire/
│   ├── Logistics.Aspire.AppHost            # Orchestration
│   └── Logistics.Aspire.ServiceDefaults    # Aspire service defaults
├── Client/
│   ├── Logistics.Angular/                  # Angular workspace
│   │   ├── projects/
│   │   │   ├── shared/                     # @logistics/shared library
│   │   │   ├── tms-portal/                 # TMS Portal (dispatchers)
│   │   │   ├── admin-portal/               # Admin Portal (super admin)
│   │   │   └── customer-portal/            # Customer Portal (self-service)
│   │   └── angular.json
│   └── Logistics.DriverApp                 # Kotlin Multiplatform mobile
├── Core/
│   ├── Logistics.Application               # Business logic (CQRS)
│   ├── Logistics.Application.Contracts     # Service interfaces
│   ├── Logistics.Domain                    # Entities, domain events
│   ├── Logistics.Domain.Primitives         # Value objects, enums
│   └── Logistics.Mappings                  # Entity-to-DTO mappers
├── Infrastructure/
│   ├── Logistics.Infrastructure.Persistence            # EF Core, DbContexts, migrations
│   ├── Logistics.Infrastructure.Communications         # SignalR, email, notifications
│   ├── Logistics.Infrastructure.Integrations.Eld       # Samsara, Motive integrations
│   ├── Logistics.Infrastructure.Integrations.LoadBoard # DAT, Truckstop, 123Loadboard
│   ├── Logistics.Infrastructure.Payments               # Stripe services
│   ├── Logistics.Infrastructure.Documents              # PDF, storage, VIN decoder
│   ├── Logistics.Infrastructure.Routing                # Trip optimization, geocoding
│   └── Logistics.Infrastructure.Storage                # Azure Blob, file storage
├── Shared/
│   ├── Logistics.Shared.Geo                # Geolocation utilities
│   ├── Logistics.Shared.Identity           # Identity models
│   └── Logistics.Shared.Models             # DTOs for contracts
└── Presentation/
    ├── Logistics.API                       # REST API
    ├── Logistics.IdentityServer            # OAuth2/OIDC
    └── Logistics.DbMigrator                # Migrations runner
```

## Tech Stack

### Backend

| Technology | Purpose |
|------------|---------|
| .NET 10 | Runtime |
| ASP.NET Core | Web framework |
| Entity Framework Core | ORM |
| Duende IdentityServer | OAuth2/OIDC |
| MediatR | CQRS mediator |
| FluentValidation | Request validation |
| Serilog | Structured logging |
| SignalR | Real-time communication |
| Hangfire | Background jobs |

### Frontend

| Technology | Purpose |
|------------|---------|
| Angular 21 | TMS Portal & Customer Portal |
| PrimeNG | UI components |
| Angular 21 | Admin App |
| Kotlin Multiplatform | Driver Mobile App |
| Compose Multiplatform | Mobile UI |

### Infrastructure

| Technology | Purpose |
|------------|---------|
| PostgreSQL 18 | Database |
| Docker | Containerization |
| .NET Aspire | Orchestration |
| Nginx | Reverse proxy |
| GitHub Actions | CI/CD |

### External Services

| Service | Purpose |
|---------|---------|
| Stripe | Payment processing |
| Firebase | Push notifications |
| Mapbox | Maps and geocoding |
| Azure Blob Storage | Document/photo storage |
| NHTSA API | VIN decoding (vehicle info) |

## Design Patterns

### CQRS (Command Query Responsibility Segregation)

Commands and queries are separated:

```csharp
// Command - modifies state
public record CreateLoadCommand(CreateLoadDto Dto) : IRequest<DataResult<LoadDto>>;

// Query - reads state
public record GetLoadByIdQuery(string Id) : IRequest<DataResult<LoadDto>>;
```

### MediatR Pipeline

Requests flow through behaviors:

```text
Request → ValidationBehavior → LoggingBehavior → Handler → Response
```

### Repository Pattern

Generic repository with specifications:

```csharp
public interface IRepository<T> where T : class, IAggregateRoot
{
    Task<T?> GetAsync(ISpecification<T> spec);
    Task<IList<T>> GetListAsync(ISpecification<T> spec);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}
```

### Specification Pattern

Reusable query conditions:

```csharp
public class ActiveLoadsSpec : Specification<Load>
{
    public ActiveLoadsSpec()
    {
        Query.Where(l => l.Status == LoadStatus.Active)
             .OrderByDescending(l => l.CreatedDate);
    }
}
```

### Domain Events

Entities raise events for side effects:

```csharp
public class Load : AggregateRoot
{
    public void Complete()
    {
        Status = LoadStatus.Completed;
        AddDomainEvent(new LoadCompletedEvent(this));
    }
}
```

### Unit of Work

Transaction management across repositories:

```csharp
public interface IUnitOfWork
{
    IRepository<Load> Loads { get; }
    IRepository<Trip> Trips { get; }
    Task<int> SaveChangesAsync();
}
```

## Infrastructure Projects

The infrastructure layer is split into 8 focused projects for better maintainability:

### Logistics.Infrastructure.Persistence

**Purpose**: Database access, repositories, multi-tenancy

**Contents**:

- MasterDbContext (tenants, subscriptions)
- TenantDbContext (operational data per company)
- 43 Entity Configurations
- Migrations (Master and Tenant)
- Repository and Unit of Work implementations
- Multi-tenancy services
- EF Core interceptors (domain events, auditing)

### Logistics.Infrastructure.Communications

**Purpose**: Real-time communication and messaging

**Contents**:

- SignalR Hubs: `TrackingHub`, `ChatHub`, `NotificationHub`
- Real-time service implementations (wraps SignalR)
- Email services (SMTP, Fluid templates)
- Push notifications (Firebase)
- Captcha validation (Google reCAPTCHA)

### Logistics.Infrastructure.Integrations.Eld

**Purpose**: ELD provider integrations for HOS compliance

**Providers**:

- Samsara ELD
- Motive (KeepTruckin) ELD
- Demo provider for testing

**Features**: Factory pattern for provider selection, webhook handlers

### Logistics.Infrastructure.Integrations.LoadBoard

**Purpose**: Load board integrations for freight search

**Providers**:

- DAT Load Board
- Truckstop.com
- 123Loadboard
- Demo provider for testing

**Features**: Search loads, post trucks, provider-specific mappers

### Logistics.Infrastructure.Payments

**Purpose**: Payment processing

**Contents**:

- Stripe standard payment processing
- Stripe Connect (destination charges)
- Payment link generation
- Webhook handling

### Logistics.Infrastructure.Documents

**Purpose**: Document generation and storage

**Contents**:

- PDF generation (QuestPDF) - invoices, payroll stubs
- PDF import with template-based extraction
- Azure Blob Storage service
- File-based storage service
- VIN decoder (NHTSA API)

### Logistics.Infrastructure.Routing

**Purpose**: Trip optimization and geocoding

**Contents**:

- Heuristic trip optimizer (nearest neighbor)
- Mapbox Matrix trip optimizer
- Composite optimizer (combines both)
- Mapbox geocoding service
- Trip tracking service

### Logistics.Infrastructure.Storage

**Purpose**: File and document storage

**Contents**:

- Azure Blob Storage implementation
- File-based storage implementation
- Storage abstraction layer

## Multi-Tenancy

See [Multi-Tenancy Architecture](multi-tenancy.md) for details.

## Next Steps

- [Domain Model](domain-model.md) - Entity relationships
- [Multi-Tenancy](multi-tenancy.md) - Database isolation strategy
- [API Overview](../api/overview.md) - REST API design
