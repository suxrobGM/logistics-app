using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Payroll.Queries;

/// <summary>Advances, newest first.</summary>
[RequiresFeature(TenantFeature.Payroll)]
public class GetCashAdvancesQuery : IQuery<Result<List<CashAdvanceDto>>>
{
    public Guid? EmployeeId { get; set; }
}
