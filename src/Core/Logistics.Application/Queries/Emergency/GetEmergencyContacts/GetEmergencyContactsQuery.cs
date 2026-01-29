using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetEmergencyContactsQuery : SearchableQuery, IAppRequest<PagedResult<EmergencyContactDto>>
{
    public Guid? EmployeeId { get; set; }
    public EmergencyContactType? ContactType { get; set; }
    public bool? IsActive { get; set; }
}
