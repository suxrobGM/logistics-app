using Logistics.Application.Abstractions.Context;
using Logistics.Domain.Entities;
using Logistics.Shared.Identity.Roles;

namespace Logistics.Application.Common.Extensions;

public static class CurrentUserServiceExtensions
{
    extension(ICurrentUserService currentUserService)
    {
        /// <summary>
        /// Checks whether the caller is a tenant driver, whose reads are limited to their own records.
        /// Platform admins keep full visibility even when they also hold the driver role.
        /// </summary>
        public bool IsTenantDriver()
        {
            return currentUserService.IsInRole(TenantRoles.Driver) &&
                   !currentUserService.IsInRole(AppRoles.SuperAdmin, AppRoles.Admin);
        }

        /// <summary>A tenant driver may only act on a truck they drive; other callers are not restricted.</summary>
        public bool CanDrive(Truck? truck)
        {
            if (!currentUserService.IsTenantDriver())
            {
                return true;
            }

            return currentUserService.GetUserId() is { } userId && truck?.IsDrivenBy(userId) == true;
        }

        /// <summary>Rejects access to another tenant's data unless the caller is a platform admin.</summary>
        public void EnsureOwnsTenant(Guid tenantId, string resource)
        {
            if (currentUserService.IsInRole(AppRoles.SuperAdmin, AppRoles.Admin))
            {
                return;
            }

            if (currentUserService.GetTenantId() == tenantId)
            {
                return;
            }

            throw new TenantAccessDeniedException($"You do not have access to this {resource}.");
        }
    }

}
