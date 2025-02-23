using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Duende.IdentityModel;
using Duende.IdentityModel.OidcClient;
using Logistics.Shared.Consts.Claims;
using IBrowser = Duende.IdentityModel.OidcClient.Browser.IBrowser;
using Result = Duende.IdentityModel.OidcClient.Result;

namespace Logistics.DriverApp.Services.Authentication;

public class AuthService : IAuthService
{
    private readonly OidcClient _oidcClient;
    private readonly ITokenStorage _tokenStorage;

    public AuthService(
        OidcClientOptions options, 
        IBrowser browser,
        ITokenStorage tokenStorage)
    {
#if DEBUG
        options.HttpClientFactory = (_) => new System.Net.Http.HttpClient(InsecureHttpsClient.GetPlatformMessageHandler());
#endif
        _oidcClient = new OidcClient(options)
        {
            Options =
            {
                Browser = browser
            }
        };
        _tokenStorage = tokenStorage;
        Options = options;
    }
    
    public UserInfo? User { get; private set; }
    public OidcClientOptions Options { get; }
    
    public async Task<bool> CanAutoLoginAsync()
    {
        var tokenInfo = await _tokenStorage.GetTokenAsync();
        return tokenInfo is not null;
    }

    public async Task<AuthResult> LoginAsync()
    {
        AuthResult loginResult;
        var tokenInfo = await _tokenStorage.GetTokenAsync();
        var isTokenExpired = IsAccessTokenExpired(tokenInfo);

        if (tokenInfo == null) // first time login
        {
            loginResult = await PerformLoginAsync();
        }
        else if (isTokenExpired) // just refresh the access token
        {
            loginResult = await RefreshAccessTokenAsync(tokenInfo.RefreshToken);

            if (loginResult.IsError)
                await PerformLoginAsync();
        }
        else // use the existing unexpired access token
        {
            loginResult = new AuthResult() { AccessToken = tokenInfo.AccessToken };
        }

        if (loginResult.AccessToken != null)
        {
            User = ParseUserData(loginResult.AccessToken);
        }
        
        return loginResult;
    }

    private async Task<AuthResult> PerformLoginAsync()
    {
        var loginResult = await _oidcClient.LoginAsync();
        
        if (!loginResult.IsError)
        {
            await _tokenStorage.SaveTokenAsync(loginResult.AccessToken, loginResult.AccessTokenExpiration, loginResult.RefreshToken);
        }
        
        return new AuthResult()
        {
            Error = loginResult.Error,
            ErrorDescription = loginResult.ErrorDescription,
            AccessToken = loginResult.AccessToken
        };
    }

    private async Task<AuthResult> RefreshAccessTokenAsync(string refreshToken)
    {
        var refreshTokenResult = await _oidcClient.RefreshTokenAsync(refreshToken);
        
        if (!refreshTokenResult.IsError)
        {
            await _tokenStorage.SaveTokenAsync(refreshTokenResult.AccessToken, refreshTokenResult.AccessTokenExpiration, refreshTokenResult.RefreshToken);
        }
        
        return new AuthResult()
        {
            Error = refreshTokenResult.Error,
            ErrorDescription = refreshTokenResult.ErrorDescription,
            AccessToken = refreshTokenResult.AccessToken
        };
    }

    public async Task<Result> LogoutAsync()
    {
        var result = await _oidcClient.LogoutAsync();
        User = null;
        _tokenStorage.ClearTokenFromCache();
        return result;
    }

    private static UserInfo ParseUserData(string accessToken)
    {
        var userIdentity = new UserInfo();
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadToken(accessToken) as JwtSecurityToken;
        var claims = jwtSecurityToken?.Claims.ToList() ?? new List<Claim>();
        
        foreach (var claim in claims)
        {
            switch (claim.Type)
            {
                case JwtClaimTypes.Subject:
                    userIdentity.Id = claim.Value;
                    break;
                case JwtClaimTypes.GivenName:
                    userIdentity.FirstName = claim.Value;
                    break;
                case JwtClaimTypes.FamilyName:
                    userIdentity.LastName = claim.Value;
                    break;
                case CustomClaimTypes.Role:
                    userIdentity.Roles.Add(claim.Value);
                    break;
                case CustomClaimTypes.Permission:
                    userIdentity.Permissions.Add(claim.Value);
                    break;
                case CustomClaimTypes.Tenant:
                    userIdentity.TenantId = claim.Value;
                    break;
            }
        }
        return userIdentity;
    }
    
    private static bool IsAccessTokenExpired(TokenInfo? tokenInfo)
    {
        if (tokenInfo is null)
            return false;
            
        var accessTokenExpiration = tokenInfo.AccessTokenExpiration;
        return accessTokenExpiration.UtcDateTime <= DateTime.UtcNow;
    }
}
