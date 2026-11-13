---
paths:
  - "test/*"
---

# Testing Conventions

## Test Projects

| Project                                      | Tests For                                             | Extra Dependencies          |
| -------------------------------------------- | ----------------------------------------------------- | --------------------------- |
| `Logistics.Application.Tests`                | Application layer (handlers, services)                | `MockQueryable.NSubstitute` |
| `Logistics.Infrastructure.AI.Tests`          | AI agent, tools, prompts, LLM pricing                 | `MockQueryable.NSubstitute` |
| `Logistics.Infrastructure.Tests`             | Every other infrastructure project and HostDefaults, one folder each | `MockQueryable.NSubstitute`, EF InMemory, QuestPDF |
| `Logistics.Architecture.Tests`               | Layering / dependency boundaries                      | `TngTech.ArchUnitNET.xUnitV3` |

`Logistics.Infrastructure.Tests` has one folder per source project: `Persistence/` (services,
converters, AI quota), `Integrations/` (QBO, ELD, DAT, FMCSA, fuel cards, VIN, webhook signatures),
`Payments/` (Stripe, and tax under `Payments/Tax/`), `Routing/`, `Communications/`, `Documents/` and
`HostDefaults/`. The namespace follows the folder (`Logistics.Infrastructure.Tests.Payments.Tax.Data`).
`TestKit/StubHttpMessageHandler` is the one-response HTTP fake (`Json(...)`, `Throws(...)`, recorded
`Requests`). A test of infrastructure code goes there, never in `Application.Tests`.

`Application.Tests` folders mirror `src/Core/Logistics.Application/Modules/`
(`Operations/Loads/`, `Integrations/AIDispatch/`, ...) and the namespace follows the folder. Tests
of code outside `Modules/` mirror `Common/` (`Common/Pipeline/`, `Common/Validation/`, ...), and
`Primitives/` holds domain value types and extensions.

## Test kit

`Logistics.Application.Tests/TestKit/` - use it instead of hand-building fixtures:

- `TestData.Address/Load/Truck/Driver(...)` - valid entities; parameters are only the fields tests
  vary, set anything else on the result. `TestData.TripLoadCommand()` is a valid new-load request.
- `TestTenant.Create(...)` - a valid tenant (company name, MC number, distance unit).
- `uow.Stub<T>(rows)` - wires `Repository<T>()` to a substitute whose `Query`, `GetByIdAsync`,
  `GetAsync` and `GetListAsync` read `rows` on every call (pass a `List` to see rows added later);
  returns the repository for `Received` checks.
  `uow.StubLoadLookups()` stubs everything `LoadLookupBuilder` reads.
- `AgentTestContext` - the substitute rig for the AI dispatch/copilot handler tests.

`TestData.cs`, `TestTenant.cs` and `UnitOfWorkStubs.cs` are compiled into
`Logistics.Infrastructure.AI.Tests` as linked files (see its csproj), so both projects share one copy.

## Stack

- **Framework**: xUnit v3 on Microsoft.Testing.Platform (`global.json` `test.runner`); test projects are `Exe`
- **Mocking**: NSubstitute (`Substitute.For<T>()`)
- **IQueryable mocking**: `MockQueryable.NSubstitute` (`.BuildMock()`) for EF Core queries
- **Coverage**: `coverlet.MTP` (`dotnet test --coverlet`)

## Conventions

- Test class name: `{ClassUnderTest}Tests`
- Test method name: `{Method}_{Scenario}_{ExpectedResult}` (e.g., `GetQuotaStatus_OverQuota_RemainingIsZero`)
- Use `#region` blocks to group related tests (e.g., `#region Session counting`)
- Field naming: `_sut` for the system under test
- Use `[Fact]` for single cases, `[Theory]` + `[InlineData]` for parameterized tests
- A theory with a `decimal` parameter takes `[MemberData]` over a `TheoryData<...>` with `m` literals, never `[InlineData(8.10)]`: attributes can't hold decimals, so the double literal is converted, and since .NET 11 that conversion keeps binary noise (8.0999999999999996…)
- Inject mocked dependencies via constructor, wire them in the constructor body

## Architecture tests

CLAUDE.md carries the rule (discovery, never a hand-maintained roster - that is how two infra
projects went unchecked). Here is how the discovery actually works:

- `CsprojReferenceTests.InfrastructureProjects` enumerates `src/Infrastructure/*` from disk.
- `BoundaryTests.InfrastructureAssemblies` derives from `AssemblyAnchors.AllInfrastructure`.
- Exemptions: the local `exempt` array and `AssemblyAnchors.BoundaryExempt` (both just
  `Infrastructure.AI`). A boundary failure means fix the dependency, not add an exemption.

Adding an infra project: see CLAUDE.md for the anchor + `ProjectReference` requirement.

Test commands are in CLAUDE.md; `--filter-method "*Method"` narrows to a single method. The VSTest `--filter` syntax no longer applies.

## Patterns

```csharp
// Setup mocks
private readonly IMyRepo _repo = Substitute.For<IMyRepo>();
private readonly MyService _sut;

public MyServiceTests()
{
    _sut = new MyService(_repo);
}

// Mock IQueryable (for EF Core .Query() chains)
var mock = items.ToList().BuildMock();
_repo.Query().Returns(mock);

// Mock async repository methods
_repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(entity);
```
