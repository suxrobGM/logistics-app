using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetEldProviderConfigurationsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetEldProviderConfigurationsQuery, Result<List<EldProviderConfigurationDto>>>
{
    public async Task<Result<List<EldProviderConfigurationDto>>> Handle(
        GetEldProviderConfigurationsQuery req,
        CancellationToken ct)
    {
        var configs = await tenantUow.Repository<EldProviderConfiguration>().GetListAsync();

        var dtos = new List<EldProviderConfigurationDto>();

        foreach (var config in configs)
        {
            var driverMappingsCount = await tenantUow.Repository<EldDriverMapping>()
                .CountAsync(m => m.ProviderType == config.ProviderType, ct);

            var vehicleMappingsCount = await tenantUow.Repository<EldVehicleMapping>()
                .CountAsync(m => m.ProviderType == config.ProviderType, ct);

            dtos.Add(config.ToDto(driverMappingsCount, vehicleMappingsCount));
        }

        return Result<List<EldProviderConfigurationDto>>.Ok(dtos);
    }
}
