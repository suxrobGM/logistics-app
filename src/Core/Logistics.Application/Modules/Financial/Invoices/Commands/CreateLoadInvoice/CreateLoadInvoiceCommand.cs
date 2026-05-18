using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

public class CreateLoadInvoiceCommand : ICommand
{
    public Guid CustomerId { get; set; }
    public Guid LoadId { get; set; }
    public string? StripePaymentMethodId { get; set; }
    public decimal PaymentAmount { get; set; }
}
