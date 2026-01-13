#nullable enable

using System.Security.Claims;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.IdentityServer.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using CustomClaimTypes = Logistics.Shared.Identity.Claims.CustomClaimTypes;

namespace Logistics.IdentityServer.Services;

public class UserCustomClaimsFactory(
    UserManager<User> userManager,
    RoleManager<AppRole> roleManager,
    IOptions<IdentityOptions> options,
    IHttpContextAccessor httpContextAccessor,
    ITenantUnitOfWork tenantUow)
    : UserClaimsPrincipalFactory<User, AppRole>(userManager, roleManager, options)
{
    private readonly HttpContext _httpContext = httpContextAccessor.HttpContext!;
    private readonly RoleManager<AppRole> _roleManager = roleManager;
    private readonly UserManager<User> _userManager = userManager;

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
    {
        var claimsIdentity = await base.GenerateClaimsAsync(user);
        var tenantId = _httpContext.GetTenantId() ?? user.TenantId;

        AddProfileClaims(claimsIdentity, user);
        await AddAppRoleClaimsAsync(claimsIdentity, user);

        if (tenantId is null)
        {
            return claimsIdentity;
        }

        await tenantUow.SetCurrentTenantByIdAsync(tenantId.Value);
        var employee = await tenantUow.Repository<Employee>().GetByIdAsync(user.Id);

        claimsIdentity.AddClaim(new Claim(CustomClaimTypes.Tenant, tenantId.Value.ToString()));
        await AddTenantRoleClaimsAsync(claimsIdentity, employee);
        return claimsIdentity;
    }

    private async Task AddAppRoleClaimsAsync(ClaimsIdentity claimsIdentity, User user)
    {
        var appRoles = await _userManager.GetRolesAsync(user);

        foreach (var roleName in appRoles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);

            if (role is null)
            {
                continue;
            }

            var claims = await _roleManager.GetClaimsAsync(role);
            claimsIdentity.AddClaims(claims);
        }
    }

    private async Task AddTenantRoleClaimsAsync(ClaimsIdentity claimsIdentity, Employee? employee)
    {
        if (employee is null)
        {
            return;
        }

        var tenantRoleClaimRepository = tenantUow.Repository<TenantRoleClaim>();

        foreach (var tenantRole in employee.Roles)
        {
            var roleClaims = await tenantRoleClaimRepository.GetListAsync(i => i.RoleId == tenantRole.Id);
            claimsIdentity.AddClaims(roleClaims.Select(i => new Claim(i.ClaimType, i.ClaimValue)));
            claimsIdentity.AddClaim(new Claim(CustomClaimTypes.Role, tenantRole.Name));
        }
    }

    private static void AddProfileClaims(ClaimsIdentity claimsIdentity, User user)
    {
        if (!string.IsNullOrEmpty(user.FirstName))
        {
            claimsIdentity.AddClaim(new Claim("given_name", user.FirstName));
        }

        if (!string.IsNullOrEmpty(user.LastName))
        {
            claimsIdentity.AddClaim(new Claim("family_name", user.LastName));
        }
    }
}
