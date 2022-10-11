using IdentityModel.OidcClient;
using IBrowser = IdentityModel.OidcClient.Browser.IBrowser;

namespace Logistics.DriverApp.Authentication;

public interface IAuthService
{
    string? AccessToken { get; }
    UserIdentity? User { get; }
    IBrowser Browser { get; }
    OidcClientOptions Options { get; }
    Task<LoginResult> LoginAsync();
    Task<LogoutResult> LogoutAsync();
}
