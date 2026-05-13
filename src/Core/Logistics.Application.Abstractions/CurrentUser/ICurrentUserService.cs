namespace Logistics.Application.Abstractions.CurrentUser;

/// <summary>
/// Service for accessing the current authenticated user's information.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's ID from the authentication context.
    /// </summary>
    /// <returns>The user's GUID if authenticated, null otherwise.</returns>
    Guid? GetUserId();

    /// <summary>
    /// Gets the current user's full name from the authentication context.
    /// </summary>
    /// <returns>The user's full name if available, "Unknown" otherwise.</returns>
    string GetUserName();

    /// <summary>
    /// Gets the IP address of the current request, or null when no HTTP context is present
    /// (e.g., background jobs, the DbMigrator).
    /// </summary>
    string? IpAddress { get; }

    /// <summary>
    /// Gets the User-Agent header of the current request, or null when no HTTP context is present.
    /// </summary>
    string? UserAgent { get; }
}
