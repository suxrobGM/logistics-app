# Phase 8 — Read-side projection services

## Goal

Replace the top ~10 heaviest query handlers with thin shims that delegate to dedicated read services. The read services live in `Infrastructure.Persistence/Reads/` and bypass the generic repository — direct `DbContext` access, compiled queries, projection to DTO.

## Why

Audit findings (from `okay-let-s-build-a-rippling-hollerith.md` master plan):

- 77 `.Query()` usages across 52 files in the Application project
- [`SafetyReportHandler.cs`](../../../../src/Core/Logistics.Application/Queries/Reports/Safety/SafetyReportHandler.cs) is 311 lines
- Lazy loading + generic repository is fine for most query handlers, but heavy report/stats handlers do multi-aggregate joins and projection in memory after a lazy-loaded fetch — N+1 at scale.

The right shape for those:

- Direct `TenantDbContext` access
- Compiled queries (`EF.CompileAsyncQuery`) for hot paths
- Project to DTO directly in SQL (`Select(x => new XDto { ... })`)
- Skip the aggregate-load + lazy-load chain entirely

This is opportunistic by nature — pick handlers based on real production p95 latency, not gut feel. Phase 8 is the _pattern_; specific migrations should be driven by telemetry.

## Assembly-visibility rule (Codex caught this — don't get it wrong)

The reader **interface** must be **public** and live in `Application.Abstractions`. The **implementation** lives in `Infrastructure.Persistence/Reads/`.

A common mistake: define `internal interface ISafetyReportReader` in Application and implement it in Infrastructure. **This will not compile** — Infrastructure cannot implement an internal Application interface. The previous draft of the master plan said "internal to the module if possible," which is wrong. Always public, always in Abstractions.

## Prerequisites

- Phase 1, 2, 3 done (clean layering).
- Phase 7 helpful — module structure makes the natural homes for reader interfaces obvious.
- Telemetry: if you don't have p95 latency per query handler, install/enable it first. Picking handlers blind defeats the purpose.

## Pattern

### Interface

```csharp
// src/Core/Logistics.Application.Abstractions/Modules/Compliance/ReadModels/ISafetyReportReader.cs
namespace Logistics.Application.Abstractions.Modules.Compliance.ReadModels;

public interface ISafetyReportReader
{
    Task<SafetyReportDto> GetAsync(SafetyReportQuery query, CancellationToken ct);
}

public sealed record SafetyReportQuery(Guid TenantId, DateOnly From, DateOnly To, /* ... */);
public sealed record SafetyReportDto(/* ... */);
```

### Implementation

```csharp
// src/Infrastructure/Logistics.Infrastructure.Persistence/Reads/Compliance/SafetyReportReader.cs
internal sealed class SafetyReportReader(TenantDbContext db) : ISafetyReportReader
{
    private static readonly Func<TenantDbContext, Guid, DateOnly, DateOnly, IAsyncEnumerable<DvirRow>>
        _dvirsByPeriod = EF.CompileAsyncQuery(
            (TenantDbContext ctx, Guid tenantId, DateOnly from, DateOnly to) =>
                ctx.Dvirs
                    .Where(d => d.TenantId == tenantId && d.OccurredAt >= from.ToDateTime(default) && d.OccurredAt < to.ToDateTime(default))
                    .Select(d => new DvirRow(d.Id, d.Status, d.OccurredAt))
        );

    public async Task<SafetyReportDto> GetAsync(SafetyReportQuery q, CancellationToken ct)
    {
        // Build a small set of focused queries, projected to DTOs, joined in-memory at the end.
        var dvirs = new List<DvirRow>();
        await foreach (var row in _dvirsByPeriod(db, q.TenantId, q.From, q.To).WithCancellation(ct))
            dvirs.Add(row);
        // ... combine into SafetyReportDto ...
        return new SafetyReportDto(/* ... */);
    }

    private sealed record DvirRow(Guid Id, DvirStatus Status, DateTime OccurredAt);
}
```

### Registration

In `Infrastructure.Persistence/Registrar.cs`:

```csharp
services.AddScoped<ISafetyReportReader, SafetyReportReader>();
```

### Handler becomes thin

Before (311 lines):

```csharp
public class SafetyReportHandler : IRequestHandler<SafetyReportQuery, Result<SafetyReportDto>>
{
    // ... 311 lines of repository queries, in-memory joins, mapping ...
}
```

After (~30 lines):

```csharp
internal sealed class SafetyReportHandler(ISafetyReportReader reader, IValidator<SafetyReportQuery> validator)
    : IAppRequestHandler<SafetyReportQuery, Result<SafetyReportDto>>
{
    public async Task<Result<SafetyReportDto>> Handle(SafetyReportQuery req, CancellationToken ct)
    {
        var dto = await reader.GetAsync(new(req.TenantId, req.From, req.To), ct);
        return Result<SafetyReportDto>.Ok(dto);
    }
}
```

Validation runs in the pipeline (not duplicated here). `[RequiresFeature]` still applies.

## Candidate handlers (start with these — re-rank per real telemetry)

| Handler                               | File                                                                                            | Lines  | Why suspect                              |
| ------------------------------------- | ----------------------------------------------------------------------------------------------- | ------ | ---------------------------------------- |
| `SafetyReportHandler`                 | `src/Core/Logistics.Application/Queries/Reports/Safety/SafetyReportHandler.cs`                  | 311    | Multi-aggregate joins, dashboard surface |
| Payroll reports                       | `src/Core/Logistics.Application/Queries/Reports/Payroll/*Handler.cs`                            | varies | Time-series + multi-employee aggregation |
| Dashboard stats                       | `src/Core/Logistics.Application/Queries/Stats/*Handler.cs`                                      | varies | Hit on every dashboard load              |
| Load search / list (paged + filtered) | `src/Core/Logistics.Application/Queries/Load/GetLoads*/Handler.cs`                              | varies | Tenant front-page; high traffic          |
| Tracking page load                    | `src/Core/Logistics.Application/Queries/Tracking/GetPublicTracking/GetPublicTrackingHandler.cs` | varies | Public unauthenticated; perf-critical    |

Re-rank based on actual production p95.

## Step-by-step (per chosen handler)

1. **Measure first.** Capture current p95 and query count via SQL profiler or EF logging.
2. **Define the reader interface + DTO** in `Abstractions/Modules/{Module}/ReadModels/`.
3. **Implement the reader** in `Infrastructure.Persistence/Reads/{Module}/`.
4. **Use compiled queries** for the hot path. Add explicit `.Select(...)` projections so EF doesn't load full entities.
5. **Add a snapshot test** that asserts the new handler returns DTOs _equivalent_ to the old one (compare field-by-field with a known fixture).
6. **Switch the handler** to the thin shim.
7. **Re-measure.** Confirm the new p95 + query count is at least as good as before.
8. **Commit.**

One handler per PR. Don't batch.

## Optional Phase-5 arch rule per migrated handler

After a handler is migrated to a reader, an arch test can enforce that other handlers in the same module use the reader instead of `Repository<T>().Query()` for the same data. This is optional and per-handler — only worth it where misuse is plausible.

## Critical files (per migration)

- New: `src/Core/Logistics.Application.Abstractions/Modules/{Module}/ReadModels/I{X}Reader.cs`
- New: `src/Infrastructure/Logistics.Infrastructure.Persistence/Reads/{Module}/{X}Reader.cs`
- Updated: the original handler — shrunk to a shim
- Updated: `src/Infrastructure/Logistics.Infrastructure.Persistence/Registrar.cs` — register the reader
- Updated: test fixtures asserting DTO equivalence

## Verification

- Snapshot test: old vs new DTO is field-for-field equal for a known fixture.
- EF logging in CI: query count for the migrated handler dropped vs baseline (or at least didn't increase).
- p95 production latency improved (measure after deploy).
- `dotnet build Logistics.slnx` + `dotnet test` green.

## Risks

| Risk                                                                                             | Mitigation                                                                                                                   |
| ------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------- |
| Reader misses a field the old handler returned → silent regression                               | Snapshot test field-for-field.                                                                                               |
| Reader introduces a new N+1 because of a sneaky lazy-load on a Domain entity referenced by a DTO | Use scalar projections (`Select(x => x.Id)`, not `Select(x => new XDto { Inner = x.Inner })`) where `Inner` is a navigation. |
| Reader becomes a god-class (one reader per module ends up with 20 methods)                       | Split per query, not per module. `ISafetyReportReader` + `ISafetyIncidentReader` is fine.                                    |
| Compiled query closure captures something tenant-scoped at startup                               | Pass tenant ID as a parameter — never close over `ITenantUnitOfWork.CurrentTenantId`.                                        |

## Acceptance (per handler migration)

- [ ] Reader interface lives in `Abstractions/Modules/{X}/ReadModels/`, public.
- [ ] Reader implementation in `Infrastructure.Persistence/Reads/{X}/`, internal sealed.
- [ ] Handler shrunk to a shim (≤ 50 lines).
- [ ] Snapshot test compares old vs new DTOs and passes.
- [ ] EF logging confirms query count not increased.
- [ ] Reader registered in `Persistence/Registrar.cs`.
- [ ] One commit per migrated handler.
