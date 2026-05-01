---
paths:
  - "test/*"
---

# Testing Conventions

## Test Projects

| Project                             | Tests For                                    | Extra Dependencies          |
| ----------------------------------- | -------------------------------------------- | --------------------------- |
| `Logistics.Application.Tests`       | Application layer (handlers, services)       | —                           |
| `Logistics.Infrastructure.AI.Tests` | AI agent, quota, tools, prompts, LLM pricing | `MockQueryable.NSubstitute` |

## Stack

- **Framework**: xUnit
- **Mocking**: NSubstitute (`Substitute.For<T>()`)
- **IQueryable mocking**: `MockQueryable.NSubstitute` (`.BuildMock()`) for EF Core queries
- **Coverage**: coverlet

## Conventions

- Test class name: `{ClassUnderTest}Tests`
- Test method name: `{Method}_{Scenario}_{ExpectedResult}` (e.g., `GetQuotaStatus_OverQuota_RemainingIsZero`)
- Use `#region` blocks to group related tests (e.g., `#region Session counting`)
- Field naming: `sut` for the system under test
- Use `[Fact]` for single cases, `[Theory]` + `[InlineData]` for parameterized tests
- Inject mocked dependencies via constructor, wire them in the constructor body

## Running Tests

```bash
dotnet test                                    # All tests
dotnet test --filter "ClassName"               # Filter by class
dotnet test --filter "FullyQualifiedName~Method" # Filter by method
```

## Patterns

```csharp
// Setup mocks
private readonly IMyRepo repo = Substitute.For<IMyRepo>();
private readonly MyService sut;

public MyServiceTests()
{
    sut = new MyService(repo);
}

// Mock IQueryable (for EF Core .Query() chains)
var mock = items.ToList().BuildMock();
repo.Query().Returns(mock);

// Mock async repository methods
repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(entity);
```
