using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Operations.Loads.Services;
using Logistics.Application.Modules.Operations.Loads;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Trucks.Queries;

internal sealed class GetTruckHandler(ITenantUnitOfWork tenantUow, ILoadLookupBuilder lookupBuilder)
    : IRequestHandler<GetTruckQuery, Result<TruckDto>>
{
    public async Task<Result<TruckDto>> Handle(
        GetTruckQuery req, CancellationToken ct)
    {
        var truckEntity = await TryGetTruck(req.TruckOrDriverId);

        if (truckEntity is null)
        {
            return Result<TruckDto>.Fail($"Could not find a truck with ID '{req.TruckOrDriverId}'", ErrorCodes.NotFound);
        }

        var loads = await GetLoadsAsync(truckEntity.Id, req.IncludeLoads, req.ActiveLoadsOnly, ct);
        var lookup = await lookupBuilder.BuildAsync(loads, ct);

        var truckDto = ConvertToDto(truckEntity, loads, lookup);
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

    private async Task<IReadOnlyCollection<Load>> GetLoadsAsync(
        Guid truckId, bool includeLoads, bool activeLoadsOnly, CancellationToken ct)
    {
        if (!includeLoads)
        {
            return [];
        }

        var query = tenantUow.Repository<Load>().Query().Where(l => l.AssignedTruckId == truckId);
        if (activeLoadsOnly)
        {
            query = query.Where(l => l.DeliveredAt == null);
        }

        return await query.ToListAsync(ct);
    }

    private static TruckDto ConvertToDto(
        Truck truckEntity,
        IReadOnlyCollection<Load> loads,
        LoadLookup lookup)
    {
        var truckDto = truckEntity.ToDto([]);
        truckDto.Loads = [.. loads.Select(l => l.ToDto(lookup))];
        truckDto.CurrentTrailerNumber = truckEntity.CurrentTrailer?.Number;
        return truckDto;
    }
}
