---
name: add-hangfire-job
description: Add a recurring or on-demand Hangfire background job (nightly sync, reminder sweep, cleanup, quarter close). Use when work must run on a schedule or outside a request - e.g. "sync fuel card transactions nightly", "email drivers whose licence expires in 30 days". Codifies the tenant fan-out, the per-tenant DI scope, and the feature gate that [RequiresFeature] cannot provide here.
---

# Add a Hangfire job

Jobs live in `src/Presentation/Logistics.API/Jobs/`. There is no base class - a job is a plain class
with a primary constructor and a `static ScheduleJobs()`.

**The thing that makes jobs different from everything else in this repo: they do not go through the
MediatR pipeline.** No `FeatureCheckBehaviour`, no validation behaviour, no `ICurrentTenantAccessor`
populated from a request. Every guard you get for free in a handler you must write yourself here.

## Decide first

1. **Recurring or on-demand?** Recurring → `RecurringJob.AddOrUpdate` in `ScheduleJobs()`. On-demand →
   no `ScheduleJobs()`; something enqueues it (see `AIDispatchSessionJob` + `HangfireAIDispatchRunner`,
   `CommandEnqueuerJob` + `HangfireCommandEnqueuer`).
2. **Per-tenant or global?** Almost always per-tenant → fan out with `TenantJobRunner.ForEachTenantAsync`.
   Global (master DB only, e.g. `WebhookEventCleanupJob`) → one scope, no fan-out.
3. **Is the work behind a tenant feature flag?** If yes, you must check it explicitly.

## 1. The job class

`src/Presentation/Logistics.API/Jobs/{Name}Job.cs`. Copy the shape from `FuelCardSyncJob.cs`:

```csharp
public class ThingSyncJob(
    ILogger<ThingSyncJob> logger,
    IServiceScopeFactory scopeFactory)
{
    public static void ScheduleJobs()
    {
        RecurringJob.AddOrUpdate<ThingSyncJob>(
            "thing-sync",                                   // kebab-case id
            job => job.SyncAllTenantsAsync(CancellationToken.None),
            Cron.Daily(2));
    }

    [AutomaticRetry(Attempts = 2)]
    public Task SyncAllTenantsAsync(CancellationToken ct) =>
        TenantJobRunner.ForEachTenantAsync(scopeFactory, logger, "thing sync", SyncTenantAsync, ct);

    private async Task SyncTenantAsync(IServiceScope scope, Tenant tenant, CancellationToken ct)
    {
        // ...
    }
}
```

**Constructor injection is limited to `ILogger<T>` and `IServiceScopeFactory`** (plus `IHubContext<>`
where the job pushes SignalR updates). The job instance is resolved from the **root** container, so
anything scoped that you inject here is captured for the lifetime of the process. Everything else is
pulled from the per-tenant scope inside the body.

## 2. Register it

One line in the `ScheduleJobs(this WebApplication app)` extension in
`src/Presentation/Logistics.API/Setup.cs` (~line 233):

```csharp
ThingSyncJob.ScheduleJobs();
```

If you **rename** an existing recurring id, the old entry stays orphaned in Hangfire storage and keeps
firing. Add a cleanup line next to the others:

```csharp
RecurringJob.RemoveIfExists("old-id");
```

## 3. Feature gate (if the work is flag-gated)

`TenantJobRunner.ForEachTenantAsync` deliberately does **not** check features - its XML doc explains
why: `IftaQuarterCloseJob` needs its breadcrumb purge to run for every tenant while the quarter
snapshot stays IFTA-gated. So the check goes in **your body**, first thing:

```csharp
// Jobs bypass the MediatR pipeline, so the [RequiresFeature] gate on the commands does not
// apply here - check explicitly, or a downgraded tenant keeps having expenses written.
var featureService = scope.ServiceProvider.GetRequiredService<IFeatureService>();
if (!await featureService.IsFeatureEnabledAsync(tenant.Id, TenantFeature.FuelCards))
{
    return;
}
```

Only 3 of the current 16 recurring jobs do this. If your job writes anything a downgraded tenant
shouldn't get, it needs to be the 4th.

## 4. Tenant context

```csharp
var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();
tenantUow.SetCurrentTenant(tenant);        // BEFORE any Repository<T>() call
```

Calling `Repository<T>()` before `SetCurrentTenant` reads whatever tenant the scope last resolved.
It does not throw. It returns the wrong tenant's rows.

## 5. Keep the body thin

Put the real work in an application service (`Logistics.Application/Modules/{Module}/{Feature}/Services/`)
and have the job orchestrate: resolve → gate → set tenant → call service → log. That service is what
gets unit-tested; the job itself is glue.

`TenantJobRunner` already wraps each tenant in its own try/catch and logs, so **don't** add a
per-tenant try/catch that swallows - you'd hide the failure from the cycle log.

## Checklist

- [ ] `Jobs/{Name}Job.cs` created; only `ILogger<T>` / `IServiceScopeFactory` (/ `IHubContext<>`) in the constructor
- [ ] `[AutomaticRetry(Attempts = 2)]` on the entry method
- [ ] `static ScheduleJobs()` with a kebab-case recurring id and a `Cron.*` schedule
- [ ] Called from `Setup.cs` `ScheduleJobs(this WebApplication app)`
- [ ] Renamed an id? `RecurringJob.RemoveIfExists("old-id")` added
- [ ] Per-tenant work fans out via `TenantJobRunner.ForEachTenantAsync`
- [ ] Feature-gated work checks `IFeatureService` **inside the body**
- [ ] `tenantUow.SetCurrentTenant(tenant)` precedes every `Repository<T>()` call
- [ ] All services resolved from the per-tenant `IServiceScope`, none from the constructor
- [ ] Real logic lives in an application service and has a unit test

Two failures the checklist won't catch: **forgetting the `Setup.cs` line** compiles and simply never
runs, with nothing reporting it; and a **`Cron.Minutely()` job doing tenant fan-out** stacks a scope
per tenant per minute - `EldSyncJob`'s `Cron.MinuteInterval(5)` is the floor.

## Related

- `.claude/rules/backend/testing.md` - test the service, not the job
- `add-tenant-feature-flag` - if you're introducing the flag as well as the job
- `feature-map.md` → the feature's row, which should list the job under **Jobs**
