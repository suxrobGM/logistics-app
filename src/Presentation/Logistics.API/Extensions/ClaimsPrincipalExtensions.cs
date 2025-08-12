using System.Security.Claims;

namespace Logistics.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool HasOneTheseRoles(this ClaimsPrincipal userIdentity, params string[] roles)
    {
        return roles.Any(userIdentity.IsInRole);
    }

    public static string? GetRole(this ClaimsPrincipal userIdentity)
    {
        return userIdentity.Claims.FirstOrDefault(i => i.Type == ClaimTypes.Role)?.Value;
    }

    public static Guid GetUserId(this ClaimsPrincipal userIdentity)
    {
        var userIdClaim = userIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
