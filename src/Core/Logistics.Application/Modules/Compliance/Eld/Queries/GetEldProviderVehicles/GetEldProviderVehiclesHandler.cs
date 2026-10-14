using Logistics.Application.Abstractions.Eld;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Eld.Queries;

internal sealed class GetEldProviderVehiclesHandler(
    ITenantUnitOfWork tenantUow,
    IEldProviderFactory eldProviderFactory)
    : IRequestHandler<GetEldProviderVehiclesQuery, Result<List<EldVehicleDto>>>
{
    public async Task<Result<List<EldVehicleDto>>> Handle(GetEldProviderVehiclesQuery req, CancellationToken ct)
    {
        var config = await tenantUow.Repository<EldProviderConfiguration>().GetByIdAsync(req.ProviderId, ct);
        if (config is null)
        {
            return Result<List<EldVehicleDto>>.Fail("ELD provider configuration not found");
        }

        var vehicles = await eldProviderFactory.GetProvider(config).GetAllVehiclesAsync();
        return Result<List<EldVehicleDto>>.Ok(vehicles.ToList());
    }
}
