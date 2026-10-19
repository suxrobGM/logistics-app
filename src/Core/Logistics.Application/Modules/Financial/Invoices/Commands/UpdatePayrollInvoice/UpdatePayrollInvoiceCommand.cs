using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

[RequiresFeature(TenantFeature.Payroll)]
public class UpdatePayrollInvoiceCommand : ICommand
{
    public Guid Id { get; set; }
    public Guid? EmployeeId { get; set; }
    public DateOnly? PeriodStart { get; set; }
    public DateOnly? PeriodEnd { get; set; }
}
