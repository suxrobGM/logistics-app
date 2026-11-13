using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Financial.Payroll.Commands;

[RequiresFeature(TenantFeature.Payroll)]
public class BatchApprovePayrollCommand : ICommand
{
    public required List<Guid> Ids { get; set; }
    public string? Notes { get; set; }
}
