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

        /// <summary>
        /// Guards a single-target-tenant endpoint (e.g. <c>PUT /tenants/{id}</c>) against a caller
        /// acting on a tenant other than their own. A platform SuperAdmin/Admin may act on any
        /// tenant; anyone else must have <paramref name="tenantId"/> equal to their own JWT tenant
        /// claim. Unlike <c>Permission.Tenant.Manage</c> alone, this actually checks the route's
        /// target against the caller - the policy is also granted to every ordinary tenant's own
        /// Owner role, so without this check any Owner could act on any other tenant.
        /// </summary>
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
