using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Trucks.Queries;

internal sealed class GetTrucksHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetTrucksQuery, PagedResult<TruckDto>>
{
    public Task<PagedResult<TruckDto>> Handle(
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

        var totalItems = baseQuery.Count();
        baseQuery = baseQuery.ApplyPaging(req.Page, req.PageSize);

        var trucks = (req.IncludeLoads
                ? baseQuery.Select(i => i.ToDto(i.Loads.Select(load => load.ToDto())))
                : baseQuery.Select(i => i.ToDto(new List<LoadDto>())))
            .ToArray();

        return Task.FromResult(PagedResult<TruckDto>.Ok(trucks, totalItems, req.PageSize));
    }
}
