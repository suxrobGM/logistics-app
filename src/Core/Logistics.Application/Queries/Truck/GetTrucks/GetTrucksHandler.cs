using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Specifications;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetTrucksHandler : RequestHandler<GetTrucksQuery, PagedResult<TruckDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetTrucksHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<PagedResult<TruckDto>> HandleValidated(
        GetTrucksQuery req,
        CancellationToken cancellationToken)
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
