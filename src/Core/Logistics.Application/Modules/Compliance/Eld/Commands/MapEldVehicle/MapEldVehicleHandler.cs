using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Eld.Commands;

internal sealed class MapEldVehicleHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<MapEldVehicleCommand, Result>
{
    public async Task<Result> Handle(MapEldVehicleCommand req, CancellationToken ct)
    {
        var truck = await tenantUow.Repository<Truck>().GetByIdAsync(req.TruckId, ct);
        if (truck is null)
        {
            return Result.Fail($"Truck with ID {req.TruckId} not found");
        }

        var mappings = tenantUow.Repository<EldVehicleMapping>();
        if (await mappings.GetAsync(m => m.TruckId == req.TruckId && m.ProviderType == req.ProviderType, ct) is not null)
        {
            return Result.Fail($"Truck {truck.Number} is already mapped to an ELD vehicle for this provider");
        }

        if (await mappings.GetAsync(
                m => m.ExternalVehicleId == req.ExternalVehicleId && m.ProviderType == req.ProviderType, ct) is not null)
        {
            return Result.Fail($"ELD vehicle {req.ExternalVehicleId} is already mapped to another truck");
        }

        await mappings.AddAsync(new EldVehicleMapping
        {
            TruckId = req.TruckId,
            ProviderType = req.ProviderType,
            ExternalVehicleId = req.ExternalVehicleId,
            ExternalVehicleName = req.ExternalVehicleName
        }, ct);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
