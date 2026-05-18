# Phase 4 — Per-module registrars + Scrutor scanning

> **Status: DONE on 2026-05-14.** Part of group `03-modular-reorganization/`. `IApplicationService` marker live; Scrutor scan registers 13 services; 6 stub module registrars in place; DI smoke test green (17/17 passing).

## Goal

Shrink the top-level [`Application/Registrar.cs`](../../../../src/Core/Logistics.Application/Registrar.cs) from a hand-listed catalog of services into a thin orchestrator that delegates to per-module registrars and an assembly-scanning convention.

## Why

Today `Registrar.cs` hand-registers every concrete service (`PayrollService`, `LoadService`, `DispatchEligibilityService`, etc.). Each new application service adds a line. At 50+ services this file becomes:

- A merge magnet
- The bottleneck for adding a feature
- An unmaintainable sprawl

Per-module registrars + Scrutor auto-registration push registration close to the code, and the top-level file becomes orchestration only.

## Prerequisites

- Phase 1 fully done.
- Phase 2 done (so the file already shrunk by ~2 services that moved out).
- Phase 3 done.
- Can run before or after Phase 5/6 — orthogonal.

## Design

### Marker interfaces (in `Abstractions/Common/`)

```csharp
namespace Logistics.Application.Abstractions.Common;

/// <summary>Marker for application workflow services (Scoped lifetime by default).</summary>
public interface IApplicationService;
```

Make every concrete application workflow service (`LoadService`, `PayrollService`, etc.) implement this marker through its primary interface, e.g.:

```csharp
public interface ILoadService : IApplicationService { ... }
public interface IPayrollService : IApplicationService { ... }
```

### Scrutor registration

```csharp
public static IServiceCollection AddApplicationServices(this IServiceCollection services)
{
    services.Scan(scan => scan
        .FromAssemblyOf<Registrar>()
        .AddClasses(c => c.AssignableTo<IApplicationService>(), publicOnly: false)
        .AsImplementedInterfaces()
        .WithScopedLifetime());
    return services;
}
```

### Module registrars

Each module owns a partial registrar in its folder (created in Phase 7's reorg, stubbed today):

```csharp
// src/Core/Logistics.Application/Modules/Operations/Registrar.cs (or any agreed location)
namespace Logistics.Application.Modules.Operations;

public static class OperationsModuleRegistrar
{
    public static IServiceCollection AddOperationsModule(this IServiceCollection services)
    {
        // Hand-wire anything that can't be auto-registered (decorators, named instances, etc.)
        return services;
    }
}
```

For Phase 4 (before Phase 7 reorg), these can be no-op stubs that just exist to claim the namespace. They get bodies in Phase 7.

### New `Registrar.cs` shape

```csharp
public static class Registrar
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddApplicationCommon();
        services.AddApplicationServices();  // Scrutor scan of IApplicationService

        services.AddOperationsModule();
        services.AddComplianceModule();
        services.AddFinancialModule();
        services.AddIdentityAccessModule();
        services.AddIntegrationsModule();
        services.AddPlatformModule();

        return services;
    }

    private static IServiceCollection AddApplicationCommon(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Registrar).Assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(FeatureCheckBehaviour<,>));
            // TransactionBehaviour added in Phase 6
        });
        return services;
    }
}
```

## Package addition

Add Scrutor to `src/Core/Logistics.Application/Logistics.Application.csproj`:

```xml
<PackageReference Include="Scrutor" Version="6.1.0" />
```

(Match the latest stable at execution time.)

## Step-by-step

1. **Add `IApplicationService` marker** in `Abstractions/Common/IApplicationService.cs`. Commit.
2. **Make existing service interfaces extend `IApplicationService`**:
   - `ILoadService`, `IPayrollService`, `IUserService`, `IDispatchEligibilityService`, `IInvoiceTaxApplier`, the various privacy/reminder service interfaces still in Application.
   - One file per interface, no behavior change. Commit.
3. **Add Scrutor**:
   - Package reference.
   - `AddApplicationServices()` helper that scans by marker.
   - Commit.
4. **Stub module registrars** (no-op for now):
   - `Modules/Operations/Registrar.cs`, `Compliance`, `Financial`, `IdentityAccess`, `Integrations`, `Platform`.
   - Each has `Add{Module}Module()` returning `services` unchanged.
   - Commit (or include in the Phase 4 final commit).
5. **Rewrite top-level `Registrar.cs`** to the new shape above. Delete the hand-listed `services.AddScoped<I*, *>()` lines that Scrutor now covers.
6. **Build + run a DI smoke test**:
   - Either a quick `WebApplicationFactory` integration test that resolves each known service, or:
   - Manually run the API and confirm startup.
7. Commit.

## Critical files

- `src/Core/Logistics.Application.Abstractions/Common/IApplicationService.cs` (new)
- Every `Application/Services/I*.cs` for services that stay in Application — small `: IApplicationService` addition
- `src/Core/Logistics.Application/Registrar.cs` — major rewrite
- `src/Core/Logistics.Application/Modules/{Operations,Compliance,Financial,IdentityAccess,Integrations,Platform}/Registrar.cs` — new stubs
- `src/Core/Logistics.Application/Logistics.Application.csproj` — add Scrutor

## Verification

- `dotnet build Logistics.slnx` → 0 errors.
- API startup smoke test: every previously-registered service still resolves.
- Specifically resolve: `ILoadService`, `IPayrollService`, `IUserService`, `IDispatchEligibilityService`, `IEligibilityCheck` (from Phase 2), all the Privacy services. Each `GetRequiredService<X>()` returns a non-null instance of the expected concrete type.
- A subtle one: assert the **lifetime** is Scoped (not Singleton). Singleton-ifying a tenant-aware service silently breaks tenant isolation.

## Risks

| Risk                                                                   | Mitigation                                                                                                                                                                                               |
| ---------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Scrutor registers more than expected (e.g., picks up internal helpers) | Use `c.AssignableTo<IApplicationService>()`, NOT broad `WithDefaultLifetime`. The marker is the gate.                                                                                                    |
| Scrutor missed something the hand-list had                             | Run a DI startup test that resolves every previously-explicit registration.                                                                                                                              |
| `IApplicationService` becomes a god-marker (anyone adds it)            | Keep it for services with implementations in Application only. If a port has its impl in Infrastructure, it does NOT extend this marker — those are registered in `Infrastructure.*/Registrar.cs` files. |
| Lifetime accidentally switched (Scoped vs Singleton)                   | Verify in startup test.                                                                                                                                                                                  |

## Acceptance

- [ ] `IApplicationService` marker exists.
- [ ] Every in-Application service interface extends it.
- [ ] Scrutor registers them.
- [ ] Top-level `Registrar.cs` is ~30 lines or fewer, delegating to module registrars + Scrutor.
- [ ] DI startup smoke test resolves every formerly-explicit service.
- [ ] Module registrar stubs exist for the 6 modules.
- [ ] Build green, tests pass.
