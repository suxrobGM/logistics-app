using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetLoadsHandler : RequestHandler<GetLoadsQuery, PagedResponseResult<LoadDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetLoadsHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<PagedResponseResult<LoadDto>> HandleValidated(
        GetLoadsQuery req, 
        CancellationToken cancellationToken)
    {
        var totalItems = await _tenantUow.Repository<Load>().CountAsync();
        var spec = new SearchLoads(req.Search, req.OrderBy);

        var baseQuery = _tenantUow.Repository<Load>().ApplySpecification(spec);

        if (req.OnlyActiveLoads)
        {
            baseQuery = baseQuery.Where(i => i.DeliveryDate == null);
        }
        if (!string.IsNullOrEmpty(req.UserId))
        {
            baseQuery = baseQuery.Where(i => i.AssignedTruck != null &&
                                             i.AssignedTruck.Drivers.Select(emp => emp.Id).Contains(req.UserId));
        }
        if (!string.IsNullOrEmpty(req.TruckId))
        {
            baseQuery = baseQuery.Where(i => i.AssignedTruckId == req.TruckId);
        }
        if (req.StartDate.HasValue && req.EndDate.HasValue)
        {
            baseQuery = baseQuery.Where(i => i.DispatchedDate >= req.StartDate && 
                                             i.DispatchedDate <= req.EndDate);
        }
        if (!req.LoadAllPages)
        {
            baseQuery = baseQuery.Skip((req.Page - 1) * req.PageSize).Take(req.PageSize);
        }
        
        var loads = baseQuery.Select(i => i.ToDto()).ToArray();
        var totalPages = (int)Math.Ceiling(totalItems / (double)req.PageSize);
        return PagedResponseResult<LoadDto>.Create(loads, totalItems, totalPages);
    }
}
