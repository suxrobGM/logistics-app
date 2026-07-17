using Hangfire;
using Logistics.Application.Abstractions.Realtime;
using Logistics.Application.Modules.Integrations.FuelCards.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
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
    public async Task SyncAllTenantsAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var masterUow = scope.ServiceProvider.GetRequiredService<IMasterUnitOfWork>();

        var tenants = await masterUow.Repository<Tenant>().GetListAsync(t => t.ConnectionString != null);

        logger.LogInformation("Starting fuel card sync for {TenantCount} tenants", tenants.Count);

        foreach (var tenant in tenants)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            try
            {
                await SyncTenantAsync(tenant, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error syncing fuel card data for tenant {TenantName}", tenant.Name);
            }
        }

        logger.LogInformation("Completed fuel card sync cycle");
    }

    private async Task SyncTenantAsync(Tenant tenant, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
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
