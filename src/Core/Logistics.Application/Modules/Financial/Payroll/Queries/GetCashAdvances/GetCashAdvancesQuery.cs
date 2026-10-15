using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Payroll.Queries;

/// <summary>Advances, newest first. <see cref="Outstanding"/> keeps only approved ones not yet deducted.</summary>
[RequiresFeature(TenantFeature.Payroll)]
public class GetCashAdvancesQuery : IQuery<Result<List<CashAdvanceDto>>>
{
    public Guid? EmployeeId { get; set; }
    public CashAdvanceStatus? Status { get; set; }
    public bool Outstanding { get; set; }
}
