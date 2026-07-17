# Backend Development Guide

Patterns and conventions for .NET backend development.

## Project Structure

```text
src/Core/Logistics.Application/
├── Modules/            # Feature code grouped by bounded context
│   ├── Operations/     # Loads, Trips, Trucks, Containers, Terminals, …
│   │   └── Loads/
│   │       ├── Commands/CreateLoad/
│   │       └── Queries/GetLoadById/
│   ├── Compliance/     # ELD, DVIR, Accidents, Safety, Privacy, Ifta
│   ├── Financial/      # Invoices, Payments, Payroll, Expenses, Tax
│   ├── IdentityAccess/ # Users, Tenants, Roles, Subscriptions
│   ├── Integrations/   # AiDispatch, LoadBoard, FuelCards, Accounting, Webhooks, Messaging, Documents
│   └── Platform/       # Stats, Reports, BlogPosts, Notifications
├── Services/           # Application workflow services
├── Behaviours/         # MediatR pipeline
└── Validators/         # FluentValidation

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

```csharp
// Modules/Operations/Loads/Commands/CreateLoad/CreateLoadCommand.cs
public record CreateLoadCommand(CreateLoadDto Dto) : ICommand<Result<LoadDto>>;

internal sealed class CreateLoadHandler(ITenantUnitOfWork unitOfWork)
    : IRequestHandler<CreateLoadCommand, Result<LoadDto>>
{
    public async Task<Result<LoadDto>> Handle(
        CreateLoadCommand req,
        CancellationToken ct)
    {
        var load = Load.Create(req.Dto.CustomerId, req.Dto.Origin, req.Dto.Destination);

        await unitOfWork.Repository<Load>().AddAsync(load, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<LoadDto>.Ok(load.ToDto());
    }
}
```

### Step 2: Add Validation

```csharp
// Validators/CreateLoadValidator.cs
public class CreateLoadValidator : AbstractValidator<CreateLoadCommand>
{
    public CreateLoadValidator()
    {
        RuleFor(x => x.Dto.CustomerId)
            .NotEmpty()
            .WithMessage("Customer is required");

        RuleFor(x => x.Dto.Origin)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Dto.Destination)
            .NotEmpty()
            .MaximumLength(200);
    }
}
```

### Step 3: Create Controller Endpoint

```csharp
// Controllers/LoadsController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LoadsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LoadsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Owner,Manager,Dispatcher")]
    public async Task<ActionResult<Result<LoadDto>>> Create(CreateLoadDto dto)
    {
        var result = await _mediator.Send(new CreateLoadCommand(dto));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
```

### Step 4: Define DTO

```csharp
// Logistics.Shared.Models/LoadDto.cs
public record CreateLoadDto
{
    public string CustomerId { get; init; } = string.Empty;
    public string Origin { get; init; } = string.Empty;
    public string Destination { get; init; } = string.Empty;
}

public record LoadDto
{
    public string Id { get; init; } = string.Empty;
    public string CustomerId { get; init; } = string.Empty;
    public string Origin { get; init; } = string.Empty;
    public string Destination { get; init; } = string.Empty;
    public LoadStatus Status { get; init; }
    public DateTime CreatedDate { get; init; }
}
```

## Domain Entities

### Entity Base Class

```csharp
public abstract class Entity
{
    public string Id { get; protected set; } = Guid.NewGuid().ToString();
    public DateTime CreatedDate { get; protected set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; protected set; }
}

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
    public string CustomerId { get; private set; }
    public string Origin { get; private set; }
    public string Destination { get; private set; }
    public LoadStatus Status { get; private set; }

    private Load() { } // EF Core

    public static Load Create(string customerId, string origin, string destination)
    {
        var load = new Load
        {
            CustomerId = customerId,
            Origin = origin,
            Destination = destination,
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
        ModifiedDate = DateTime.UtcNow;
        AddDomainEvent(new LoadCompletedEvent(this));
    }
}
```

## Specifications

```csharp
public class ActiveLoadsByCustomerSpec : Specification<Load>
{
    public ActiveLoadsByCustomerSpec(string customerId)
    {
        Query
            .Where(l => l.CustomerId == customerId)
            .Where(l => l.Status == LoadStatus.Active)
            .OrderByDescending(l => l.CreatedDate);
    }
}

// Usage
var loads = await _unitOfWork.Loads.GetListAsync(
    new ActiveLoadsByCustomerSpec(customerId));
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

Recurring Hangfire jobs live in `src/Presentation/Logistics.API/Jobs/`. Two rules matter here.

**Fan out with `TenantJobRunner`.** `TenantJobRunner.ForEachTenantAsync(scopeFactory, logger, operation, body, ct)` runs the body once per tenant that has a provisioned database, giving each tenant its own DI scope and its own try/catch so one tenant's failure does not abort the rest of the cycle. Do not hand-roll the loop.

**Check features yourself.** Jobs bypass the MediatR pipeline, so `FeatureCheckBehaviour` never runs and `[RequiresFeature]` has no effect. A job that needs a feature gate must call `IFeatureService.IsFeatureEnabledAsync(tenantId, feature)` inside the body. Keep the check in the body rather than hoisting it to the runner: a job may still need to do some work for every tenant regardless of the flag, as `IftaQuarterCloseJob` does when it purges breadcrumbs for tenants without IFTA enabled.

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

```csharp
public class CreateLoadHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly CreateLoadHandler _handler;

    public CreateLoadHandlerTests()
    {
        _unitOfWork = new Mock<IUnitOfWork>();
        _handler = new CreateLoadHandler(_unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_ValidLoad_ReturnsSuccess()
    {
        // Arrange
        var command = new CreateLoadCommand(new CreateLoadDto
        {
            CustomerId = "cust-1",
            Origin = "Chicago",
            Destination = "New York"
        });

        _unitOfWork.Setup(x => x.Loads.AddAsync(It.IsAny<Load>()))
            .Returns(Task.CompletedTask);
        _unitOfWork.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }
}
```

## Conventions

### Naming

- Commands: `{Verb}{Entity}Command` (CreateLoadCommand)
- Queries: `Get{Entity}Query` (GetLoadByIdQuery)
- Handlers: `{Command/Query}Handler`
- Validators: `{Command}Validator`

### Response Types

- Use `Result<T>` for single items
- Use `PagedResult<T>` for lists

### Error Handling

- Throw domain exceptions for business rule violations
- Return `Result.Fail("message")` for expected failures. When the client must branch on the failure (show an upgrade dialog, offer an override), use the `Result.Fail(message, ErrorCodes.X)` overload so it carries a machine-readable code instead of a message the client has to substring-match
- Let middleware handle unexpected exceptions

## Next Steps

- [Angular Guide](angular-guide.md) - Frontend patterns
- [Testing](testing.md) - Test strategies
