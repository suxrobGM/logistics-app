using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class SyncAllDriversHosStatusHandler(
    ITenantUnitOfWork tenantUow,
    IEldProviderFactory eldProviderFactory,
    ILogger<SyncAllDriversHosStatusHandler> logger)
    : IAppRequestHandler<SyncAllDriversHosStatusCommand, Result>
{
    public async Task<Result> Handle(SyncAllDriversHosStatusCommand req, CancellationToken ct)
    {
        // Get all active provider configurations
        var configs = await tenantUow.Repository<EldProviderConfiguration>()
            .GetListAsync(c => c.IsActive, ct);

        if (!configs.Any())
        {
            return Result.Fail("No active ELD providers configured");
        }

        var syncCount = 0;
        var errorCount = 0;

        foreach (var config in configs)
        {
            try
            {
                var providerService = eldProviderFactory.GetProvider(config);

                // Get all HOS data from provider
                var hosDataList = await providerService.GetAllDriversHosStatusAsync();

                // Get all driver mappings for this provider
                var mappings = await tenantUow.Repository<EldDriverMapping>()
                    .GetListAsync(m => m.ProviderType == config.ProviderType && m.IsSyncEnabled, ct);

                foreach (var mapping in mappings)
                {
                    var hosData = hosDataList.FirstOrDefault(h =>
                        h.ExternalDriverId == mapping.ExternalDriverId);

                    if (hosData == null)
                    {
                        continue;
                    }

                    // Get or create HOS status record
                    var hosStatus = await tenantUow.Repository<DriverHosStatus>()
                        .GetAsync(h => h.EmployeeId == mapping.EmployeeId, ct);

                    if (hosStatus == null)
                    {
                        hosStatus = new DriverHosStatus
                        {
                            EmployeeId = mapping.EmployeeId,
                            ExternalDriverId = mapping.ExternalDriverId,
                            ProviderType = config.ProviderType,
                            CurrentDutyStatus = hosData.CurrentDutyStatus
                        };
                        await tenantUow.Repository<DriverHosStatus>().AddAsync(hosStatus);
                    }

                    // Update HOS status
                    hosStatus.CurrentDutyStatus = hosData.CurrentDutyStatus;
                    hosStatus.StatusChangedAt = hosData.StatusChangedAt;
                    hosStatus.DrivingMinutesRemaining = hosData.DrivingMinutesRemaining;
                    hosStatus.OnDutyMinutesRemaining = hosData.OnDutyMinutesRemaining;
                    hosStatus.CycleMinutesRemaining = hosData.CycleMinutesRemaining;
                    hosStatus.TimeUntilBreakRequired = hosData.TimeUntilBreakRequired;
                    hosStatus.IsInViolation = hosData.IsInViolation;
                    hosStatus.NextMandatoryBreakAt = hosData.NextMandatoryBreakAt;
                    hosStatus.LastUpdatedAt = DateTime.UtcNow;

                    // Update mapping last synced time
                    mapping.LastSyncedAt = DateTime.UtcNow;

                    syncCount++;
                }

                // Update config last synced time
                config.LastSyncedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to sync HOS data from {ProviderType}", config.ProviderType);
                errorCount++;
            }
        }

        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation("Synced HOS status for {SyncCount} drivers with {ErrorCount} errors",
            syncCount, errorCount);

        return errorCount == 0
            ? Result.Ok()
            : Result.Fail($"Sync completed with {errorCount} errors");
    }
}
