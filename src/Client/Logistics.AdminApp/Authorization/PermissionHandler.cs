using Logistics.AdminApp.Services;
using Microsoft.AspNetCore.Authorization;

namespace Logistics.AdminApp.Authorization;

internal class PermissionHandler(PermissionService permissionService) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        await permissionService.LoadPermissionsAsync();
        var hasRequiredPermission = permissionService.HasPermission(requirement.Permission);

        if (hasRequiredPermission)
        {
            context.Succeed(requirement);
        }
    }
}
