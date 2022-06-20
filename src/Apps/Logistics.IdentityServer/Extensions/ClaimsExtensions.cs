#nullable enable
using System.Security.Claims;

namespace Logistics.IdentityServer;

public static class ClaimsExtensions
{
    public static string? GetRole(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.Claims.FirstOrDefault(i => i.Type == "role")?.Value;
    }

    public static bool TryAdd(this IList<Claim> claims, string claimType, string? claimValue)
    {
        if (string.IsNullOrEmpty(claimValue)) 
            return false;
        
        claims.Add(new Claim(claimType, claimValue));
        return true;
    }
}