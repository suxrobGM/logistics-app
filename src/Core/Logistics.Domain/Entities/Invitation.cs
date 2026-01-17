using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Represents an invitation to join a tenant as an employee or customer user.
/// Stored in the master database for cross-tenant access.
/// </summary>
public class Invitation : AuditableEntity, IMasterEntity
{
    /// <summary>
    /// Email address the invitation was sent to.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Unique token for invitation verification.
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// The tenant this invitation is for.
    /// </summary>
    public required Guid TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    /// <summary>
    /// The type of user being invited (Employee or CustomerUser).
    /// </summary>
    public required InvitationType Type { get; set; }

    /// <summary>
    /// The tenant role to assign upon acceptance (e.g., "tenant.dispatcher", "tenant.owner").
    /// </summary>
    public required string TenantRole { get; set; }

    /// <summary>
    /// For CustomerUser invitations, the customer ID to associate with.
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// When the invitation expires.
    /// </summary>
    public required DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Current status of the invitation.
    /// </summary>
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

    /// <summary>
    /// When the invitation was accepted (if accepted).
    /// </summary>
    public DateTime? AcceptedAt { get; set; }

    /// <summary>
    /// The user who was created when invitation was accepted.
    /// </summary>
    public Guid? AcceptedByUserId { get; set; }
    public virtual User? AcceptedByUser { get; set; }

    /// <summary>
    /// The user who sent the invitation.
    /// </summary>
    public required Guid InvitedByUserId { get; set; }
    public virtual User? InvitedByUser { get; set; }

    /// <summary>
    /// Optional personal message included in the invitation email.
    /// </summary>
    public string? PersonalMessage { get; set; }

    /// <summary>
    /// Number of times the invitation email was sent/resent.
    /// </summary>
    public int SendCount { get; set; } = 1;

    /// <summary>
    /// Last time the invitation email was sent.
    /// </summary>
    public DateTime? LastSentAt { get; set; }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsValid => Status == InvitationStatus.Pending && !IsExpired;
}
