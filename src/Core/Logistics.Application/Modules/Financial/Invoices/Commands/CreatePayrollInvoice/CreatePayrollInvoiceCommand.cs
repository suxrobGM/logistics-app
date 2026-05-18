using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

[RequiresFeature(TenantFeature.Payroll)]
public class CreatePayrollInvoiceCommand : ICommand
{
    public Guid EmployeeId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}
