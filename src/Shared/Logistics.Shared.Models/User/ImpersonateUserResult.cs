namespace Logistics.Shared.Models;

/// <summary>
/// Result of a successful impersonation request.
/// </summary>
public class ImpersonateUserResult
{
    /// <summary>
    /// The one-time impersonation token.
    /// </summary>
    public required string ImpersonationToken { get; set; }

    /// <summary>
    /// URL to redirect the admin to, which will sign them in as the target user.
    /// </summary>
    public required string ImpersonationUrl { get; set; }
}
