using Logistics.Application.Abstractions;
using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetTrucksHandler : RequestHandler<GetTrucksQuery, PagedResult<TruckDto>>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public GetTrucksHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    public override async Task<PagedResult<TruckDto>> Handle(
        GetTrucksQuery req,
        CancellationToken ct)
    {
        var totalItems = await _tenantUow.Repository<Truck>().CountAsync();
        var spec = new SearchTrucks(req.Search, req.OrderBy, req.Page, req.PageSize);

        var truckQuery = _tenantUow.Repository<Truck>().ApplySpecification(spec);

        var trucks = (req.IncludeLoads
                ? truckQuery.Select(i => i.ToDto(i.Loads.Select(load => load.ToDto())))
                : truckQuery.Select(i => i.ToDto(new List<LoadDto>())))
            .ToArray();

        return PagedResult<TruckDto>.Succeed(trucks, totalItems, req.PageSize);
    }
}
