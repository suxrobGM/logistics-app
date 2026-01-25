namespace Logistics.Application.Services;

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
}
