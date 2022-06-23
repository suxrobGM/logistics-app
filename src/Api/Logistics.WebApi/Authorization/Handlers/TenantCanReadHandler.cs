using Logistics.Domain.ValueObjects;
using Logistics.WebApi.Authorization.Requirements;

namespace Logistics.WebApi.Authorization.Handlers;

public class TenantCanReadHandler : AuthorizationHandler<TenantCanReadRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, TenantCanReadRequirement requirement)
    {
        if (context.User.IsInRole(UserRole.Admin))
        {
            context.Succeed(requirement);
        }
    
        return Task.CompletedTask;
    }
}