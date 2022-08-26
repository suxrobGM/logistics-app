using Logistics.Domain.Shared;
using Logistics.WebApi.Authorization.Requirements;

namespace Logistics.WebApi.Authorization.Handlers;

public class TenantCanReadDisplayNameOnlyHandler : AuthorizationHandler<TenantCanReadDisplayNameOnlyRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, TenantCanReadDisplayNameOnlyRequirement requirement)
    {
        if (context.User.IsInRole(AppRoles.Admin) ||
            context.User.IsInRole(TenantRoles.Owner) ||
            context.User.IsInRole(TenantRoles.Manager) ||
            context.User.IsInRole(TenantRoles.Dispatcher))
        {
            context.Succeed(requirement);
        }
    
        return Task.CompletedTask;
    }
}