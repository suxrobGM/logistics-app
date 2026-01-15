using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

public class Payment : AuditableEntity, IMasterEntity, ITenantEntity
{
    public required Money Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public required Guid MethodId { get; set; }
    public required Guid TenantId { get; set; }
    public string? Description { get; set; }
    public required Address BillingAddress { get; set; }
    public string? StripePaymentIntentId { get; set; }
}
