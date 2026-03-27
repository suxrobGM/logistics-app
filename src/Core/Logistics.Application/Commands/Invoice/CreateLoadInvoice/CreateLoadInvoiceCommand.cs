using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class CreateLoadInvoiceCommand : IAppRequest
{
    public Guid CustomerId { get; set; }
    public Guid LoadId { get; set; }
    public string? StripePaymentMethodId { get; set; }
    public decimal PaymentAmount { get; set; }
}
