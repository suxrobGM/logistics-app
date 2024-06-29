using Logistics.Shared.Consts.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Logistics.API.Authorization;

internal class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        PermissionRequirement requirement)
    {
        var permissions = context.User.Claims.Where(x => x.Type == CustomClaimTypes.Permission &&
                                                          x.Value == requirement.Permission);
        if (permissions.Any())
        {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}
