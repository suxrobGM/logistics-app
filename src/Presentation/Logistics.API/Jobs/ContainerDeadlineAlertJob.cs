using Hangfire;
using Logistics.Application.Abstractions.Features;
using Logistics.Application.Modules.Operations.Containers.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.API.Jobs;

/// <summary>
/// Daily sweep that warns dispatchers about containers whose last free day or empty-return
/// deadline is today, tomorrow, or already passed.
/// </summary>
public class ContainerDeadlineAlertJob(
    ILogger<ContainerDeadlineAlertJob> logger,
    IServiceScopeFactory scopeFactory)
{
    /// <summary>
    /// 12:00 UTC - early morning across US time zones, before the day's port moves are planned.
    /// </summary>
    public static void ScheduleJobs()
    {
        RecurringJob.AddOrUpdate<ContainerDeadlineAlertJob>(
            "container-deadline-alerts",
            job => job.SendAllTenantsAsync(CancellationToken.None),
            Cron.Daily(12));
    }

    [AutomaticRetry(Attempts = 2)]
    public Task SendAllTenantsAsync(CancellationToken ct) =>
        TenantJobRunner.ForEachTenantAsync(scopeFactory, logger, "container deadline alerts", SendTenantAsync, ct);

    private async Task SendTenantAsync(IServiceScope scope, Tenant tenant, CancellationToken ct)
    {
        // Jobs bypass the request pipeline, so the [RequiresFeature] gate does not apply here -
        // without this a downgraded tenant keeps getting demurrage alerts.
        var featureService = scope.ServiceProvider.GetRequiredService<IFeatureService>();
        if (!await featureService.IsFeatureEnabledAsync(tenant.Id, TenantFeature.IntermodalContainers))
        {
            return;
        }

        var alertService = scope.ServiceProvider.GetRequiredService<IContainerDeadlineAlertService>();
        var sent = await alertService.SendAlertsAsync(DateTime.UtcNow, ct);

        if (sent > 0)
        {
            logger.LogInformation(
                "Sent {Count} container deadline alert(s) for tenant {TenantName}", sent, tenant.Name);
        }
    }
}
