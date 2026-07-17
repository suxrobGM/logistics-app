using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.FuelCards.Queries;

internal sealed class GetFuelCardProviderConfigurationsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetFuelCardProviderConfigurationsQuery, Result<List<FuelCardProviderConfigurationDto>>>
{
    public async Task<Result<List<FuelCardProviderConfigurationDto>>> Handle(
        GetFuelCardProviderConfigurationsQuery req,
        CancellationToken ct)
    {
        var configs = await tenantUow.Repository<FuelCardProviderConfiguration>().GetListAsync(ct: ct);

        // One grouped count for every provider, rather than a COUNT round trip per config.
        var pendingCounts = await tenantUow.Repository<FuelCardTransaction>().Query()
            .Where(t => t.Status == FuelCardTransactionStatus.Pending)
            .GroupBy(t => t.ProviderType)
            .Select(g => new { ProviderType = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ProviderType, x => x.Count, ct);

        var dtos = configs
            .Select(config => new FuelCardProviderConfigurationDto
            {
                Id = config.Id,
                ProviderType = config.ProviderType,
                ProviderName = config.ProviderType.GetDescription(),
                IsActive = config.IsActive,
                IsConnected = config.IsActive && !string.IsNullOrEmpty(config.ApiKey),
                LastSyncedAt = config.LastSyncedAt,
                ExternalAccountId = config.ExternalAccountId,
                PendingTransactionsCount = pendingCounts.GetValueOrDefault(config.ProviderType)
            })
            .ToList();

        return Result<List<FuelCardProviderConfigurationDto>>.Ok(dtos);
    }
}
