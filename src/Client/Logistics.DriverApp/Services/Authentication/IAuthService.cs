using IdentityModel.OidcClient;
using Result = IdentityModel.OidcClient.Result;

namespace Logistics.DriverApp.Services.Authentication;

/// <summary>
/// Defines the contract for an authentication service using OIDC (OpenID Connect).
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Gets the authenticated user's information, including ID, roles, and permissions.
    /// </summary>
    UserInfo? User { get; }

    /// <summary>
    /// Gets the options used for the OIDC client.
    /// </summary>
    OidcClientOptions Options { get; }

    /// <summary>
    /// Attempts to authenticate the user using the OIDC code flow.
    /// Saves the acquired access token to secure storage for future use.
    /// For subsequent attempts, simply returns the token from the cache.
    /// Also capable of refreshing the access token automatically if it's expired.
    /// </summary>
    /// <returns>
    /// The task result contains an <see cref="AuthResult"/> object, which will include
    /// an access token if the login was successful; otherwise, it will contain an error message.
    /// </returns>
    Task<AuthResult> LoginAsync();

    /// <summary>
    /// Signs the user out and removes all tokens from secure storage.
    /// </summary>
    /// <returns>A <see cref="IdentityModel.OidcClient.Result"/> indicating the outcome.</returns>
    Task<Result> LogoutAsync();

    /// <summary>
    /// Checks if the service can automatically authenticate the user without invoking the browser.
    /// </summary>
    /// <returns>
    /// A <c>true</c> if automatic login is possible; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> CanAutoLoginAsync();
}
