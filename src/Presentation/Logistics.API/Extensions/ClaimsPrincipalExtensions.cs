using System.Security.Claims;

namespace Logistics.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal userIdentity)
    {
        public bool HasOneTheseRoles(params string[] roles)
        {
            return roles.Any(userIdentity.IsInRole);
        }

        public string? GetRole()
        {
            return userIdentity.Claims.FirstOrDefault(i => i.Type == ClaimTypes.Role)?.Value;
        }

        public Guid? GetUserId()
        {
            var userIdClaim = userIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
