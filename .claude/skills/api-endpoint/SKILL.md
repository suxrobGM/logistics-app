---
name: api-endpoint
description: Workflow for adding new API endpoints
---

# Adding a New API Endpoint

Follow these steps to add a new API endpoint:

## 1. Create Command or Query

Location: `src/Core/Logistics.Application/Commands/` or `Queries/`

**Command (for mutations):**

```csharp
public record CreateCustomerCommand(string Name, string Email) : IRequest<Result<CustomerDto>>;

internal sealed class CreateCustomerCommandHandler(
    ITenantUnitOfWork unitOfWork)
    : IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken ct)
    {
        var customer = Customer.Create(request.Name, request.Email);
        unitOfWork.Repository<Customer>().Add(customer);
        await unitOfWork.SaveChangesAsync(ct);
        return customer.ToDto();
    }
}
```

**Query (for reads):**

```csharp
public record GetCustomerQuery(Guid Id) : IRequest<Result<CustomerDto>>;

internal sealed class GetCustomerQueryHandler(
    ITenantUnitOfWork unitOfWork)
    : IRequestHandler<GetCustomerQuery, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(GetCustomerQuery request, CancellationToken ct)
    {
        var customer = await unitOfWork.Repository<Customer>().GetByIdAsync(request.Id, ct);
        return customer is null
            ? Result.Fail<CustomerDto>("Customer not found")
            : customer.ToDto();
    }
}
```

## 2. Add Validation (Optional)

Add validation in the handler or create a separate validator:

```csharp
internal sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
```

## 3. Create Controller Method

Location: `src/Presentation/Logistics.API/Controllers/`

```csharp
[ApiController]
[Route("api/[controller]")]
public class CustomersController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<CustomerDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerCommand command)
    {
        var result = await sender.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<CustomerDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await sender.Send(new GetCustomerQuery(id));
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
}
```

## 4. Create DTOs

Location: `src/Core/Logistics.Shared.Models/`

```csharp
public record CustomerDto(Guid Id, string Name, string Email);
```

## 5. Create Mapper

Location: `src/Core/Logistics.Mappings/`

See the `mapperly` skill for mapping patterns.
