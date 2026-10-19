using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Operations.Loads;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Trucks.Queries;

internal sealed class GetTrucksHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetTrucksQuery, PagedResult<TruckDto>>
{
    public async Task<PagedResult<TruckDto>> Handle(
        GetTrucksQuery req,
        CancellationToken ct)
    {
        var baseQuery = tenantUow.Repository<Truck>().Query();

        if (!string.IsNullOrEmpty(req.Search))
        {
            var search = req.Search.ToLower();
            baseQuery = baseQuery.Where(i => i.Number.ToLower().Contains(search) ||
                            i.MainDriver != null && (i.MainDriver.FirstName.ToLower().Contains(search) ||
                                                     i.MainDriver.LastName.ToLower().Contains(search)) ||
                            (i.SecondaryDriver != null &&
                             (i.SecondaryDriver.FirstName.ToLower().Contains(search) ||
                              i.SecondaryDriver.LastName.ToLower().Contains(search))));
        }

        baseQuery = baseQuery.OrderBy(req.OrderBy);

        if (req.Statuses?.Length > 0)
        {
            baseQuery = baseQuery.Where(i => req.Statuses.Contains(i.Status));
        }

        if (req.Types?.Length > 0)
        {
            baseQuery = baseQuery.Where(i => req.Types.Contains(i.Type));
        }

        var totalItems = await baseQuery.CountAsync(ct);
        baseQuery = baseQuery.ApplyPaging(req.Page, req.PageSize);

        var entities = await baseQuery.ToArrayAsync(ct);
        var trailerNumbers = await GetTrailerNumbersAsync(entities, ct);

        if (!req.IncludeLoads)
        {
            var withoutLoads = entities
                .Select(i => WithTrailer(i.ToDto(new List<LoadDto>()), trailerNumbers))
                .ToArray();
            return PagedResult<TruckDto>.Ok(withoutLoads, totalItems, req.PageSize);
        }

        // i.Loads would lazy-load one SELECT per truck; fetch the whole page's loads in one.
        var truckIds = entities.Select(i => i.Id).ToArray();
        var pageLoads = await tenantUow.Repository<Load>()
            .Query()
            .Where(l => l.AssignedTruckId != null && truckIds.Contains(l.AssignedTruckId.Value))
            .ToArrayAsync(ct);

        var lookup = await LoadLookupResolver.ResolveAsync(tenantUow, pageLoads, ct);
        var loadsByTruck = pageLoads
            .GroupBy(l => l.AssignedTruckId!.Value)
            .ToDictionary(g => g.Key, g => g.ToArray());

        var trucks = entities
            .Select(i => WithTrailer(
                i.ToDto(loadsByTruck.GetValueOrDefault(i.Id, []).Select(load => load.ToDto(lookup))),
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

    private static TruckDto WithTrailer(TruckDto dto, Dictionary<Guid, string> trailerNumbers)
    {
        dto.CurrentTrailerNumber = dto.CurrentTrailerId is { } id ? trailerNumbers.GetValueOrDefault(id) : null;
        return dto;
    }
}
