using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Queries;

internal sealed class GetLoadsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetLoadsQuery, PagedResult<LoadDto>>
{
    public Task<PagedResult<LoadDto>> Handle(
        GetLoadsQuery req,
        CancellationToken ct)
    {
        var baseQuery = tenantUow.Repository<Load>().Query();

        if (!string.IsNullOrEmpty(req.Search))
        {
            baseQuery = baseQuery.Where(i =>
                (i.Name != null && i.Name.Contains(req.Search)) ||
                (i.Customer != null && i.Customer.Name.Contains(req.Search)) ||
                i.Number.ToString().Contains(req.Search) ||
                i.OriginAddress.Line1.Contains(req.Search) ||
                (i.OriginAddress.Line2 != null && i.OriginAddress.Line2.Contains(req.Search)) ||
                i.DestinationAddress.Line1.Contains(req.Search) ||
                (i.DestinationAddress.Line2 != null && i.DestinationAddress.Line2.Contains(req.Search)));
        }

        if (req.OnlyActiveLoads)
        {
            baseQuery = baseQuery.Where(i =>
                i.Status == LoadStatus.Dispatched || i.Status == LoadStatus.PickedUp);
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

        var totalItems = baseQuery.Count();

        baseQuery = baseQuery.OrderBy(req.OrderBy);

        if (!req.LoadAllPages)
        {
            baseQuery = baseQuery.ApplyPaging(req.Page, req.PageSize);
        }

        var loads = baseQuery.Select(i => i.ToDto()).ToArray();
        return Task.FromResult(PagedResult<LoadDto>.Ok(loads, totalItems, req.PageSize));
    }
}
