using Logistics.Application.Tenant.Mappers;
using Logistics.Domain.Enums;
using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetLoadsHandler : RequestHandler<GetLoadsQuery, PagedResponseResult<LoadDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetLoadsHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override Task<PagedResponseResult<LoadDto>> HandleValidated(
        GetLoadsQuery req, 
        CancellationToken cancellationToken)
    {
        var totalItems = _tenantRepository.Query<Load>().Count();
        var spec = new SearchLoads(req.Search, req.OrderBy, req.Descending);

        var baseQuery = _tenantRepository.ApplySpecification(spec);

        if (req.FilterActiveLoads)
        {
            baseQuery = baseQuery.Where(i => i.DeliveryDate == null);
        }
        if (!string.IsNullOrEmpty(req.TruckId))
        {
            baseQuery = baseQuery.Where(i => i.AssignedTruckId == req.TruckId);
        }
        if (req.StartDate.HasValue && req.EndDate.HasValue)
        {
            baseQuery = baseQuery.Where(i => i.DispatchedDate >= req.StartDate && i.DispatchedDate <= req.EndDate);
        }
        if (!req.LoadAllPages)
        {
            baseQuery = baseQuery.Skip((req.Page - 1) * req.PageSize).Take(req.PageSize);
        }
        
        var loads = baseQuery.Select(i => i.ToDto()).ToArray();
        var totalPages = (int)Math.Ceiling(totalItems / (double)req.PageSize);
        return Task.FromResult(PagedResponseResult<LoadDto>.Create(loads, totalItems, totalPages));
    }
}
