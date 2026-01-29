using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetActiveEmergencyAlertsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetActiveEmergencyAlertsQuery, PagedResult<EmergencyAlertDto>>
{
    public Task<PagedResult<EmergencyAlertDto>> Handle(GetActiveEmergencyAlertsQuery req, CancellationToken ct)
    {
        var baseQuery = tenantUow.Repository<EmergencyAlert>().Query()
            .Where(a => a.Status == EmergencyAlertStatus.Active ||
                        a.Status == EmergencyAlertStatus.Acknowledged ||
                        a.Status == EmergencyAlertStatus.Dispatching ||
                        a.Status == EmergencyAlertStatus.OnScene);

        baseQuery = baseQuery.OrderBy(a => a.TriggeredAt, descending: true);

        var totalItems = baseQuery.Count();
        baseQuery = baseQuery.ApplyPaging(req.Page, req.PageSize);

        var dtos = baseQuery.Select(a => a.ToDto()).ToArray();

        return Task.FromResult(PagedResult<EmergencyAlertDto>.Succeed(dtos, totalItems, req.PageSize));
    }
}
