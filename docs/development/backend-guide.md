# Backend Development Guide

Patterns and conventions for .NET backend development.

## Project Structure

```text
src/Core/Logistics.Application/
├── Commands/           # Write operations
│   └── Load/
│       ├── CreateLoadCommand.cs
│       └── CreateLoadHandler.cs
├── Queries/            # Read operations
│   └── Load/
│       ├── GetLoadByIdQuery.cs
│       └── GetLoadByIdHandler.cs
├── Services/           # Application services
├── Behaviors/          # MediatR pipeline
└── Validators/         # FluentValidation

src/Core/Logistics.Domain/
├── Entities/           # Domain entities
├── ValueObjects/       # Value objects
├── Events/             # Domain events
├── Specifications/     # Query specifications
└── Interfaces/         # Abstractions

src/Infrastructure/Logistics.Infrastructure/
├── Data/               # DbContext, migrations
├── Repositories/       # Repository implementations
└── Services/           # External service integrations
```

## Adding a New Feature

### Step 1: Create Command/Query

```csharp
// Commands/Load/CreateLoadCommand.cs
public record CreateLoadCommand(CreateLoadDto Dto) : IRequest<DataResult<LoadDto>>;

public class CreateLoadHandler : IRequestHandler<CreateLoadCommand, DataResult<LoadDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateLoadHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DataResult<LoadDto>> Handle(
        CreateLoadCommand request,
        CancellationToken cancellationToken)
    {
        var load = Load.Create(request.Dto.CustomerId, request.Dto.Origin, request.Dto.Destination);

        await _unitOfWork.Loads.AddAsync(load);
        await _unitOfWork.SaveChangesAsync();

        return DataResult<LoadDto>.Success(load.ToDto());
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
    public async Task<ActionResult<DataResult<LoadDto>>> Create(CreateLoadDto dto)
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

- Use `DataResult<T>` for single items
- Use `PagedDataResult<T>` for lists

### Error Handling

- Throw domain exceptions for business rule violations
- Return `DataResult.Failure()` for expected failures
- Let middleware handle unexpected exceptions

## Next Steps

- [Angular Guide](angular-guide.md) - Frontend patterns
- [Testing](testing.md) - Test strategies
