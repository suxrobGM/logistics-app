using Logistics.Domain.Shared;
using Logistics.WebApi.Authorization.Requirements;

namespace Logistics.WebApi.Authorization.Handlers;

public class UserCanWriteHandler : AuthorizationHandler<UserCanWriteRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserCanWriteRequirement requirement)
    {
        if (context.User.IsInRole(AppRoles.Admin))
        {
            context.Succeed(requirement);
        }
    
        return Task.CompletedTask;
    }
}