using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Trucks.Queries;

internal sealed class GetTruckHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetTruckQuery, Result<TruckDto>>
{
    public async Task<Result<TruckDto>> Handle(
        GetTruckQuery req, CancellationToken ct)
    {
        var truckEntity = await TryGetTruck(req.TruckOrDriverId);

        if (truckEntity is null)
        {
            return Result<TruckDto>.Fail($"Could not find a truck with ID '{req.TruckOrDriverId}'");
        }

        var truckDto = ConvertToDto(truckEntity, req.IncludeLoads, req.OnlyActiveLoads);
        return Result<TruckDto>.Ok(truckDto);
    }

    private async Task<Truck?> TryGetTruck(Guid? truckOrDriverId)
    {
        if (!truckOrDriverId.HasValue)
        {
            return null;
        }

        var truck = await tenantUow.Repository<Truck>().GetAsync(i => i.Id == truckOrDriverId);
        return truck ?? await GetTruckFromDriver(truckOrDriverId.Value);
    }

    private Task<Truck?> GetTruckFromDriver(Guid userId)
    {
        return tenantUow.Repository<Truck>().GetAsync(i => i.MainDriverId == userId || i.SecondaryDriverId == userId);
    }

    private static TruckDto ConvertToDto(Truck truckEntity, bool includeLoads, bool onlyActiveLoads)
    {
        var truckDto = truckEntity.ToDto(new List<LoadDto>());

        if (!includeLoads)
        {
            return truckDto;
        }

        var loads = truckEntity.Loads.Select(l => l.ToDto());
        if (onlyActiveLoads)
        {
            loads = loads.Where(l => l.DeliveredAt == null);
        }

        truckDto.Loads = loads.ToArray();
        return truckDto;
    }
}
