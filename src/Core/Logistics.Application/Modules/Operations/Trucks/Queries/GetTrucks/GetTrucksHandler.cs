using Logistics.Application.Abstractions;
using Logistics.Application.Common.Extensions;
using Logistics.Application.Modules.Operations.Loads.Services;
using Logistics.Application.Modules.Operations.Loads;
using Logistics.Application.Modules.Operations.Trucks.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Trucks.Queries;

internal sealed class GetTrucksHandler(
    ITenantUnitOfWork tenantUow,
    ILoadLookupBuilder lookupBuilder,
    ITruckDtoMapper truckDtoMapper)
    : IRequestHandler<GetTrucksQuery, PagedResult<TruckDto>>
{
    private static readonly SortFields<Truck> Sorts = new SortFields<Truck>("number")
        .Add("number", x => x.Number)
        .Add("type", x => x.Type)
        .Add("status", x => x.Status)
        .Add("licensePlate", x => x.LicensePlate);

    public async Task<PagedResult<TruckDto>> Handle(
        GetTrucksQuery req,
        CancellationToken ct)
    {
        var baseQuery = tenantUow.Repository<Truck>().Query().FilterForList(req);

        var totalItems = await baseQuery.CountAsync(ct);

        var entities = await Sorts.Apply(baseQuery, req.OrderBy, x => x.Id)
            .ApplyPaging(req.Page, req.PageSize)
            .ToArrayAsync(ct);
        var trailerNumbers = await GetTrailerNumbersAsync(entities, ct);
        var truckDtos = await truckDtoMapper.MapAsync(entities, ct);

        if (!req.IncludeLoads)
        {
            var withoutLoads = entities
                .Select(i => WithTrailer(truckDtos[i.Id], trailerNumbers))
                .ToArray();
            return PagedResult<TruckDto>.Ok(withoutLoads, totalItems, req.PageSize);
        }

        // i.Loads would lazy-load one SELECT per truck; fetch the whole page's loads in one.
        var truckIds = entities.Select(i => i.Id).ToArray();
        var pageLoads = await tenantUow.Repository<Load>()
            .Query()
            .Where(l => l.AssignedTruckId != null && truckIds.Contains(l.AssignedTruckId.Value))
            .ToArrayAsync(ct);

        var lookup = await lookupBuilder.BuildAsync(pageLoads, ct);
        var loadsByTruck = pageLoads
            .GroupBy(l => l.AssignedTruckId!.Value)
            .ToDictionary(g => g.Key, g => g.ToArray());

        var trucks = entities
            .Select(i => WithTrailer(
                WithLoads(truckDtos[i.Id], loadsByTruck.GetValueOrDefault(i.Id, []).Select(load => load.ToDto(lookup))),
                trailerNumbers))
            .ToArray();

        return PagedResult<TruckDto>.Ok(trucks, totalItems, req.PageSize);
    }

    private async Task<Dictionary<Guid, string>> GetTrailerNumbersAsync(Truck[] trucks, CancellationToken ct)
    {
        var trailerIds = trucks.Where(i => i.CurrentTrailerId != null).Select(i => i.CurrentTrailerId!.Value).ToArray();
        if (trailerIds.Length == 0)
        {
            return [];
        }

        return await tenantUow.Repository<Trailer>().Query()
            .Where(i => trailerIds.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id, i => i.Number, ct);
    }

    private static TruckDto WithLoads(TruckDto dto, IEnumerable<LoadDto> loads)
    {
        dto.Loads = loads;
        return dto;
    }

    private static TruckDto WithTrailer(TruckDto dto, Dictionary<Guid, string> trailerNumbers)
    {
        dto.CurrentTrailerNumber = dto.CurrentTrailerId is { } id ? trailerNumbers.GetValueOrDefault(id) : null;
        return dto;
    }
}
