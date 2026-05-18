using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.Financial.Payroll.Commands;

public class PayPayrollInvoiceCommand : ICommand
{
    public Guid InvoiceId { get; set; }
}
