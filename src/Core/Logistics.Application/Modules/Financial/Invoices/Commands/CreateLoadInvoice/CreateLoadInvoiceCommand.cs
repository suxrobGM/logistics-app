using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

/// <summary>
///     Creates a load invoice built from the load's charges (linehaul, fuel surcharge, accessorials).
/// </summary>
public class CreateLoadInvoiceCommand : ICommand<Result<Guid>>
{
    public Guid CustomerId { get; set; }
    public Guid LoadId { get; set; }
    public string? StripePaymentMethodId { get; set; }

    /// <summary>
    ///     The payment recorded against the new invoice when <see cref="RecordPayment" /> is true.
    ///     Ignored otherwise - the invoice total always comes from the load's charges.
    /// </summary>
    public decimal PaymentAmount { get; set; }

    /// <summary>When false, the invoice is created unpaid, to be settled later.</summary>
    public bool RecordPayment { get; set; } = true;
}
