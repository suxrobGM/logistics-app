# Layering

LogisticsX follows a Clean / Onion architecture with four conceptual layers. Dependency arrows only point inward — outer layers know about inner layers, never the reverse.

```text
Presentation  →  Application                   →  Domain
                 ↘ Application.Abstractions  ↗
Infrastructure  →  Application.Abstractions  →  Domain
```

| Layer                       | Project                              | Knows about                    | Examples                                                    |
| --------------------------- | ------------------------------------ | ------------------------------ | ----------------------------------------------------------- |
| Domain                      | `Logistics.Domain` + `.Primitives`   | Nothing else                   | `Load`, `Trip`, `Address` VO, `LoadCompletedEvent`          |
| Abstractions                | `Logistics.Application.Abstractions` | Domain                         | `IEmailService`, `IStorageService`, `ICommand<T>` markers   |
| Application                 | `Logistics.Application`              | Domain, Abstractions           | `Modules/Operations/Loads/Commands/...`, `ILoadService`     |
| Infrastructure (N projects) | `Logistics.Infrastructure.*`         | Domain, Abstractions           | `StripePaymentService`, `MapboxGeocoder`, `TenantDbContext` |
| Presentation                | `Logistics.API`, `McpServer`, …      | Application + composition root | Controllers, hubs, jobs, `Program.cs`                       |

Presentation is the composition root: it references both Application and every Infrastructure project so it can wire ports to adapters in `Program.cs`. Nothing else is allowed to reference Infrastructure.

## Ports vs. workflow services — the load-bearing rule

There are two kinds of "service" in this codebase and they live in different places. Getting this wrong is the most common cause of layer leaks.

### Infrastructure ports → `Application.Abstractions`

These are interfaces **implemented by Infrastructure**. They abstract external systems (Stripe, Mapbox, the database, a SignalR hub). Application code calls them through the interface; the implementation is wired at the composition root.

Examples — verified locations under `src/Core/Logistics.Application.Abstractions/`:

- `Storage/IBlobStorageService.cs` — implemented by `Infrastructure.Storage`
- `Geocoding/IGeocodingService.cs` — implemented by `Infrastructure.Routing`
- `AiDispatch/ILlmProvider.cs` — implemented by `Infrastructure.AI`
- `Dispatch/IDispatchEligibilityService.cs` — pure-domain check, currently here as a port
- `Notifications/INotificationService.cs` — implemented by `Infrastructure.Communications`
- `Payments/IStripePaymentService.cs` — implemented by `Infrastructure.Payments`

If Infrastructure implements it, the interface goes in Abstractions.

### Application workflow services → `Application`

These are interfaces **implemented inside Application itself**. They orchestrate domain logic and ports — no external system, no EF Core, no HTTP. They exist for testability and for sharing logic across handlers. Each lives beside its feature under `src/Core/Logistics.Application/Modules/{Module}/{Feature}/Services/`:

- `Modules/Operations/Loads/Services/ILoadService.cs` — load-lifecycle orchestration
- `Modules/Financial/Payroll/Services/PayrollService.cs` — payroll computation
- `Modules/Compliance/Eld/Services/RuleSetSelector.cs` — region → HOS ruleset

If Application implements it, the interface stays in Application.

Trivial CRUD handlers don't get bespoke classes: `Delete`/`GetById`/`Update` slices subclass the shared generic base handlers in `src/Core/Logistics.Application/Handlers/` (`DeleteTenantEntityHandler`, `DeleteMasterEntityHandler`, `GetTenantEntityByIdHandler`, `UpdateTenantEntityHandler`).

### Quick decision

> "Does Infrastructure implement this?" — **Yes** → Abstractions. **No** → Application.

## One example per layer

```csharp
// Domain
public class Load : AggregateRoot
{
    public void Complete() { Status = LoadStatus.Completed; AddDomainEvent(new LoadCompletedEvent(this)); }
}

// Abstraction (port)
namespace Logistics.Application.Abstractions.Storage;
public interface IBlobStorageService
{
    Task<string> UploadAsync(Stream content, string key, CancellationToken ct);
}

// Application (handler + workflow service)
namespace Logistics.Application.Modules.Operations.Loads.Commands.CompleteLoad;
internal sealed class CompleteLoadHandler(ITenantUnitOfWork uow, ILoadService loads) : IRequestHandler<CompleteLoadCommand, Result>
{
    public async Task<Result> Handle(CompleteLoadCommand cmd, CancellationToken ct) { /* … */ }
}

// Infrastructure (adapter)
namespace Logistics.Infrastructure.Storage.Providers.R2;
internal sealed class R2BlobStorageService(IAmazonS3 s3) : IBlobStorageService { /* … */ }

// Presentation (composition root)
builder.Services.AddStorageInfrastructure(builder.Configuration);
```

## Enforcement

Layering rules are not aspirational — they're verified on every build by [`test/Logistics.Architecture.Tests/`](../../test/Logistics.Architecture.Tests/) (ArchUnitNET over compiled assemblies). The matrix:

- `Application` ↛ Infrastructure, `Application` ↛ `Microsoft.AspNetCore.Http`
- `Abstractions` ↛ `Application`, `Abstractions` ↛ Infrastructure, `Abstractions` ↛ EF Core, `Abstractions` ↛ AspNetCore.Http
- Each Infrastructure assembly → Abstractions, not Application
- No handler injects `IHttpContextAccessor`

`Integrations.Common` is the one deliberate exception to the "each Infrastructure assembly → Abstractions" line: it is a leaf helper project holding shared `HttpClient` JSON helpers and references no other project in the repo, Abstractions included. The rule it must satisfy is the same one everything else does — no dependency on `Application`.

Both infrastructure rules discover what they cover rather than hand-listing it, so a new project is picked up off disk automatically; the IL-level boundary rule additionally needs an anchor in `AssemblyAnchors.AllInfrastructure` and a `ProjectReference` in the arch-tests csproj. CLAUDE.md holds the canonical version of this rule.

Exemptions exist and are named explicitly — the local `exempt` array in `CsprojReferenceTests.cs` and `AssemblyAnchors.BoundaryExempt`, both currently just `Logistics.Infrastructure.AI`, which stays there until slice 1.9-AI decouples it. Adding your project to either array is the wrong reflex: a boundary failure means the dependency is wrong, not the rule.
