using System.Security.Claims;
using Logistics.Application.Queries;
using Logistics.Shared.Identity.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;

namespace Logistics.API.Authorization;

internal class PermissionHandler(IServiceProvider serviceProvider, IMemoryCache cache)
    : AuthorizationHandler<PermissionRequirement>
{
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(5);

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
        var cacheKey = $"permissions:{userId}:{tenantId?.ToString() ?? "no-tenant"}";

        if (!cache.TryGetValue(cacheKey, out HashSet<string>? permissions))
        {
            permissions = await FetchPermissionsAsync(userId, tenantId);
            cache.Set(cacheKey, permissions, CacheExpiry);
        }

        if (permissions?.Contains(requirement.Permission) == true)
        {
            context.Succeed(requirement);
        }
    }

    private async Task<HashSet<string>> FetchPermissionsAsync(Guid userId, Guid? tenantId)
    {
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new GetCurrentUserPermissionsQuery
        {
            UserId = userId,
            TenantId = tenantId
        });

        return result.IsSuccess ? result.Value!.ToHashSet() : [];
    }
}
