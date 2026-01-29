using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetDriverCertificationsQuery : SearchableQuery, IAppRequest<PagedResult<DriverCertificationDto>>
{
    public Guid? EmployeeId { get; set; }
    public CertificationType? Type { get; set; }
    public CertificationStatus? Status { get; set; }
}
