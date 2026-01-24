using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

/// <summary>
/// Represents a public tracking link for a load that allows unauthenticated access.
/// </summary>
public class TrackingLink : AuditableEntity, ITenantEntity
{
    /// <summary>
    /// Unique secure token for tracking link access.
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// The load this tracking link provides access to.
    /// </summary>
    public required Guid LoadId { get; set; }
    public virtual Load? Load { get; set; }

    /// <summary>
    /// When the tracking link expires.
    /// </summary>
    public required DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Whether the tracking link is active (can be revoked).
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// The user who created this tracking link.
    /// </summary>
    public required Guid CreatedByUserId { get; set; }

    /// <summary>
    /// Number of times this tracking link has been accessed.
    /// </summary>
    public int AccessCount { get; set; }

    /// <summary>
    /// Last time this tracking link was accessed.
    /// </summary>
    public DateTime? LastAccessedAt { get; set; }

    /// <summary>
    /// Whether the tracking link is currently valid (active and not expired).
    /// </summary>
    public bool IsValid => IsActive && DateTime.UtcNow < ExpiresAt;
}
