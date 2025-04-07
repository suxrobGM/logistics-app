using Logistics.Domain.Core;
using Logistics.Domain.ValueObjects;
using Logistics.Shared.Consts;

namespace Logistics.Domain.Entities;

public class SubscriptionPayment : Entity
{
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public PaymentMethodType? Method { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public Address BillingAddress { get; set; } = Address.NullAddress;
    public string? StripeInvoiceId { get; set; }
    public string? StripePaymentIntentId { get; set; }

    public required string SubscriptionId { get; set; }
    public virtual required Subscription Subscription { get; set; }
    
    public void SetStatus(PaymentStatus status)
    {
        if (status == PaymentStatus.Paid)
        {
            PaymentDate = DateTime.UtcNow;
        }

        Status = status;
    }
}
