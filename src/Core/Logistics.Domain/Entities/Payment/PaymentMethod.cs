using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

public abstract class PaymentMethod : Entity, ITenantEntity
{
    public abstract PaymentMethodType Type { get; protected set; }
    public string? StripePaymentMethodId { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public required Address BillingAddress { get; set; }
    public required PaymentMethodVerificationStatus VerificationStatus { get; set; }
}
