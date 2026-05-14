using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class PayPayrollInvoiceCommand : ICommand
{
    public Guid InvoiceId { get; set; }
}
