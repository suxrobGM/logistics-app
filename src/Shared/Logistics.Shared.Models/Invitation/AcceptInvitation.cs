namespace Logistics.Shared.Models;

/// <summary>
/// Input for accepting an invitation.
/// </summary>
public record AcceptInvitation
{
    /// <summary>
    /// The invitation token from the email link.
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// First name for new user account.
    /// </summary>
    public required string FirstName { get; set; }

    /// <summary>
    /// Last name for new user account.
    /// </summary>
    public required string LastName { get; set; }

    /// <summary>
    /// Password for new user account.
    /// </summary>
    public required string Password { get; set; }
}
