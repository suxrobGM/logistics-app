using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Identity.Claims;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

            // Check if user is a CustomerUser and add portal permissions
            await AddCustomerUserPermissionsAsync(req.TenantId.Value, req.UserId, permissions);
        }

        return Result<string[]>.Ok(permissions.ToArray());
    }

    private async Task AddAppRolePermissionsAsync(User user, HashSet<string> permissions)
    {
        // Get roles from ASP.NET Identity (stored in AspNetUserRoles table)
        var roleNames = await userManager.GetRolesAsync(user);

        foreach (var roleName in roleNames)
        {
            var appRole = await roleManager.FindByNameAsync(roleName);
            if (appRole is null)
            {
                continue;
            }

            var claims = await roleManager.GetClaimsAsync(appRole);
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

        if (employee.Role is null)
        {
            return;
        }

        var roleClaims = await tenantRoleClaimRepository.GetListAsync(i => i.RoleId == employee.Role.Id);
        foreach (var claim in roleClaims.Where(c => c.ClaimType == CustomClaimTypes.Permission))
        {
            permissions.Add(claim.ClaimValue);
        }
    }

    private async Task AddCustomerUserPermissionsAsync(
        Guid tenantId, Guid userId, HashSet<string> permissions)
    {
        await tenantUow.SetCurrentTenantByIdAsync(tenantId);
        var customerUserExists = await tenantUow.Repository<CustomerUser>().Query()
            .AnyAsync(cu => cu.UserId == userId && cu.IsActive);

        if (customerUserExists)
        {
            // Grant all portal permissions to active customer users
            permissions.Add(Permission.Portal.Access);
            permissions.Add(Permission.Portal.ViewLoads);
            permissions.Add(Permission.Portal.ViewInvoices);
            permissions.Add(Permission.Portal.ViewDocuments);
        }
    }
}
