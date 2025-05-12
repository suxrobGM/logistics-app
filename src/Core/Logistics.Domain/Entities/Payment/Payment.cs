using Logistics.Domain.Core;
using Logistics.Domain.ValueObjects;
using Logistics.Shared.Consts;

namespace Logistics.Domain.Entities;

public class Payment : AuditableEntity, ITenantEntity
{
    public Money Amount { get; set; } = Money.Zero();
    public PaymentStatus Status { get; set; }
    public PaymentMethodType Method { get; set; }
    public Address BillingAddress { get; set; } = Address.NullAddress;
}
