using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

/// <summary>
/// One-time use token for admin impersonation of users.
/// Stored in the master database.
/// </summary>
public class ImpersonationToken : Entity, IMasterEntity
{
    /// <summary>
    /// Secure random token for URL parameter.
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// The admin user who initiated the impersonation.
    /// </summary>
    public Guid AdminUserId { get; set; }

    /// <summary>
    /// The user being impersonated.
    /// </summary>
    public Guid TargetUserId { get; set; }

    /// <summary>
    /// When the token expires (short-lived, typically 5 minutes).
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Whether the token has been used.
    /// </summary>
    public bool IsUsed { get; set; }

    /// <summary>
    /// When the token was used (if used).
    /// </summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>
    /// When the token was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsValid => !IsUsed && DateTime.UtcNow < ExpiresAt;
}
