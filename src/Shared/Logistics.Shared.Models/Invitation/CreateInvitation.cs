using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

/// <summary>
/// Input for creating a new invitation.
/// </summary>
public record CreateInvitation
{
    /// <summary>
    /// Email address to send the invitation to.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Type of user being invited (Employee or CustomerUser).
    /// </summary>
    public required InvitationType Type { get; set; }

    /// <summary>
    /// The tenant role to assign upon acceptance.
    /// </summary>
    public required string TenantRole { get; set; }

    /// <summary>
    /// For CustomerUser invitations, the customer ID to associate with.
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// Optional personal message to include in the invitation email.
    /// </summary>
    public string? PersonalMessage { get; set; }

    /// <summary>
    /// Number of days until invitation expires (default 7).
    /// </summary>
    public int ExpirationDays { get; set; } = 7;
}
