using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Financial.Payroll.Commands;

[RequiresFeature(TenantFeature.Payroll)]
public class CreatePayrollInvoiceCommand : ICommand
{
    public Guid EmployeeId { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
}
