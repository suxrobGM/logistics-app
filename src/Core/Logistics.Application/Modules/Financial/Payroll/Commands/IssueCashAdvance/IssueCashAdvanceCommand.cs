using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Payroll.Commands;

/// <summary>A dispatcher pays a driver an advance; it is approved at once and owed until the next settlement.</summary>
[RequiresFeature(TenantFeature.Payroll)]
public class IssueCashAdvanceCommand : ICommand<Result<CashAdvanceDto>>
{
    public Guid EmployeeId { get; set; }
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
}
