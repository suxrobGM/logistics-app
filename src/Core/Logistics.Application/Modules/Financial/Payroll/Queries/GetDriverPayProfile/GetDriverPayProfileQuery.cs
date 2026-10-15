using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Payroll.Queries;

[RequiresFeature(TenantFeature.Payroll)]
public class GetDriverPayProfileQuery : IQuery<Result<DriverPayProfileDto>>
{
    public Guid EmployeeId { get; set; }
}
