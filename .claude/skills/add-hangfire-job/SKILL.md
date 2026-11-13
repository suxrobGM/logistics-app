---
name: add-hangfire-job
description: Add a recurring or on-demand Hangfire background job (nightly sync, reminder sweep, cleanup, quarter close). Use when work must run on a schedule or outside a request - e.g. "sync fuel card transactions nightly", "email drivers whose licence expires in 30 days". Covers tenant fan-out, per-tenant scopes and the feature gate [RequiresFeature] can't provide here.
---

# Add a Hangfire job

Jobs live in `src/Presentation/Logistics.API/Jobs/`. **They bypass the request pipeline**: no feature check, no validation, no tenant from a request. Every guard a handler gets for free you write yourself.

## Decide

- **No class needed** if the job is one call to a service that handles tenants itself: add a `RecurringJob.AddOrUpdate<IThingService>` line to `Jobs/Scheduling/RecurringJobs.cs`. Hangfire resolves it with no tenant set.
- **Recurring** jobs have a `static ScheduleJobs()`. **On-demand** jobs are enqueued by a runner (`AIDispatchTurnJob` + `HangfireAIDispatchTurnRunner`).
- **Per-tenant** (almost always): fan out with `TenantJobRunner.ForEachTenantAsync`. Global master-DB work uses one scope.

## Write it

Copy `FuelCardSyncJob.cs`:

```csharp
public class ThingSyncJob(ILogger<ThingSyncJob> logger, IServiceScopeFactory scopeFactory)
{
    public static void ScheduleJobs() =>
        RecurringJob.AddOrUpdate<ThingSyncJob>("thing-sync", job => job.SyncAllTenantsAsync(CancellationToken.None), Cron.Daily(2));

    [AutomaticRetry(Attempts = 2)]
    public Task SyncAllTenantsAsync(CancellationToken ct) =>
        TenantJobRunner.ForEachTenantAsync(scopeFactory, logger, "thing sync", SyncTenantAsync, ct);

    private async Task SyncTenantAsync(IServiceScope scope, Tenant tenant, CancellationToken ct)
    {
        var features = scope.ServiceProvider.GetRequiredService<IFeatureService>();
        if (!await features.IsFeatureEnabledAsync(tenant.Id, TenantFeature.Things))
        {
            return;
        }

        await scope.ServiceProvider.GetRequiredService<IThingSyncService>().SyncAsync(ct);
    }
}
```

- **Constructor takes only `ILogger<T>`, `IServiceScopeFactory`** (and `IHubContext<>` for SignalR). The job comes from the root container; anything scoped injected there lives forever. Resolve everything else from the scope.
- **Feature gate inside the body.** The runner doesn't check features on purpose (`IftaQuarterCloseJob` runs part of its work ungated).
- **Tenant:** the runner already sets it per scope. Only on-demand jobs given a tenant id call `await tenantUow.SetCurrentTenantByIdAsync(tenantId)` themselves, before any `Repository<T>()`; otherwise they silently read another tenant's rows.
- Keep the job as glue; the logic goes in an application service with a unit test. Don't add a per-tenant try/catch; the runner already isolates and logs each tenant.

## Register it

Add `ThingSyncJob.ScheduleJobs();` to `ScheduleJobs(this WebApplication app)` in `src/Presentation/Logistics.API/Setup.cs`. **Forget this and the job never runs, silently.**

- Renamed a recurring id? Add `RecurringJob.RemoveIfExists("old-id");` or the old entry keeps firing.
- Fan-out jobs run no more often than every 5 minutes (`EldSyncJob`'s `Cron.MinuteInterval(5)`).
