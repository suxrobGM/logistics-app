using Logistics.Application.Abstractions.Features;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Integrations.Accounting.Services;

internal sealed class AccountingSyncService(
    ITenantUnitOfWork tenantUow,
    IFeatureService featureService,
    IAccountingConnectionService connectionService,
    IAccountingReceivablesSyncService receivablesSyncService,
    IAccountingPayablesSyncService payablesSyncService,
    ILogger<AccountingSyncService> logger) : IAccountingSyncService
{
    public async Task SyncAsync(CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();
        if (!await featureService.IsFeatureEnabledAsync(tenant.Id, TenantFeature.Accounting))
        {
            return;
        }

        var result = await connectionService.ExecuteAsync(async (provider, config) =>
        {
            var run = new SyncRun(provider, config);
            await receivablesSyncService.PushAsync(run, ct);
            await payablesSyncService.PushAsync(run, ct);

            config.LastSyncedAt = run.StartedAt;
            await tenantUow.SaveChangesAsync(ct);
            return Result.Ok();
        }, ct);

        if (!result.IsSuccess)
        {
            logger.LogInformation("Skipped accounting sync for {TenantName}: {Reason}", tenant.Name, result.Error);
        }
    }
}
