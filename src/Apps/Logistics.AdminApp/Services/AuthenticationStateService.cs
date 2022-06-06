using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Logistics.AdminApp.Services;

public class AuthenticationStateService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly NavigationManager _navigationManager;
    private readonly IConfiguration _configuration;

    public AuthenticationStateService(
        AuthenticationStateProvider authenticationStateProvider,
        NavigationManager navigationManager,
        IConfiguration configuration)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _configuration = configuration;
        _navigationManager = navigationManager;
    }

    public async Task ReevaluateAuthenticationStateAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var usersIds = _configuration.GetRequiredSection("AllowedUsers").Get<string[]>();
        var userExternalId = GetId(authState.User);

        if (usersIds.Contains(userExternalId))
        {
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.Role, "admin"));
            authState.User.AddIdentity(identity);
        }
    }

    public static string? GetId(ClaimsPrincipal user)
    {
        return user?.Claims?.FirstOrDefault(i => i.Type == ClaimTypes.NameIdentifier)?.Value;
    }
}
