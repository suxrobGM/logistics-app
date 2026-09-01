using Logistics.Domain.Exceptions;
using Logistics.Shared.Identity.Claims;
using Logistics.Shared.Identity.Roles;
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

        public Guid? GetTenantId()
        {
            var tenantClaim = userIdentity.FindFirst(CustomClaimTypes.Tenant)?.Value;
            return Guid.TryParse(tenantClaim, out var tenantId) ? tenantId : null;
        }

        /// <summary>Rejects access to another tenant unless the caller is a platform admin.</summary>
        public void EnsureOwnsTenant(Guid tenantId)
        {
            if (userIdentity.HasOneTheseRoles(AppRoles.SuperAdmin, AppRoles.Admin))
            {
                return;
            }

            if (userIdentity.GetTenantId() == tenantId)
            {
                return;
            }

            throw new TenantAccessDeniedException("You do not have access to this tenant.");
        }
    }
}
