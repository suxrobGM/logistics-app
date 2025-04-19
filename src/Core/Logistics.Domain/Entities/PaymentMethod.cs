using Logistics.Domain.Core;
using Logistics.Domain.ValueObjects;
using Logistics.Shared.Consts;

namespace Logistics.Domain.Entities;

public abstract class PaymentMethod : Entity, ITenantEntity
{
    public required PaymentMethodType Type { get; set; }
    public string? StripePaymentMethodId { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public required Address BillingAddress { get; set; }
}