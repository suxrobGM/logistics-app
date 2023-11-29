using Logistics.Shared.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Logistics.AdminApp.Authorization;

internal class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        PermissionRequirement requirement)
    {
        var claims = context.User.Claims.ToArray();
        var permissions = context.User.Claims.Where(x => x.Type == CustomClaimTypes.Permission &&
                                                          x.Value == requirement.Permission).ToArray();
        if (permissions.Any())
        {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}
