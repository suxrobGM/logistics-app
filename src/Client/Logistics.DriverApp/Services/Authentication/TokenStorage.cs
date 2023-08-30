namespace Logistics.DriverApp.Services.Authentication;

public class TokenStorage : ITokenStorage
{
    private const string ACCESS_TOKEN_KEY = "access_token";
    private const string ACCESS_TOKEN_EXPIRATION_KEY = "access_token_expiration";
    private const string REFRESH_TOKEN_KEY = "refresh_token";

    public async Task<TokenInfo?> GetTokenAsync()
    {
        var accessToken = await SecureStorage.GetAsync(ACCESS_TOKEN_KEY);
        var refreshToken = await SecureStorage.GetAsync(REFRESH_TOKEN_KEY);
        var accessTokenExpirationStr = await SecureStorage.GetAsync(ACCESS_TOKEN_EXPIRATION_KEY);
        var parsedExpirationDate = DateTimeOffset.TryParse(accessTokenExpirationStr, out var accessTokenExpiration);

        if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken) && parsedExpirationDate)
        {
            return new TokenInfo(accessToken, accessTokenExpiration, refreshToken);
        }

        return null;
    }

    public async Task SaveTokenAsync(string accessToken, DateTimeOffset accessTokenExpiration, string refreshToken)
    {
        await SecureStorage.SetAsync(ACCESS_TOKEN_KEY, accessToken);
        await SecureStorage.SetAsync(ACCESS_TOKEN_EXPIRATION_KEY, accessTokenExpiration.ToString());
        await SecureStorage.SetAsync(REFRESH_TOKEN_KEY, refreshToken);
    }

    public void ClearTokenFromCache()
    {
        SecureStorage.Remove(ACCESS_TOKEN_KEY);
        SecureStorage.Remove(ACCESS_TOKEN_EXPIRATION_KEY);
        SecureStorage.Remove(REFRESH_TOKEN_KEY);
    }
}