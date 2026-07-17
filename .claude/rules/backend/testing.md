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
| `Logistics.Architecture.Tests`      | Layering / dependency boundaries             | `TngTech.ArchUnitNET.xUnit` |

Other `Logistics.Infrastructure.*.Tests` projects cover their own infrastructure module.

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

## Architecture tests

`Logistics.Architecture.Tests` **discovers** what it covers — never reintroduce an `InlineData`
roster there, since a hand-maintained list silently skips whatever nobody remembered to add (which
is how two infra projects went unchecked).

- `CsprojReferenceTests.InfrastructureProjects` enumerates `src/Infrastructure/*` from disk.
- `BoundaryTests.InfrastructureAssemblies` derives from `AssemblyAnchors.AllInfrastructure`.
- Exemptions: the local `exempt` array and `AssemblyAnchors.BoundaryExempt` (both just
  `Infrastructure.AI`). A boundary failure means fix the dependency, not add an exemption.

Adding an infra project: the csproj rule picks it up automatically; the IL-level rule also needs
an anchor in `AssemblyAnchors.AllInfrastructure` + a `ProjectReference` in the arch-tests csproj.

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
