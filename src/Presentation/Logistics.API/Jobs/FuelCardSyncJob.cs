using Hangfire;
using Logistics.Application.Abstractions.Features;
using Logistics.Application.Abstractions.Realtime;
using Logistics.Application.Modules.Integrations.FuelCards.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.API.Jobs;

/// <summary>
///     Hangfire job that pulls fuel card transactions nightly for all tenants with an
///     active provider configuration, materializing matched transactions as fuel expenses.
/// </summary>
public class FuelCardSyncJob(
    ILogger<FuelCardSyncJob> logger,
    IServiceScopeFactory scopeFactory)
{
    /// <summary>
    ///     Schedule the fuel card sync job to run nightly at 02:00 UTC.
    /// </summary>
    public static void ScheduleJobs()
    {
        RecurringJob.AddOrUpdate<FuelCardSyncJob>(
            "fuelcard-sync",
            job => job.SyncAllTenantsAsync(CancellationToken.None),
            Cron.Daily(2));
    }

    [AutomaticRetry(Attempts = 2)]
    public Task SyncAllTenantsAsync(CancellationToken ct) =>
        TenantJobRunner.ForEachTenantAsync(scopeFactory, logger, "fuel card sync", SyncTenantAsync, ct);

    private async Task SyncTenantAsync(IServiceScope scope, Tenant tenant, CancellationToken ct)
    {
        // Jobs bypass the MediatR pipeline, so the [RequiresFeature] gate on the commands does not
        // apply here — check explicitly, or a downgraded tenant keeps having expenses written.
        var featureService = scope.ServiceProvider.GetRequiredService<IFeatureService>();
        if (!await featureService.IsFeatureEnabledAsync(tenant.Id, TenantFeature.FuelCards))
        {
            return;
        }

        var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();

        tenantUow.SetCurrentTenant(tenant);

        var hasActiveConfig = await tenantUow.Repository<FuelCardProviderConfiguration>()
            .GetAsync(c => c.IsActive, ct) is not null;

        if (!hasActiveConfig)
        {
            return;
        }

        var syncService = scope.ServiceProvider.GetRequiredService<IFuelCardSyncService>();
        var result = await syncService.SyncCurrentTenantAsync(ct: ct);

        logger.LogInformation(
            "Fuel card sync for tenant {TenantName}: {Imported} imported ({Matched} matched, {Pending} pending)",
            tenant.Name, result.Imported, result.Matched, result.Pending);

        if (result.Pending > 0)
        {
            var notificationService = scope.ServiceProvider.GetRequiredService<IRealtimeNotificationService>();
            await notificationService.BroadcastNotificationAsync(tenant.Id.ToString(), new NotificationDto
            {
                Title = "Fuel card transactions need review",
                Message = $"{result.Pending} fuel card transaction(s) could not be matched to a truck. Review them in Fuel Cards.",
                CreatedDate = DateTime.UtcNow
            });
        }
    }
}
