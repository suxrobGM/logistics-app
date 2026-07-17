using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Integrations.FuelCards.Services;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.FuelCards.Commands;

internal sealed class SyncFuelCardTransactionsHandler(IFuelCardSyncService syncService)
    : IAppRequestHandler<SyncFuelCardTransactionsCommand, Result<FuelCardSyncResultDto>>
{
    public async Task<Result<FuelCardSyncResultDto>> Handle(SyncFuelCardTransactionsCommand req, CancellationToken ct)
    {
        var result = await syncService.SyncCurrentTenantAsync(req.ProviderType, ct);
        return Result<FuelCardSyncResultDto>.Ok(result);
    }
}
