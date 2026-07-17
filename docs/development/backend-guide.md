# Backend Development Guide

Patterns and conventions for .NET backend development.

## Project Structure

```text
src/Core/Logistics.Application/
├── Modules/            # Feature code grouped by bounded context
│   ├── Operations/     # Loads, Trips, Trucks, Containers, Terminals, …
│   │   └── Loads/
│   │       ├── Commands/CreateLoad/
│   │       ├── Queries/GetLoadById/
│   │       └── Services/         # feature-scoped workflow services (e.g. ILoadService)
│   ├── Compliance/     # ELD, DVIR, Accidents, Safety, Privacy, Ifta
│   ├── Financial/      # Invoices, Payments, Payroll, Expenses, Tax
│   ├── IdentityAccess/ # Users, Tenants, Roles, Subscriptions
│   ├── Integrations/   # AiDispatch, LoadBoard, FuelCards, Accounting, Webhooks, Messaging, Documents
│   └── Platform/       # Stats, Reports, BlogPosts, Notifications
├── Handlers/           # Generic base handlers (Delete/GetById/Update) that trivial slices subclass
├── Behaviours/         # MediatR pipeline
└── Validators/         # Shared FluentValidation base validators (e.g. AddressValidator)

src/Core/Logistics.Application.Abstractions/
└── (Infrastructure ports grouped by domain: AiDispatch, Storage, Geocoding, Eld, Payments, …)

src/Core/Logistics.Domain/
├── Entities/           # Domain entities
├── ValueObjects/       # Value objects
├── Events/             # Domain events
├── Specifications/     # Query specifications
└── Interfaces/         # Abstractions

src/Infrastructure/
├── Logistics.Infrastructure.Persistence/
│   ├── Data/                      # DbContext, migrations
│   ├── Persistence/               # Repositories, UnitOfWork
│   └── Services/                  # Multi-tenancy services
│
├── Logistics.Infrastructure.Communications/
│   ├── SignalR/                   # Hubs, real-time services
│   ├── Email/                     # Email services
│   └── Notifications/             # Push notifications
│
├── Logistics.Infrastructure.AI/       # LLM providers, dispatch agent, tool registry
│
├── Logistics.Infrastructure.Integrations.Common/
│   └── HttpClientJsonExtensions.cs    # Shared HTTP JSON helpers; leaf project, no repo references
│
├── Logistics.Infrastructure.Integrations.Eld/
│   └── Providers/                 # Samsara, Motive, TtEld, Geotab, Demo
│
├── Logistics.Infrastructure.Integrations.LoadBoard/
│   └── Providers/                 # DAT, Truckstop, 123Loadboard, Demo
│
├── Logistics.Infrastructure.Integrations.FuelCards/
│   └── Providers/                 # Wex, Efs, Demo
│
├── Logistics.Infrastructure.Integrations.Accounting/
│   └── Providers/                 # QuickBooks, Demo
│
├── Logistics.Infrastructure.Payments/
│   └── Stripe/                    # Stripe, Stripe Connect
│
├── Logistics.Infrastructure.Documents/
│   └── Pdf/                       # PDF generation, template import
│
├── Logistics.Infrastructure.Routing/
│   └── Trip/                      # Trip optimization, geocoding
│
├── Logistics.Infrastructure.Storage/
│   └── (Blob storage implementations: Azure Blob, Cloudflare R2, local file system)
│
├── Logistics.Infrastructure.Tax/
│   ├── Stripe/                    # Stripe Tax calculator and config
│   ├── Manual/                    # Manual tax calculator
│   └── Data/                      # US sales tax, EU VAT, other-country rates
│
└── Logistics.Infrastructure.Vin/
    └── (VIN decoder: WMI prefix lookup + NHTSA API)
```

## Adding a New Feature

### Step 1: Create Command/Query

Commands and queries are the request contract — they are bound **directly** as the request body (flat
properties, no `*Dto` wrapper). Add a dedicated `*Request` record only when the wire shape must differ
from the command; see [api-design.md](../../.claude/rules/backend/api-design.md).

```csharp
// Modules/Operations/Loads/Commands/CreateLoad/CreateLoadCommand.cs
public class CreateLoadCommand : ICommand   // ICommand<Result<T>> when the client needs data back
{
    public string Name { get; set; } = null!;
    public Guid CustomerId { get; set; }
    public Address OriginAddress { get; set; } = null!;
    public Address DestinationAddress { get; set; } = null!;
    // …flat fields, value objects bind as nested objects
}

internal sealed class CreateLoadHandler(ITenantUnitOfWork unitOfWork)
    : IRequestHandler<CreateLoadCommand, Result>
{
    public async Task<Result> Handle(CreateLoadCommand req, CancellationToken ct)
    {
        var load = Load.Create(req.Name, req.CustomerId, req.OriginAddress, req.DestinationAddress);

        await unitOfWork.Repository<Load>().AddAsync(load, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
```

Trivial `Delete`/`GetById`/`Update` slices don't need a hand-written handler — subclass the generic base
handlers in `Logistics.Application/Handlers/` (`DeleteTenantEntityHandler`, `GetTenantEntityByIdHandler`,
`UpdateTenantEntityHandler`; master variants exist too). A command targets the master or tenant DB purely
by which unit of work its handler injects (`IMasterUnitOfWork` vs `ITenantUnitOfWork`) — there is no
separate marker interface.

### Step 2: Add Validation

Skip the validator entirely when its only rule would be `Id NotEmpty` (the handler's not-found path
already covers it). For Create/Update pairs sharing rules, use a shared-fields base validator. See
[csharp-conventions.md](../../.claude/rules/backend/csharp-conventions.md).

```csharp
public class CreateLoadValidator : AbstractValidator<CreateLoadCommand>
{
    public CreateLoadValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}
```

### Step 3: Create Controller Endpoint

Literal plural-lowercase route, primary-constructor DI, policy-based authorization per action. Full rules
in [api-design.md](../../.claude/rules/backend/api-design.md).

```csharp
// Controllers/LoadController.cs
[Route("loads")]
[Produces("application/json")]
public class LoadController(IMediator mediator) : ControllerBase
{
    [HttpPost(Name = "CreateLoad")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permission.Load.Manage)]
    public async Task<IActionResult> Create([FromBody] CreateLoadCommand request)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? NoContent() : BadRequest(ErrorResponse.FromResult(result));
    }
}
```

### Step 4: Define DTO

DTOs are the **read** shape, returned by queries and produced by Mapperly mappers (never mapped by hand —
see [mapperly.md](../../.claude/rules/backend/mapperly.md)). Entity ids are `Guid`.

```csharp
// Logistics.Shared.Models/LoadDto.cs
public record LoadDto
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public LoadStatus Status { get; init; }
    public DateTime CreatedDate { get; init; }
}
```

## Domain Entities

### Entity Base Class

```csharp
public abstract class Entity : IEntity<Guid>
{
    public Guid Id { get; set; } = Guid.NewGuid();
}

// AuditableEntity adds CreatedDate / LastModifiedDate; inherit it for almost everything.
public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

### Example Entity

```csharp
public class Load : AggregateRoot
{
    public string Name { get; private set; } = null!;
    public Guid CustomerId { get; private set; }
    public LoadStatus Status { get; private set; }

    private Load() { } // EF Core

    public static Load Create(string name, Guid customerId, Address origin, Address destination)
    {
        var load = new Load
        {
            Name = name,
            CustomerId = customerId,
            Status = LoadStatus.Pending
        };

        load.AddDomainEvent(new LoadCreatedEvent(load));
        return load;
    }

    public void Complete()
    {
        if (Status == LoadStatus.Completed)
            throw new InvalidOperationException("Load already completed");

        Status = LoadStatus.Completed;
        AddDomainEvent(new LoadCompletedEvent(this));
    }
}
```

## Specifications

Most reads use an inline `.Query()` inside the handler now. Write a dedicated `Specification<T>` class
**only** when the same query condition is reused across handlers (there are just a few left, e.g.
`FilterLoadInvoices`, `FilterPayrollInvoices`, `FilterLoadsByDeliveryDate`).

```csharp
public class ActiveLoadsByCustomerSpec : Specification<Load>
{
    public ActiveLoadsByCustomerSpec(Guid customerId)
    {
        Query
            .Where(l => l.CustomerId == customerId)
            .Where(l => l.Status == LoadStatus.Active)
            .OrderByDescending(l => l.CreatedDate);
    }
}

// Usage
var loads = await unitOfWork.Repository<Load>()
    .GetListAsync(new ActiveLoadsByCustomerSpec(customerId), ct);
```

## Domain Events

```csharp
// Define event
public record LoadCompletedEvent(Load Load) : IDomainEvent;

// Handle event
public class LoadCompletedHandler : INotificationHandler<LoadCompletedEvent>
{
    private readonly IInvoiceService _invoiceService;

    public async Task Handle(LoadCompletedEvent notification, CancellationToken ct)
    {
        await _invoiceService.GenerateInvoiceAsync(notification.Load);
    }
}
```

## Background Jobs

Recurring Hangfire jobs live in `src/Presentation/Logistics.API/Jobs/`. The canonical rule is in CLAUDE.md: fan out with `TenantJobRunner.ForEachTenantAsync` (never hand-roll the loop), and because jobs bypass the MediatR pipeline the body must gate features itself with `IFeatureService` (`[RequiresFeature]` is inert). Keep the feature check inside the body — a job may still do some work for every tenant regardless of the flag, as `IftaQuarterCloseJob` does when it purges breadcrumbs for tenants without IFTA enabled. The worked pattern:

```csharp
[AutomaticRetry(Attempts = 2)]
public Task SyncAllTenantsAsync(CancellationToken ct) =>
    TenantJobRunner.ForEachTenantAsync(scopeFactory, logger, "fuel card sync", SyncTenantAsync, ct);

private async Task SyncTenantAsync(IServiceScope scope, Tenant tenant, CancellationToken ct)
{
    var featureService = scope.ServiceProvider.GetRequiredService<IFeatureService>();
    if (!await featureService.IsFeatureEnabledAsync(tenant.Id, TenantFeature.FuelCards))
    {
        return;
    }

    // …
}
```

## Testing

xUnit + NSubstitute (not Moq). The system under test is named `sut`; private fields are `camelCase` with
no `_` prefix. Full conventions in [testing.md](../../.claude/rules/backend/testing.md).

```csharp
public class CreateLoadHandlerTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly CreateLoadHandler sut;

    public CreateLoadHandlerTests() => sut = new CreateLoadHandler(tenantUow);

    [Fact]
    public async Task Handle_ValidLoad_ReturnsSuccess()
    {
        // Arrange
        var command = new CreateLoadCommand
        {
            Name = "ACME-42",
            CustomerId = Guid.NewGuid()
        };
        tenantUow.SaveChangesAsync().Returns(1);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await tenantUow.Repository<Load>().Received(1).AddAsync(Arg.Any<Load>());
    }
}
```

## Conventions

- **Naming** (Commands/Queries/Handlers/Validators/DTOs/Mappers): see [csharp-conventions.md](../../.claude/rules/backend/csharp-conventions.md).
- **Response types**: `Result<T>` for single items, `PagedResult<T>` for lists.
- **Error handling**: throw domain exceptions for business-rule violations; return `Result.Fail(...)` for expected failures, and when the client must branch on the failure use the machine-readable-code overload. See [api-design.md](../../.claude/rules/backend/api-design.md). Let middleware handle unexpected exceptions.

## Next Steps

- [Angular Guide](angular-guide.md) - Frontend patterns
- [Testing rules](../../.claude/rules/backend/testing.md) - Test strategies
