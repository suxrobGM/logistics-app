using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Financial.Payroll.Commands;

[RequiresFeature(TenantFeature.Payroll)]
public class RejectPayrollInvoiceCommand : ICommand
{
    public Guid Id { get; set; }
    public required string Reason { get; set; }
}
