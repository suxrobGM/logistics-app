using System.Net;
using Hangfire.Dashboard;
using Logistics.Shared.Identity.Roles;

namespace Logistics.API.Authorization;

/// <summary>
/// Gate for the Hangfire dashboard. In development, loopback requests are allowed so the local
/// dashboard stays reachable without a token. Otherwise the caller must be authenticated and hold
/// the <see cref="AppRoles.SuperAdmin" /> role.
/// </summary>
/// <remarks>
/// Role check uses <see cref="System.Security.Claims.ClaimsPrincipal.IsInRole" />: the API's JWT
/// bearer maps the token's <c>role</c> claim onto <see cref="System.Security.Claims.ClaimTypes.Role" />
/// (JwtBearer default MapInboundClaims), which is exactly how the rest of the API gates roles
/// (<c>[Authorize(Roles = ...)]</c>, <c>HasOneTheseRoles</c>).
/// </remarks>
internal sealed class HangfireDashboardAuthorizationFilter(bool isDevelopment) : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        if (isDevelopment &&
            httpContext.Connection.RemoteIpAddress is { } remoteIp &&
            IPAddress.IsLoopback(remoteIp))
        {
            return true;
        }

        return httpContext.User.Identity?.IsAuthenticated == true &&
               httpContext.User.IsInRole(AppRoles.SuperAdmin);
    }
}
