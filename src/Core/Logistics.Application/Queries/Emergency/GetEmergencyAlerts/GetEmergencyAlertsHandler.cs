using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetEmergencyAlertsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetEmergencyAlertsQuery, PagedResult<EmergencyAlertDto>>
{
    public Task<PagedResult<EmergencyAlertDto>> Handle(GetEmergencyAlertsQuery req, CancellationToken ct)
    {
        var baseQuery = tenantUow.Repository<EmergencyAlert>().Query();

        if (req.DriverId.HasValue)
        {
            baseQuery = baseQuery.Where(a => a.DriverId == req.DriverId);
        }

        if (req.Status.HasValue)
        {
            baseQuery = baseQuery.Where(a => a.Status == req.Status);
        }

        if (req.Type.HasValue)
        {
            baseQuery = baseQuery.Where(a => a.AlertType == req.Type);
        }

        if (req.FromDate.HasValue)
        {
            baseQuery = baseQuery.Where(a => a.TriggeredAt >= req.FromDate);
        }

        if (req.ToDate.HasValue)
        {
            baseQuery = baseQuery.Where(a => a.TriggeredAt <= req.ToDate);
        }

        baseQuery = baseQuery.OrderBy(a => a.TriggeredAt, descending: true);

        var totalItems = baseQuery.Count();
        baseQuery = baseQuery.ApplyPaging(req.Page, req.PageSize);

        var dtos = baseQuery.Select(a => a.ToDto()).ToArray();

        return Task.FromResult(PagedResult<EmergencyAlertDto>.Succeed(dtos, totalItems, req.PageSize));
    }
}
