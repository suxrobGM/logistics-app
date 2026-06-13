namespace Logistics.Shared.Models;

/// <summary>
/// Result of adding an admin. Either an existing user was granted the Admin role
/// immediately, or an invitation was sent to a brand-new email.
/// </summary>
public record AddAdminResult
{
    /// <summary>
    /// True when an invitation email was sent (no user existed yet);
    /// false when an existing user was granted the Admin role directly.
    /// </summary>
    public bool Invited { get; set; }

    /// <summary>
    /// The id of the user that was granted the Admin role, when <see cref="Invited"/> is false.
    /// </summary>
    public Guid? UserId { get; set; }
}
