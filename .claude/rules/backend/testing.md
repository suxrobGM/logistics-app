---
paths:
  - "test/**/*.cs"
---

# Testing

xUnit v3 on Microsoft.Testing.Platform (test projects are `Exe`), NSubstitute, `MockQueryable.NSubstitute` for `IQueryable`, `coverlet.MTP`. Run with `dotnet test --filter-class "*X"` or `--filter-method "*X"`; VSTest `--filter` no longer works.

## Projects

| Project | Covers |
| --- | --- |
| `Logistics.Application.Tests` | Handlers and services. Folders mirror `Modules/` (`Operations/Loads/`), plus `Common/` and `Primitives/` |
| `Logistics.Infrastructure.AI.Tests` | Agents, tools, prompts, pricing |
| `Logistics.Infrastructure.Tests` | Every other infrastructure project and HostDefaults, one folder per project (`Persistence/`, `Integrations/`, `Payments/`, ...) |
| `Logistics.Architecture.Tests` | Layer boundaries (ArchUnitNET) |
| `Logistics.Mediator.Tests` | The in-repo mediator |

Namespaces follow folders. Infrastructure code is never tested in `Application.Tests`.

## Test kit

Use it instead of hand-built fixtures.

- `Application.Tests/TestKit/`: `TestData.Load/Truck/Driver/Address(...)` (valid entities; parameters are only what tests vary), `TestData.TripLoadCommand()`, `TestTenant.Create(...)`, `uow.Stub<T>(rows)` (wires `Repository<T>()` reads to `rows`; pass a `List` to see later adds), `uow.StubLoadLookups()`, `AgentTestContext`. These files are linked into the AI tests project.
- `Infrastructure.Tests/TestKit/StubHttpMessageHandler`: one-response HTTP fake (`Json`, `Throws`, recorded `Requests`).

## Conventions

- `{ClassUnderTest}Tests`; methods `{Method}_{Scenario}_{Expected}`; `_sut`; mocks wired in the constructor; `#region` to group.
- A `decimal` theory uses `[MemberData]` over `TheoryData` with `m` literals, never `[InlineData(8.10)]`: the double converts with binary noise since .NET 11.

## Architecture tests

Projects are discovered from disk (`CsprojReferenceTests.InfrastructureProjects`) and from `AssemblyAnchors.AllInfrastructure` (`BoundaryTests`). The only exemption is `Infrastructure.AI`. A boundary failure means fix the dependency, never add an exemption.
