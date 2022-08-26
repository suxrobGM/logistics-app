using Logistics.Domain.Shared;
using Logistics.WebApi.Authorization.Requirements;

namespace Logistics.WebApi.Authorization.Handlers;

public class UserCanReadHandler : AuthorizationHandler<UserCanReadRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserCanReadRequirement requirement)
    {
        if (context.User.IsInRole(AppRoles.Admin) ||
            context.User.IsInRole(TenantRoles.Owner) ||
            context.User.IsInRole(TenantRoles.Manager))
        {
            context.Succeed(requirement);
        }
    
        return Task.CompletedTask;
    }
}