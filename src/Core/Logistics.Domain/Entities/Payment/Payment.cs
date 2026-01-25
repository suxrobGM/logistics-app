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

    #region Manual Payment Fields

    /// <summary>
    /// Reference number for manual payments (check number, receipt number, etc.).
    /// </summary>
    public string? ReferenceNumber { get; set; }

    /// <summary>
    /// User ID who recorded the manual payment.
    /// </summary>
    public Guid? RecordedByUserId { get; set; }

    /// <summary>
    /// When the manual payment was recorded.
    /// </summary>
    public DateTime? RecordedAt { get; set; }

    #endregion
}
