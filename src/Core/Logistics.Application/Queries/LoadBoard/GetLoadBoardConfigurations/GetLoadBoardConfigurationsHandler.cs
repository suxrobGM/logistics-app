using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetLoadBoardConfigurationsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetLoadBoardConfigurationsQuery, Result<List<LoadBoardConfigurationDto>>>
{
    public async Task<Result<List<LoadBoardConfigurationDto>>> Handle(
        GetLoadBoardConfigurationsQuery req,
        CancellationToken ct)
    {
        var configs = await tenantUow.Repository<LoadBoardConfiguration>().GetListAsync();

        var dtos = new List<LoadBoardConfigurationDto>();

        foreach (var config in configs)
        {
            var activeListingsCount = await tenantUow.Repository<LoadBoardListing>()
                .CountAsync(l => l.ProviderType == config.ProviderType
                    && l.Status == LoadBoardListingStatus.Available, ct);

            var postedTrucksCount = await tenantUow.Repository<PostedTruck>()
                .CountAsync(t => t.ProviderType == config.ProviderType
                    && t.Status == PostedTruckStatus.Available, ct);

            dtos.Add(new LoadBoardConfigurationDto
            {
                Id = config.Id,
                ProviderType = config.ProviderType,
                ProviderName = config.ProviderType.ToString(),
                IsActive = config.IsActive,
                LastSyncedAt = config.LastSyncedAt,
                IsConnected = config.IsActive && !string.IsNullOrEmpty(config.ApiKey),
                ActiveListingsCount = activeListingsCount,
                PostedTrucksCount = postedTrucksCount,
                CompanyDotNumber = config.CompanyDotNumber,
                CompanyMcNumber = config.CompanyMcNumber
            });
        }

        return Result<List<LoadBoardConfigurationDto>>.Ok(dtos);
    }
}
