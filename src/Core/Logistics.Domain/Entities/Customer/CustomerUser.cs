using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

/// <summary>
/// Represents a user associated with a customer for portal access.
/// Links an app-level User to a tenant-specific Customer.
/// </summary>
public class CustomerUser : AuditableEntity, ITenantEntity
{
    /// <summary>
    /// The ID of the User entity (from master database).
    /// </summary>
    public required Guid UserId { get; set; }

    /// <summary>
    /// The Customer this user is associated with.
    /// </summary>
    public required Guid CustomerId { get; set; }
    public virtual Customer? Customer { get; set; }

    /// <summary>
    /// Email address for the portal user.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Whether the portal access is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Last time the user logged into the portal.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Display name for the portal user.
    /// </summary>
    public string? DisplayName { get; set; }
}
