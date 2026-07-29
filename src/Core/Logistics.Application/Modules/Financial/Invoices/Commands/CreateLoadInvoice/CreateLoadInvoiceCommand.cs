using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

public class CreateLoadInvoiceCommand : ICommand<Result<Guid>>
{
    public Guid CustomerId { get; set; }
    public Guid LoadId { get; set; }
    public string? StripePaymentMethodId { get; set; }

    /// <summary>
    ///     The invoice total. Also recorded as an immediate payment unless
    ///     <see cref="RecordPayment" /> is false.
    /// </summary>
    public decimal PaymentAmount { get; set; }

    /// <summary>When false, the invoice is created unpaid, to be settled later.</summary>
    public bool RecordPayment { get; set; } = true;
}
