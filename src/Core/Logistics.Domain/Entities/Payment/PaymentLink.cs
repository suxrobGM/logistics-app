using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

/// <summary>
///     Represents a public payment link for an invoice that allows unauthenticated access.
/// </summary>
public class PaymentLink : AuditableEntity, ITenantEntity
{
    /// <summary>
    ///     Unique secure token for payment link access.
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    ///     The invoice this payment link provides access to.
    /// </summary>
    public required Guid InvoiceId { get; set; }

    public virtual Invoice? Invoice { get; set; }

    /// <summary>
    ///     When the payment link expires (default 30 days).
    /// </summary>
    public required DateTime ExpiresAt { get; set; }

    /// <summary>
    ///     Whether the payment link is active (can be revoked).
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     The user who created this payment link.
    /// </summary>
    public required Guid CreatedByUserId { get; set; }

    /// <summary>
    ///     Number of times this payment link has been accessed.
    /// </summary>
    public int AccessCount { get; set; }

    /// <summary>
    ///     Last time this payment link was accessed.
    /// </summary>
    public DateTime? LastAccessedAt { get; set; }

    /// <summary>
    ///     Whether the payment link is currently valid (active and not expired).
    /// </summary>
    public bool IsValid => IsActive && DateTime.UtcNow < ExpiresAt;

    /// <summary>
    ///     Records an access to this payment link.
    /// </summary>
    public void RecordAccess()
    {
        AccessCount++;
        LastAccessedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Revokes the payment link.
    /// </summary>
    public void Revoke() => IsActive = false;
}
