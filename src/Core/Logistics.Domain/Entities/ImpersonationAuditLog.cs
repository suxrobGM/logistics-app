using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

/// <summary>
/// Audit log for impersonation events.
/// Tracks who impersonated whom, when, and from where.
/// </summary>
public class ImpersonationAuditLog : Entity, IMasterEntity
{
    /// <summary>
    /// The admin user who performed the impersonation.
    /// </summary>
    public Guid AdminUserId { get; set; }

    /// <summary>
    /// Email of the admin user (denormalized for easy querying).
    /// </summary>
    public string AdminEmail { get; set; } = string.Empty;

    /// <summary>
    /// The user who was impersonated.
    /// </summary>
    public Guid TargetUserId { get; set; }

    /// <summary>
    /// Email of the target user (denormalized for easy querying).
    /// </summary>
    public string TargetEmail { get; set; } = string.Empty;

    /// <summary>
    /// When the impersonation occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// IP address of the request.
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// User agent of the request.
    /// </summary>
    public string UserAgent { get; set; } = string.Empty;

    /// <summary>
    /// Whether the impersonation was successful.
    /// </summary>
    public bool WasSuccessful { get; set; }

    /// <summary>
    /// Reason for failure (if not successful).
    /// </summary>
    public string? FailureReason { get; set; }
}
