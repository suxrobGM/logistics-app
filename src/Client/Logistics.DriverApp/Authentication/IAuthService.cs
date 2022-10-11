using IdentityModel.OidcClient;

namespace Logistics.DriverApp.Authentication;

public interface IAuthService
{
    IdentityModel.OidcClient.Browser.IBrowser Browser { get; }
    Task<LoginResult> LoginAsync();
    Task<LogoutResult> LogoutAsync();
}
