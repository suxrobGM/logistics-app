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
    /// Gets the tenant the current user belongs to, from their token. This is the caller's own
    /// tenant, which is not necessarily the tenant the request resolved to - a multi-tenant user
    /// may target another via the <c>X-Tenant</c> header.
    /// </summary>
    /// <returns>The tenant's GUID if the token carries the claim, null otherwise.</returns>
    Guid? GetTenantId();

    /// <summary>
    /// Whether the current user holds any of the given roles.
    /// </summary>
    /// <returns>False when there is no HTTP context (background jobs, the DbMigrator).</returns>
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
