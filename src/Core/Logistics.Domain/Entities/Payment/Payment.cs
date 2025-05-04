using Logistics.Domain.Core;
using Logistics.Domain.ValueObjects;
using Logistics.Shared.Consts;

namespace Logistics.Domain.Entities;

public class Payment : Entity, ITenantEntity // TODO: Rename to Invoice and create subclasss for each payment type
{
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public PaymentMethodType? Method { get; set; }
    public PaymentFor PaymentFor { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? PaymentDate { get; set; }
    public string? Notes { get; set; }
    public Address BillingAddress { get; set; } = Address.NullAddress;
    public string? StripeInvoiceId { get; set; }
    public string? StripePaymentIntentId { get; set; } // TODO: remove this field
    
    /// <summary>
    /// The ID of the subscription associated with this payment, if applicable.
    /// </summary>
    public string? SubscriptionId { get; set; }

    public void SetStatus(PaymentStatus status)
    {
        if (status == PaymentStatus.Paid)
        {
            PaymentDate = DateTime.UtcNow;
        }

        Status = status;
    }
}
