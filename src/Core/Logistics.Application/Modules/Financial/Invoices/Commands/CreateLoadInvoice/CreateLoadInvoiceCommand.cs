using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

/// <summary>
///     Creates a load invoice billed to the load's customer and built from its charges (linehaul, fuel
///     surcharge, accessorials).
/// </summary>
public class CreateLoadInvoiceCommand : ICommand<Result<Guid>>
{
    public Guid LoadId { get; set; }
    public string? StripePaymentMethodId { get; set; }

    /// <summary>
    ///     The payment recorded against the new invoice; null creates it unpaid, to be settled later.
    ///     The invoice total always comes from the load's charges.
    /// </summary>
    public decimal? PaymentAmount { get; set; }
}
