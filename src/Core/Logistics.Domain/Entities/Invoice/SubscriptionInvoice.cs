using Logistics.Shared.Consts;

namespace Logistics.Domain.Entities;

public class SubscriptionInvoice : Invoice
{
    public override InvoiceType Type { get; set; } = InvoiceType.Subscription;
    public required string SubscriptionId { get; set; }
    public virtual Subscription Subscription { get; set; } = null!;
    
    public DateTime BillingPeriodStart { get; set; }
    public DateTime BillingPeriodEnd { get; set; }
}