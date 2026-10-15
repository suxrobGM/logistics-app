using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Payroll.Commands;

/// <summary>
/// Full replace of a driver's pay profile. Applies to settlements created or recalculated afterwards.
/// </summary>
[RequiresFeature(TenantFeature.Payroll)]
public class UpdateDriverPayProfileCommand : ICommand<Result>
{
    public Guid EmployeeId { get; set; }
    public bool PayExcludesFuelSurcharge { get; set; }
    public bool PayExcludesAccessorials { get; set; }
    public bool DeductFuelCardCharges { get; set; }
    public List<RecurringDeductionDto> RecurringDeductions { get; set; } = [];
}
