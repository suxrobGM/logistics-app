using Logistics.Application.Abstractions;
using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetLoadsHandler : IAppRequestHandler<GetLoadsQuery, PagedResult<LoadDto>>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public GetLoadsHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    public async Task<PagedResult<LoadDto>> Handle(
        GetLoadsQuery req,
        CancellationToken ct)
    {
        var totalItems = await _tenantUow.Repository<Load>().CountAsync();
        var spec = new SearchLoads(req.Search, req.OrderBy);

        var baseQuery = _tenantUow.Repository<Load>().ApplySpecification(spec);

        if (req.OnlyActiveLoads)
        {
            baseQuery = baseQuery.Where(i => i.DeliveryDate == null);
        }

        if (req.UserId.HasValue)
        {
            baseQuery = baseQuery.Where(i => i.AssignedTruck != null &&
                                             (i.AssignedTruck.MainDriverId == req.UserId ||
                                              i.AssignedTruck.SecondaryDriverId == req.UserId));
        }

        if (req.TruckId.HasValue)
        {
            baseQuery = baseQuery.Where(i => i.AssignedTruckId == req.TruckId);
        }

        if (req is { StartDate: not null, EndDate: not null })
        {
            baseQuery = baseQuery.Where(i => i.DispatchedDate >= req.StartDate &&
                                             i.DispatchedDate <= req.EndDate);
        }

        if (!req.LoadAllPages)
        {
            baseQuery = baseQuery.ApplyPaging(req.Page, req.PageSize);
        }

        var loads = baseQuery.Select(i => i.ToDto()).ToArray();
        return PagedResult<LoadDto>.Succeed(loads, totalItems, req.PageSize);
    }
}
