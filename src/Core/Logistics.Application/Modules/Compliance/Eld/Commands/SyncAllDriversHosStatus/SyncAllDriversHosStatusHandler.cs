using Logistics.Application.Modules.Compliance.Eld.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Compliance.Eld.Commands;

internal sealed class SyncAllDriversHosStatusHandler(
    ITenantUnitOfWork tenantUow,
    IEldSyncService eldSyncService,
    ILogger<SyncAllDriversHosStatusHandler> logger)
    : IRequestHandler<SyncAllDriversHosStatusCommand, Result>
{
    public async Task<Result> Handle(SyncAllDriversHosStatusCommand req, CancellationToken ct)
    {
        var configs = await tenantUow.Repository<EldProviderConfiguration>()
            .GetListAsync(c => c.IsActive, ct);

        if (configs.Count == 0)
        {
            return Result.Fail("No active ELD providers configured");
        }

        var errorCount = 0;
        foreach (var config in configs)
        {
            try
            {
                await eldSyncService.SyncLiveAsync(config, ct);
                await eldSyncService.SyncHistoryAsync(config, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to sync ELD data from {ProviderType}", config.ProviderType);
                errorCount++;
            }
        }

        return errorCount == 0
            ? Result.Ok()
            : Result.Fail($"Sync completed with {errorCount} errors");
    }
}
