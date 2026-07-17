using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.FuelCards.Queries;

internal sealed class GetFuelCardProviderConfigurationsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetFuelCardProviderConfigurationsQuery, Result<List<FuelCardProviderConfigurationDto>>>
{
    public async Task<Result<List<FuelCardProviderConfigurationDto>>> Handle(
        GetFuelCardProviderConfigurationsQuery req,
        CancellationToken ct)
    {
        var configs = await tenantUow.Repository<FuelCardProviderConfiguration>().GetListAsync(ct: ct);

        var dtos = new List<FuelCardProviderConfigurationDto>();
        foreach (var config in configs)
        {
            var pendingCount = await tenantUow.Repository<FuelCardTransaction>()
                .CountAsync(t => t.ProviderType == config.ProviderType
                    && t.Status == FuelCardTransactionStatus.Pending, ct);

            dtos.Add(new FuelCardProviderConfigurationDto
            {
                Id = config.Id,
                ProviderType = config.ProviderType,
                ProviderName = config.ProviderType.GetDescription(),
                IsActive = config.IsActive,
                IsConnected = config.IsActive && !string.IsNullOrEmpty(config.ApiKey),
                LastSyncedAt = config.LastSyncedAt,
                ExternalAccountId = config.ExternalAccountId,
                PendingTransactionsCount = pendingCount
            });
        }

        return Result<List<FuelCardProviderConfigurationDto>>.Ok(dtos);
    }
}
