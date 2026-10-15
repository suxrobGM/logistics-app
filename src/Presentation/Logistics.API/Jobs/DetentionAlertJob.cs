using Hangfire;
using Logistics.Application.Modules.Operations.Trips.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.API.Jobs;

/// <summary>
/// Warns dispatchers when a truck has been at a stop longer than the free time, while there is
/// still time to call the shipper. Each stop alerts once.
/// </summary>
public class DetentionAlertJob(
    ILogger<DetentionAlertJob> logger,
    IServiceScopeFactory scopeFactory)
{
    public static void ScheduleJobs()
    {
        RecurringJob.AddOrUpdate<DetentionAlertJob>(
            "detention-alerts",
            job => job.SendAllTenantsAsync(CancellationToken.None),
            Cron.MinuteInterval(15));
    }

    [AutomaticRetry(Attempts = 2)]
    public Task SendAllTenantsAsync(CancellationToken ct) =>
        TenantJobRunner.ForEachTenantAsync(scopeFactory, logger, "detention alerts", SendTenantAsync, ct);

    private async Task SendTenantAsync(IServiceScope scope, Tenant tenant, CancellationToken ct)
    {
        var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();
        tenantUow.SetCurrentTenant(tenant);

        var alertService = scope.ServiceProvider.GetRequiredService<IDetentionAlertService>();
        var sent = await alertService.SendAlertsAsync(DateTime.UtcNow, ct);

        if (sent > 0)
        {
            logger.LogInformation("Sent {Count} detention alert(s) for tenant {TenantName}", sent, tenant.Name);
        }
    }
}
