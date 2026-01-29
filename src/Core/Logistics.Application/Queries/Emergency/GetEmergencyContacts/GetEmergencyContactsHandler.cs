using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetEmergencyContactsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetEmergencyContactsQuery, PagedResult<EmergencyContactDto>>
{
    public Task<PagedResult<EmergencyContactDto>> Handle(GetEmergencyContactsQuery req, CancellationToken ct)
    {
        var baseQuery = tenantUow.Repository<EmergencyContact>().Query();

        if (req.EmployeeId.HasValue)
        {
            baseQuery = baseQuery.Where(c => c.EmployeeId == req.EmployeeId);
        }

        if (req.ContactType.HasValue)
        {
            baseQuery = baseQuery.Where(c => c.ContactType == req.ContactType);
        }

        if (req.IsActive.HasValue)
        {
            baseQuery = baseQuery.Where(c => c.IsActive == req.IsActive);
        }

        baseQuery = baseQuery.OrderBy(c => c.Priority, descending: false);

        var totalItems = baseQuery.Count();
        baseQuery = baseQuery.ApplyPaging(req.Page, req.PageSize);

        var dtos = baseQuery.Select(c => c.ToDto()).ToArray();

        return Task.FromResult(PagedResult<EmergencyContactDto>.Succeed(dtos, totalItems, req.PageSize));
    }
}
