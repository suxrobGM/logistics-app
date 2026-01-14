using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Identity.Claims;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace Logistics.Application.Queries;

internal sealed class GetCurrentUserPermissionsHandler(
    UserManager<User> userManager,
    RoleManager<AppRole> roleManager,
    ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetCurrentUserPermissionsQuery, Result<string[]>>
{
    public async Task<Result<string[]>> Handle(
        GetCurrentUserPermissionsQuery req, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(req.UserId.ToString());

        if (user is null)
        {
            return Result<string[]>.Fail($"Could not find a user with ID '{req.UserId}'");
        }

        var permissions = new HashSet<string>();

        // Get permissions from app roles
        await AddAppRolePermissionsAsync(user, permissions);

        // Get permissions from tenant roles if tenant context is available
        if (req.TenantId.HasValue)
        {
            await AddTenantRolePermissionsAsync(req.TenantId.Value, req.UserId, permissions);
        }

        return Result<string[]>.Ok(permissions.ToArray());
    }

    private async Task AddAppRolePermissionsAsync(User user, HashSet<string> permissions)
    {
        var appRoles = await userManager.GetRolesAsync(user);

        foreach (var roleName in appRoles)
        {
            var role = await roleManager.FindByNameAsync(roleName);

            if (role is null)
            {
                continue;
            }

            var claims = await roleManager.GetClaimsAsync(role);
            foreach (var claim in claims.Where(c => c.Type == CustomClaimTypes.Permission))
            {
                permissions.Add(claim.Value);
            }
        }
    }

    private async Task AddTenantRolePermissionsAsync(
        Guid tenantId, Guid userId, HashSet<string> permissions)
    {
        await tenantUow.SetCurrentTenantByIdAsync(tenantId);
        var employee = await tenantUow.Repository<Employee>().GetByIdAsync(userId);

        if (employee is null)
        {
            return;
        }

        var tenantRoleClaimRepository = tenantUow.Repository<TenantRoleClaim>();

        foreach (var tenantRole in employee.Roles)
        {
            var roleClaims = await tenantRoleClaimRepository.GetListAsync(i => i.RoleId == tenantRole.Id);
            foreach (var claim in roleClaims.Where(c => c.ClaimType == CustomClaimTypes.Permission))
            {
                permissions.Add(claim.ClaimValue);
            }
        }
    }
}
