using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetDriverBehaviorEventsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetDriverBehaviorEventsQuery, PagedResult<DriverBehaviorEventDto>>
{
    public Task<PagedResult<DriverBehaviorEventDto>> Handle(GetDriverBehaviorEventsQuery req, CancellationToken ct)
    {
        var baseQuery = tenantUow.Repository<DriverBehaviorEvent>().Query();

        if (req.DriverId.HasValue)
        {
            baseQuery = baseQuery.Where(e => e.EmployeeId == req.DriverId);
        }

        if (req.TruckId.HasValue)
        {
            baseQuery = baseQuery.Where(e => e.TruckId == req.TruckId);
        }

        if (req.EventType.HasValue)
        {
            baseQuery = baseQuery.Where(e => e.EventType == req.EventType);
        }

        if (req.IsReviewed.HasValue)
        {
            baseQuery = baseQuery.Where(e => e.IsReviewed == req.IsReviewed);
        }

        if (req.FromDate.HasValue)
        {
            baseQuery = baseQuery.Where(e => e.OccurredAt >= req.FromDate);
        }

        if (req.ToDate.HasValue)
        {
            baseQuery = baseQuery.Where(e => e.OccurredAt <= req.ToDate);
        }

        baseQuery = baseQuery.OrderBy(e => e.OccurredAt, descending: true);

        var totalItems = baseQuery.Count();
        baseQuery = baseQuery.ApplyPaging(req.Page, req.PageSize);

        var dtos = baseQuery.Select(e => e.ToDto()).ToArray();

        return Task.FromResult(PagedResult<DriverBehaviorEventDto>.Succeed(dtos, totalItems, req.PageSize));
    }
}
