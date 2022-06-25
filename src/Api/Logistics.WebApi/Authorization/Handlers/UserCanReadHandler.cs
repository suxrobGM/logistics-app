using Logistics.Domain.ValueObjects;
using Logistics.WebApi.Authorization.Requirements;

namespace Logistics.WebApi.Authorization.Handlers;

public class UserCanReadHandler : AuthorizationHandler<UserCanReadRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserCanReadRequirement requirement)
    {
        if (context.User.IsInRole(UserRole.Admin) ||
            context.User.IsInRole(EmployeeRole.Owner) ||
            context.User.IsInRole(EmployeeRole.Manager))
        {
            context.Succeed(requirement);
        }
    
        return Task.CompletedTask;
    }
}