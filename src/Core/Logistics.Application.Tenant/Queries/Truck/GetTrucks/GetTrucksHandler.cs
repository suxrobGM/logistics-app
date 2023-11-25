using Logistics.Application.Tenant.Mappers;
using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetTrucksHandler : RequestHandler<GetTrucksQuery, PagedResponseResult<TruckDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetTrucksHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<PagedResponseResult<TruckDto>> HandleValidated(
        GetTrucksQuery req,
        CancellationToken cancellationToken)
    {
        var totalItems = await _tenantUow.Repository<Truck>().CountAsync();
        var spec = new SearchTrucks(req.Search, req.OrderBy, req.Page, req.PageSize, req.Descending);
        
        var truckQuery = _tenantUow.Repository<Truck>().ApplySpecification(spec)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize);

        var trucks = (req.IncludeLoads 
                ? truckQuery.Select(i => i.ToDto(i.Loads.Select(load => load.ToDto())))
                : truckQuery.Select(i => i.ToDto(new List<LoadDto>())))
            .ToArray();

        var totalPages = (int)Math.Ceiling(totalItems / (double)req.PageSize);
        return PagedResponseResult<TruckDto>.Create(trucks, totalItems, totalPages);
    }
}
