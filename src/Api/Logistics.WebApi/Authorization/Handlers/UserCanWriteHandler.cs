using Logistics.Domain.ValueObjects;
using Logistics.WebApi.Authorization.Requirements;

namespace Logistics.WebApi.Authorization.Handlers;

public class UserCanWriteHandler : AuthorizationHandler<UserCanWriteRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserCanWriteRequirement requirement)
    {
        if (context.User.IsInRole(UserRole.Admin))
        {
            context.Succeed(requirement);
        }
    
        return Task.CompletedTask;
    }
}