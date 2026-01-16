using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

/// <summary>
/// Tracks which tenants a portal user can access.
/// Stored in the master database to enable cross-tenant lookups.
/// </summary>
public class UserTenantAccess : AuditableEntity, IMasterEntity
{
    public required Guid UserId { get; set; }
    public virtual User? User { get; set; }

    public required Guid TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    /// <summary>
    /// The CustomerUser ID in the tenant database (for reference).
    /// </summary>
    public Guid? CustomerUserId { get; set; }

    /// <summary>
    /// Cached customer name for display without cross-DB query.
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// Whether this tenant access is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Last time the user accessed this tenant's portal.
    /// </summary>
    public DateTime? LastAccessedAt { get; set; }
}
