using Logistics.Application.Abstractions;
using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetTrucksHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetTrucksQuery, PagedResult<TruckDto>>
{
    public async Task<PagedResult<TruckDto>> Handle(
        GetTrucksQuery req,
        CancellationToken ct)
    {
        var spec = new SearchTrucks(req.Search, req.OrderBy);
        var baseQuery = tenantUow.Repository<Truck>().ApplySpecification(spec);

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

        return PagedResult<TruckDto>.Succeed(trucks, totalItems, req.PageSize);
    }
}
