using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

[RequiresFeature(TenantFeature.Payroll)]
public class SubmitPayrollForApprovalCommand : ICommand
{
    public Guid Id { get; set; }
}
