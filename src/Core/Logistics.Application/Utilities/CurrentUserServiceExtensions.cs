using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Domain.Exceptions;
using Logistics.Shared.Identity.Roles;

namespace Logistics.Application.Utilities;

public static class CurrentUserServiceExtensions
{
    /// <summary>
    /// Checks whether the caller is a tenant driver, whose reads are limited to their own records.
    /// Platform admins keep full visibility even when they also hold the driver role.
    /// </summary>
    public static bool IsTenantDriver(this ICurrentUserService currentUserService)
    {
        return currentUserService.IsInRole(TenantRoles.Driver) &&
               !currentUserService.IsInRole(AppRoles.SuperAdmin, AppRoles.Admin);
    }

    /// <summary>Rejects access to another tenant's data unless the caller is a platform admin.</summary>
    public static void EnsureOwnsTenant(this ICurrentUserService currentUserService, Guid tenantId, string resource)
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
