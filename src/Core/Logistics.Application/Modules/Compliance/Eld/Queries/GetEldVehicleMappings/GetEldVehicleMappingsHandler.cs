using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Compliance.Eld.Queries;

internal sealed class GetEldVehicleMappingsHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetEldVehicleMappingsQuery, Result<List<EldVehicleMappingDto>>>
{
    public async Task<Result<List<EldVehicleMappingDto>>> Handle(GetEldVehicleMappingsQuery req, CancellationToken ct)
    {
        var config = await tenantUow.Repository<EldProviderConfiguration>().GetByIdAsync(req.ProviderId, ct);
        if (config is null)
        {
            return Result<List<EldVehicleMappingDto>>.Fail("ELD provider configuration not found");
        }

        var mappings = await tenantUow.Repository<EldVehicleMapping>()
            .Query()
            .Where(m => m.ProviderType == config.ProviderType)
            .Select(m => new EldVehicleMappingDto
            {
                Id = m.Id,
                TruckId = m.TruckId,
                TruckNumber = m.Truck.Number,
                ProviderType = m.ProviderType,
                ExternalVehicleId = m.ExternalVehicleId,
                ExternalVehicleName = m.ExternalVehicleName,
                IsSyncEnabled = m.IsSyncEnabled,
                LastSyncedAt = m.LastSyncedAt
            })
            .ToListAsync(ct);

        return Result<List<EldVehicleMappingDto>>.Ok(mappings);
    }
}
