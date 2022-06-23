using Logistics.Domain.ValueObjects;
using Logistics.WebApi.Authorization.Requirements;

namespace Logistics.WebApi.Authorization.Handlers;

public class CargoCanWriteHandler : AuthorizationHandler<CargoCanWriteRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CargoCanWriteRequirement requirement)
    {
        if (context.User.IsInRole(UserRole.Admin) ||
            context.User.IsInRole(EmployeeRole.Owner) ||
            context.User.IsInRole(EmployeeRole.Manager) ||
            context.User.IsInRole(EmployeeRole.Dispatcher))
        {
            context.Succeed(requirement);
        }
    
        return Task.CompletedTask;
    }
}