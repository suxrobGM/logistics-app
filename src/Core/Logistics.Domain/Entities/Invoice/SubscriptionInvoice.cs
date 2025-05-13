using Logistics.Shared.Consts;

namespace Logistics.Domain.Entities;

public class SubscriptionInvoice : Invoice
{
    public override InvoiceType Type { get; set; } = InvoiceType.Subscription;
    
    /// <summary>
    /// The subscription ID for which this invoice is generated.
    /// The Subscription entity is not included in the invoice to avoid circular references, and
    /// it is a master entity and should not be in the Tenant context.
    /// </summary>
    public required Guid SubscriptionId { get; set; }
    
    public DateTime BillingPeriodStart { get; set; }
    public DateTime BillingPeriodEnd { get; set; }
}