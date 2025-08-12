using Result = Duende.IdentityModel.OidcClient.Result;

namespace Logistics.DriverApp.Services.Authentication;

public class AuthResult : Result
{
    public string? AccessToken { get; internal set; }
}
