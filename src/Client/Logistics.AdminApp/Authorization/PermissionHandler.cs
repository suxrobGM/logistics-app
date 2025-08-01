using Logistics.Shared.Identity.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Logistics.AdminApp.Authorization;

internal class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        PermissionRequirement requirement)
    {
        var permissionClaim = context.User.Claims.FirstOrDefault(i => i.Type == CustomClaimTypes.Permission);
        var hasRequiredPermission = permissionClaim?.Value.Contains(requirement.Permission) ?? false;

        if (hasRequiredPermission)
        {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}
