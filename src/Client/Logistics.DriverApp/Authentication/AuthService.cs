using IdentityModel.OidcClient;

namespace Logistics.DriverApp.Authentication;

public class AuthService : IAuthService
{
    private readonly OidcClient _oidcClient;

    public AuthService(OidcClientOptions options, IdentityModel.OidcClient.Browser.IBrowser browser)
    {

        _oidcClient = new OidcClient(options);
        _oidcClient.Options.Browser = browser;
        Browser = browser;
    }

    public IdentityModel.OidcClient.Browser.IBrowser Browser { get; }

    public Task<LoginResult> LoginAsync()
    {
        return _oidcClient.LoginAsync();
    }

    public Task<LogoutResult> LogoutAsync()
    {
        return _oidcClient.LogoutAsync();
    }
}
