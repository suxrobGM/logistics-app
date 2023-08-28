using IdentityModel.OidcClient;
using System.Security.Claims;
using IBrowser = IdentityModel.OidcClient.Browser.IBrowser;

namespace Logistics.DriverApp.Services.Authentication;

public class AuthService : IAuthService
{
    private readonly OidcClient _oidcClient;

    public AuthService(OidcClientOptions options, IBrowser browser)
    {
#if DEBUG
        options.HttpClientFactory = (options) =>
        {
            var handler = new HttpsClientHandlerService();
            return new HttpClient(handler.GetPlatformMessageHandler());
        };
#endif
        _oidcClient = new OidcClient(options);
        _oidcClient.Options.Browser = browser;
        Browser = browser;
        Options = options;
    }

    public string? AccessToken { get; private set; }
    public IBrowser Browser { get; }
    public UserIdentity? User { get; private set; }

    public OidcClientOptions Options { get; }

    public async Task<LoginResult> LoginAsync()
    {
        var result = await _oidcClient.LoginAsync();

        if (!result.IsError)
        {
            AccessToken = result.AccessToken;
            User = ParseUserIdentity(result.User.Claims);
        }

        return result;
    }

    public async Task<LogoutResult> LogoutAsync()
    {
        var result = await _oidcClient.LogoutAsync();

        if (!result.IsError)
        {
            AccessToken = null;
            User = null;
        }

        return result;
    }

    private static UserIdentity ParseUserIdentity(IEnumerable<Claim> claims)
    {
        var userIdentity = new UserIdentity();
        foreach (var claim in claims)
        {
            switch (claim.Type)
            {
                case "name":
                    userIdentity.UserName = claim.Value;
                    break;

                case "sub":
                    userIdentity.Id = claim.Value;
                    break;

                case "role":
                    userIdentity.Roles.Add(claim.Value);
                    break;

                case "permission":
                    userIdentity.Roles.Add(claim.Value);
                    break;
            }
        }
        return userIdentity;
    }
}
