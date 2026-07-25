using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Operations.Loads;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Trucks.Queries;

internal sealed class GetTrucksHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetTrucksQuery, PagedResult<TruckDto>>
{
    public async Task<PagedResult<TruckDto>> Handle(
        GetTrucksQuery req,
        CancellationToken ct)
    {
        var baseQuery = tenantUow.Repository<Truck>().Query();

        if (!string.IsNullOrEmpty(req.Search))
        {
            baseQuery = baseQuery.Where(i => i.Number.Contains(req.Search) ||
                            i.MainDriver != null && (i.MainDriver.FirstName.Contains(req.Search) ||
                                                     i.MainDriver.LastName.Contains(req.Search)) ||
                            (i.SecondaryDriver != null &&
                             (i.SecondaryDriver.FirstName.Contains(req.Search) ||
                              i.SecondaryDriver.LastName.Contains(req.Search))));
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

        if (!req.IncludeLoads)
        {
            var withoutLoads = entities.Select(i => i.ToDto(new List<LoadDto>())).ToArray();
            return PagedResult<TruckDto>.Ok(withoutLoads, totalItems, req.PageSize);
        }

        // i.Loads would lazy-load one SELECT per truck; fetch the whole page's loads in one.
        var truckIds = entities.Select(i => i.Id).ToArray();
        var pageLoads = await tenantUow.Repository<Load>()
            .Query()
            .Where(l => l.AssignedTruckId != null && truckIds.Contains(l.AssignedTruckId.Value))
            .ToArrayAsync(ct);

        var intermodal = await LoadIntermodalResolver.ResolveAsync(tenantUow, pageLoads, ct);
        var loadsByTruck = pageLoads
            .GroupBy(l => l.AssignedTruckId!.Value)
            .ToDictionary(g => g.Key, g => g.ToArray());

        var trucks = entities
            .Select(i => i.ToDto(loadsByTruck.GetValueOrDefault(i.Id, []).Select(load => load.ToDto(intermodal))))
            .ToArray();

        return PagedResult<TruckDto>.Ok(trucks, totalItems, req.PageSize);
    }
}
