using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Application.Modules.Operations.Loads;
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

        // Resolve only the loads that reach the DTO, not ones OnlyActiveLoads drops.
        var loads = SelectLoads(truckEntity, req.IncludeLoads, req.OnlyActiveLoads);
        var intermodal = await LoadIntermodalResolver.ResolveAsync(tenantUow, loads, ct);

        var truckDto = ConvertToDto(truckEntity, loads, intermodal);
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

    private static IReadOnlyCollection<Load> SelectLoads(
        Truck truckEntity, bool includeLoads, bool onlyActiveLoads)
    {
        if (!includeLoads)
        {
            return [];
        }

        return onlyActiveLoads
            ? [.. truckEntity.Loads.Where(l => l.DeliveredAt is null)]
            : [.. truckEntity.Loads];
    }

    private static TruckDto ConvertToDto(
        Truck truckEntity,
        IReadOnlyCollection<Load> loads,
        LoadIntermodalLookup intermodal)
    {
        var truckDto = truckEntity.ToDto(new List<LoadDto>());
        truckDto.Loads = [.. loads.Select(l => l.ToDto(intermodal))];
        return truckDto;
    }
}
