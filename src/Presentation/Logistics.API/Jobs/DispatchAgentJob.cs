using Hangfire;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.API.Jobs;

/// <summary>
///     Hangfire job that periodically runs the AI dispatch agent for all eligible tenants.
///     Runs every 15 minutes. Only runs for tenants with AgenticDispatch feature enabled.
/// </summary>
public class DispatchAgentJob(
    ILogger<DispatchAgentJob> logger,
    IServiceScopeFactory scopeFactory)
{
    public static void ScheduleJobs()
    {
        RecurringJob.AddOrUpdate<DispatchAgentJob>(
            "dispatch-agent",
            job => job.RunForAllTenantsAsync(CancellationToken.None),
            "*/15 * * * *"); // Every 15 minutes
    }

    [AutomaticRetry(Attempts = 1)]
    public async Task RunForAllTenantsAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var masterUow = scope.ServiceProvider.GetRequiredService<IMasterUnitOfWork>();

        var tenants = await masterUow.Repository<Tenant>()
            .GetListAsync(t => t.ConnectionString != null, ct);

        logger.LogInformation("Starting dispatch agent cycle for {TenantCount} tenants", tenants.Count);

        foreach (var tenant in tenants)
        {
            if (ct.IsCancellationRequested)
                break;

            try
            {
                await RunForTenantAsync(tenant, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Dispatch agent failed for tenant {TenantName}", tenant.Name);
            }
        }

        logger.LogInformation("Completed dispatch agent cycle");
    }

    private async Task RunForTenantAsync(Tenant tenant, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var featureService = scope.ServiceProvider.GetRequiredService<IFeatureService>();

        // Check if feature is enabled for this tenant
        if (!await featureService.IsFeatureEnabledAsync(tenant.Id, TenantFeature.AgenticDispatch))
        {
            return;
        }

        var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();
        tenantUow.SetCurrentTenant(tenant);

        // Skip if a session is already running
        var runningSession = await tenantUow.Repository<DispatchSession>()
            .GetAsync(s => s.Status == DispatchSessionStatus.Running, ct);

        if (runningSession is not null)
        {
            logger.LogDebug("Skipping tenant {TenantName} — session already running", tenant.Name);
            return;
        }

        // Skip if no unassigned loads
        var unassignedCount = await tenantUow.Repository<Load>()
            .CountAsync(l => l.Status == LoadStatus.Draft && l.TripStops.Count == 0, ct);

        if (unassignedCount == 0)
        {
            logger.LogDebug("Skipping tenant {TenantName} — no unassigned loads", tenant.Name);
            return;
        }

        // Check AI quota status
        var quotaService = scope.ServiceProvider.GetRequiredService<IAiQuotaService>();
        var quotaStatus = await quotaService.GetQuotaStatusAsync(tenant.Id, ct);

        logger.LogInformation(
            "Running dispatch agent for tenant {TenantName} ({UnassignedLoads} unassigned loads, quota: {Used}/{Quota})",
            tenant.Name, unassignedCount, quotaStatus.UsedThisWeek, quotaStatus.WeeklyQuota);

        var agentService = scope.ServiceProvider.GetRequiredService<IDispatchAgentService>();

        await agentService.RunAsync(new DispatchAgentRequest(
            TenantId: tenant.Id,
            Mode: DispatchAgentMode.Autonomous,
            TriggeredByUserId: null,
            IsOverage: quotaStatus.IsOverQuota), ct);
    }
}
