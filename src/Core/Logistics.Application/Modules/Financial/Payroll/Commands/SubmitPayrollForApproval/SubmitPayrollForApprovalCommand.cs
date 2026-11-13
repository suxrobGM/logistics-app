using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Financial.Payroll.Commands;

[RequiresFeature(TenantFeature.Payroll)]
public class SubmitPayrollForApprovalCommand : ICommand
{
    public Guid Id { get; set; }
}
