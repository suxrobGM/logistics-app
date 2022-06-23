using Logistics.Domain.ValueObjects;
using Logistics.WebApi.Authorization.Requirements;

namespace Logistics.WebApi.Authorization.Handlers;

public class TenantCanWriteHandler : AuthorizationHandler<TenantCanWriteRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, TenantCanWriteRequirement requirement)
    {
        if (context.User.IsInRole(UserRole.Admin))
        {
            context.Succeed(requirement);
        }
    
        return Task.CompletedTask;
    }
}