using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;
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

                // For Demo provider, auto-create mappings if none exist
                if (config.ProviderType == EldProviderType.Demo && !mappings.Any())
                {
                    mappings = await AutoCreateDemoMappingsAsync(hosDataList, ct);
                }

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

    /// <summary>
    /// Auto-creates driver mappings for Demo provider by mapping demo drivers to existing employees.
    /// This allows the Demo provider to work out-of-the-box without manual configuration.
    /// </summary>
    private async Task<List<EldDriverMapping>> AutoCreateDemoMappingsAsync(
        IEnumerable<EldDriverHosDataDto> hosDataList,
        CancellationToken ct)
    {
        // Get available employees (take up to 8 for demo)
        var employees = await tenantUow.Repository<Employee>()
            .Query()
            .Take(8)
            .ToListAsync(ct);

        if (!employees.Any())
        {
            logger.LogWarning("No employees found to create demo driver mappings");
            return [];
        }

        var mappings = new List<EldDriverMapping>();
        var hosDataArray = hosDataList.ToArray();

        for (var i = 0; i < Math.Min(employees.Count, hosDataArray.Length); i++)
        {
            var mapping = new EldDriverMapping
            {
                EmployeeId = employees[i].Id,
                ExternalDriverId = hosDataArray[i].ExternalDriverId,
                ExternalDriverName = hosDataArray[i].ExternalDriverName,
                ProviderType = EldProviderType.Demo,
                IsSyncEnabled = true
            };

            await tenantUow.Repository<EldDriverMapping>().AddAsync(mapping);
            mappings.Add(mapping);
        }

        logger.LogInformation("Auto-created {Count} demo driver mappings", mappings.Count);
        return mappings;
    }
}
