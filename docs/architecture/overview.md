# Architecture Overview

Logistics TMS follows Domain-Driven Design (DDD) with CQRS pattern.

## System Architecture

![Architecture Diagram](diagrams/project_architecture.jpg)

## Layer Structure

```
┌─────────────────────────────────────────────────────────────────┐
│                     Presentation Layer                          │
│  Logistics.API │ Logistics.IdentityServer │ Logistics.AdminApp  │
└─────────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────────┐
│                     Application Layer                           │
│         Commands │ Queries │ Services │ SignalR Hubs           │
└─────────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────────┐
│                       Domain Layer                              │
│        Entities │ Value Objects │ Domain Events │ Specs        │
└─────────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────────┐
│                    Infrastructure Layer                         │
│              EF Core │ Repositories │ Unit of Work             │
└─────────────────────────────────────────────────────────────────┘
```

## Project Structure

```
src/
├── Aspire/
│   └── Logistics.Aspire.AppHost       # Orchestration
├── Client/
│   ├── Logistics.OfficeApp            # Angular frontend
│   ├── Logistics.DriverApp            # Kotlin Multiplatform mobile
│   └── Logistics.HttpClient           # Shared API client
├── Core/
│   ├── Logistics.Application          # Business logic (CQRS)
│   ├── Logistics.Domain               # Entities, domain events
│   └── Logistics.Shared               # DTOs, shared models
├── Infrastructure/
│   └── Logistics.Infrastructure       # EF Core, repositories
└── Presentation/
    ├── Logistics.API                  # REST API
    ├── Logistics.IdentityServer       # OAuth2/OIDC
    ├── Logistics.AdminApp             # Blazor admin
    └── Logistics.DbMigrator           # Migrations runner
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
| Angular 21 | Office App SPA |
| PrimeNG | UI components |
| Blazor Server | Admin App |
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
| Azure Blob Storage | File storage (optional) |

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

```
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

## Multi-Tenancy

See [Multi-Tenancy Architecture](multi-tenancy.md) for details.

## Next Steps

- [Domain Model](domain-model.md) - Entity relationships
- [Multi-Tenancy](multi-tenancy.md) - Database isolation strategy
- [API Overview](../api/overview.md) - REST API design
