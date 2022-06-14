using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Logistics.OfficeApp.Services;

public class AuthenticationStateService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IApiClient _apiClient;
    private readonly IHttpContextAccessor _context;

    public AuthenticationStateService(
        AuthenticationStateProvider authenticationStateProvider,
        IApiClient apiClient,
        IHttpContextAccessor context)
    {
        _apiClient = apiClient;
        _context = context;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task ReevaluateAuthenticationStateAsync()
    {
        SetTenant();
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var userExternalId = authState.User.GetId();
        var userRole = await _apiClient.GetEmployeeRoleAsync(userExternalId!);

        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim(ClaimTypes.Role, userRole.Role));
        authState.User.AddIdentity(identity);
    }

    private void SetTenant()
    {
        var tenantCookie = _context?.HttpContext?.Request?.Cookies["X-Tenant"];
        _apiClient.SetCurrentTenantId(tenantCookie);
    }
}
