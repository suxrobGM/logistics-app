using Logistics.Application.Tenant.Mappers;
using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetTrucksHandler : RequestHandler<GetTrucksQuery, PagedResponseResult<TruckDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetTrucksHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override Task<PagedResponseResult<TruckDto>> HandleValidated(
        GetTrucksQuery req,
        CancellationToken cancellationToken)
    {
        var totalItems = _tenantRepository.Query<Truck>().Count();
        var spec = new SearchTrucks(req.Search, req.OrderBy, req.Descending);
        
        var truckQuery = _tenantRepository.ApplySpecification(spec)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize);

        var trucks = (req.IncludeLoads 
                ? truckQuery.Select(i => i.ToDto(i.Loads.Select(load => load.ToDto())))
                : truckQuery.Select(i => i.ToDto(new List<LoadDto>())))
            .ToArray();

        var totalPages = (int)Math.Ceiling(totalItems / (double)req.PageSize);
        return Task.FromResult(new PagedResponseResult<TruckDto>(trucks, totalItems, totalPages));
    }
}
