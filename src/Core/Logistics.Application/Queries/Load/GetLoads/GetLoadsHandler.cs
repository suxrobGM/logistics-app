using Logistics.Application.Abstractions;
using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetLoadsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetLoadsQuery, PagedResult<LoadDto>>
{
    public async Task<PagedResult<LoadDto>> Handle(
        GetLoadsQuery req,
        CancellationToken ct)
    {
        var totalItems = await tenantUow.Repository<Load>().CountAsync(ct: ct);
        var spec = new SearchLoads(req.Search, req.OrderBy);

        var baseQuery = tenantUow.Repository<Load>().ApplySpecification(spec);

        if (req.OnlyActiveLoads)
        {
            baseQuery = baseQuery.Where(i => i.DeliveredAt == null);
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

        if (req.Statuses?.Length > 0)
        {
            baseQuery = baseQuery.Where(i => req.Statuses.Contains(i.Status));
        }

        if (req.Types?.Length > 0)
        {
            baseQuery = baseQuery.Where(i => req.Types.Contains(i.Type));
        }

        if (req.CustomerId.HasValue)
        {
            baseQuery = baseQuery.Where(i => i.CustomerId == req.CustomerId.Value);
        }

        if (req is { StartDate: not null, EndDate: not null })
        {
            baseQuery = baseQuery.Where(i => i.DispatchedAt >= req.StartDate &&
                                             i.DispatchedAt <= req.EndDate);
        }

        if (!req.LoadAllPages)
        {
            baseQuery = baseQuery.ApplyPaging(req.Page, req.PageSize);
        }

        var loads = baseQuery.Select(i => i.ToDto()).ToArray();
        return PagedResult<LoadDto>.Succeed(loads, totalItems, req.PageSize);
    }
}
