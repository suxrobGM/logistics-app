using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetDriverCertificationsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetDriverCertificationsQuery, PagedResult<DriverCertificationDto>>
{
    public Task<PagedResult<DriverCertificationDto>> Handle(GetDriverCertificationsQuery req, CancellationToken ct)
    {
        var baseQuery = tenantUow.Repository<DriverCertification>().Query();

        if (req.EmployeeId.HasValue)
        {
            baseQuery = baseQuery.Where(c => c.EmployeeId == req.EmployeeId);
        }

        if (req.Type.HasValue)
        {
            baseQuery = baseQuery.Where(c => c.CertificationType == req.Type);
        }

        if (req.Status.HasValue)
        {
            baseQuery = baseQuery.Where(c => c.Status == req.Status);
        }

        baseQuery = baseQuery.OrderBy(c => c.ExpirationDate, descending: false);

        var totalItems = baseQuery.Count();
        baseQuery = baseQuery.ApplyPaging(req.Page, req.PageSize);

        var dtos = baseQuery.Select(c => c.ToDto()).ToArray();

        return Task.FromResult(PagedResult<DriverCertificationDto>.Succeed(dtos, totalItems, req.PageSize));
    }
}
