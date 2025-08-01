﻿using Logistics.Domain.Core;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public abstract class PaymentMethod : Entity, ITenantEntity
{
    public abstract PaymentMethodType Type { get; set; }
    public string? StripePaymentMethodId { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public required Address BillingAddress { get; set; }
    public required PaymentMethodVerificationStatus VerificationStatus { get; set; }
}
