using Logistics.Application.Tenant.Mappers;
using Logistics.Models;

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
        string[]? loadIds = null;
        var trucksDtoList = new List<TruckDto>();
        
        if (req.IncludeLoadIds)
        {
            loadIds = _tenantRepository.Query<Truck>()
                .SelectMany(i => i.Loads)
                .Select(i => i.Id)
                .ToArray();
        }
        
        var totalItems = _tenantRepository.Query<Truck>().Count();
        var spec = new SearchTrucks(req.Search, req.Descending);

        var trucks = _tenantRepository
            .ApplySpecification(spec)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToArray();

        foreach (var truckEntity in trucks)
        {
            var truckDto = truckEntity.ToDto();
            truckDto.LoadIds = loadIds;
            trucksDtoList.Add(truckDto);
        }

        var totalPages = (int)Math.Ceiling(totalItems / (double)req.PageSize);
        return Task.FromResult(new PagedResponseResult<TruckDto>(trucksDtoList, totalItems, totalPages));
    }
}
