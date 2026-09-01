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
    /// The caller's own tenant from their token, which is not necessarily the tenant the request
    /// resolved to - a multi-tenant user may target another via the <c>X-Tenant</c> header.
    /// </summary>
    Guid? GetTenantId();

    /// <summary>Whether the caller holds any of these roles. False when there is no HTTP context.</summary>
    bool IsInRole(params string[] roles);

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
