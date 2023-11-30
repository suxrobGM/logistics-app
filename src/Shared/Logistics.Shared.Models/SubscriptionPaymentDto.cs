using Logistics.Shared.Enums;

namespace Logistics.Shared.Models;

public class SubscriptionPaymentDto
{
    public required string Id { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public PaymentMethod? Method { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public required AddressDto BillingAddress { get; set; }
    public required SubscriptionDto Subscription { get; set; }
}
