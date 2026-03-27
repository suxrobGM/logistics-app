using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class PayPayrollInvoiceCommand : IAppRequest
{
    public Guid InvoiceId { get; set; }
}
