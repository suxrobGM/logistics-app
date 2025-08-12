namespace Logistics.DriverApp.Services.Authentication;

public class TokenStorage : ITokenStorage
{
    private const string AccessTokenKey = "access_token";
    private const string AccessTokenExpirationKey = "access_token_expiration";
    private const string RefreshTokenKey = "refresh_token";

    public async Task<TokenInfo?> GetTokenAsync()
    {
        var accessToken = await SecureStorage.GetAsync(AccessTokenKey);
        var refreshToken = await SecureStorage.GetAsync(RefreshTokenKey);
        var accessTokenExpirationStr = await SecureStorage.GetAsync(AccessTokenExpirationKey);
        var parsedExpirationDate = DateTimeOffset.TryParse(accessTokenExpirationStr, out var accessTokenExpiration);

        if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken) && parsedExpirationDate)
        {
            return new TokenInfo(accessToken, accessTokenExpiration, refreshToken);
        }

        return null;
    }

    public async Task SaveTokenAsync(string accessToken, DateTimeOffset accessTokenExpiration, string refreshToken)
    {
        await SecureStorage.SetAsync(AccessTokenKey, accessToken);
        await SecureStorage.SetAsync(AccessTokenExpirationKey, accessTokenExpiration.ToString());
        await SecureStorage.SetAsync(RefreshTokenKey, refreshToken);
    }

    public void ClearTokenFromCache()
    {
        SecureStorage.Remove(AccessTokenKey);
        SecureStorage.Remove(AccessTokenExpirationKey);
        SecureStorage.Remove(RefreshTokenKey);
    }
}
