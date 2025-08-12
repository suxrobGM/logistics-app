namespace Logistics.DriverApp.Services.Authentication;

/// <summary>
/// A storage mechanism for storing the access token and refresh token.
/// </summary>
public interface ITokenStorage
{
    /// <summary>
    /// Retrieves access token and refresh token from the secure cache.
    /// </summary>
    /// <returns>The <see cref="TokenInfo"/> data if found from the cache otherwise null</returns>
    Task<TokenInfo?> GetTokenAsync();

    /// <summary>
    /// Saves access token, access token expiration and refresh tokens to the secure cache.
    /// </summary>
    /// <param name="accessToken">Access Token</param>
    /// <param name="accessTokenExpiration">Access Token Expiration</param>
    /// <param name="refreshToken">Refresh Token</param>
    Task SaveTokenAsync(string accessToken, DateTimeOffset accessTokenExpiration, string refreshToken);

    /// <summary>
    /// Clears all tokens from the secure cache
    /// </summary>
    void ClearTokenFromCache();
}
