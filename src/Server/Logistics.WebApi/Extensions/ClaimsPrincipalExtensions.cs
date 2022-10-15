using System.Security.Claims;

namespace Logistics.WebApi.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool HasOneTheseRoles(this ClaimsPrincipal userIdentity, IEnumerable<string> roles)
    {
        return roles.Any(role => userIdentity.IsInRole(role));
    }
}
