using System.Security.Claims;

namespace Logistics.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool HasOneTheseRoles(this ClaimsPrincipal userIdentity, IEnumerable<string> roles)
    {
        return roles.Any(role => userIdentity.IsInRole(role));
    }
}
