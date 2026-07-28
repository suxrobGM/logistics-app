using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

public class CreateLoadInvoiceCommand : ICommand
{
    public Guid CustomerId { get; set; }
    public Guid LoadId { get; set; }
    public string? StripePaymentMethodId { get; set; }

    /// <summary>
    ///     The invoice total. Also recorded as an immediate payment unless
    ///     <see cref="RecordPayment" /> is false.
    /// </summary>
    public decimal PaymentAmount { get; set; }

    /// <summary>
    ///     When false, the invoice is created unpaid (to be settled later via a payment link or
    ///     manual payment) instead of applying <see cref="PaymentAmount" /> as a payment on creation.
    /// </summary>
    public bool RecordPayment { get; set; } = true;
}
