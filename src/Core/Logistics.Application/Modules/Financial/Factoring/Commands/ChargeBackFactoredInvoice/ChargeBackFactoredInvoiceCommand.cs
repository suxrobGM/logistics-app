using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Factoring.Commands;

/// <summary>
/// The factor sold a funded invoice back to the carrier (recourse): the customer owes the carrier again.
/// </summary>
public class ChargeBackFactoredInvoiceCommand : ICommand<Result>
{
    public Guid BatchId { get; set; }
    public Guid ItemId { get; set; }
    public DateTime ChargedBackAt { get; set; }
}
