using System.Security.Claims;
using Logistics.Application.Modules.IdentityAccess.Users.Services;
using Logistics.Shared.Identity.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Logistics.API.Authorization;

internal class PermissionHandler(IServiceProvider serviceProvider)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var tenantClaim = context.User.FindFirst(CustomClaimTypes.Tenant)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId) || userId == Guid.Empty)
        {
            return;
        }

        var tenantId = Guid.TryParse(tenantClaim, out var tid) ? tid : (Guid?)null;

        // The handler is a singleton, so the scoped permission service needs its own scope. The
        // cache behind it is a singleton, which is what makes the lookup shared across requests.
        using var scope = serviceProvider.CreateScope();
        var userPermissions = scope.ServiceProvider.GetRequiredService<IUserPermissionService>();

        if (await userPermissions.HasPermissionAsync(userId, tenantId, requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}
