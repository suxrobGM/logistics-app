using IdentityModel.OidcClient;

namespace Logistics.DriverApp.Services.Authentication;

public class AuthResult : Result
{
    public string? AccessToken { get; internal set; }
}