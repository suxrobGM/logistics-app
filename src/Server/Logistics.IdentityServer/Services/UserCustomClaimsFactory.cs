#nullable enable
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Logistics.Domain.Persistence;
using CustomClaimTypes = Logistics.Shared.Claims.CustomClaimTypes;

namespace Logistics.IdentityServer.Services;

public class UserCustomClaimsFactory : UserClaimsPrincipalFactory<User, AppRole>
{
    private readonly HttpContext _httpContext;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly UserManager<User> _userManager;
    private readonly ITenantRepository _tenantRepository;

    public UserCustomClaimsFactory(
        UserManager<User> userManager, 
        RoleManager<AppRole> roleManager, 
        IOptions<IdentityOptions> options,
        IHttpContextAccessor httpContextAccessor,
        ITenantRepository tenantRepository) 
        : base(userManager, roleManager, options)
    {
        _httpContext = httpContextAccessor.HttpContext!;
        _roleManager = roleManager;
        _userManager = userManager;
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
    {
        var claimsIdentity = await base.GenerateClaimsAsync(user);
        var tenantId = _httpContext.GetTenantId();
        
        await AddAppRoleClaims(claimsIdentity, user);

        if (string.IsNullOrEmpty(tenantId)) 
            return claimsIdentity;
        
        var employee = await _tenantRepository.GetAsync<Employee>(user.Id);

        AddTenantRoles(claimsIdentity, employee);
        await AddTenantRoleClaims(claimsIdentity, employee);
        
        claimsIdentity.AddClaim(new Claim(CustomClaimTypes.Tenant, tenantId));
        return claimsIdentity;
    }

    private async Task AddAppRoleClaims(ClaimsIdentity claimsIdentity, User user)
    {
        var appRoles = await _userManager.GetRolesAsync(user);
        
        if (appRoles == null)
            return;

        foreach (var roleName in appRoles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            var claims = await _roleManager.GetClaimsAsync(role);

            if (claims == null) 
                continue;

            claimsIdentity.AddClaims(claims);
        }
    }

    private async Task AddTenantRoleClaims(ClaimsIdentity claimsIdentity, Employee? employee)
    {
        if (employee == null)
            return;

        foreach (var tenantRole in employee.Roles)
        {
            var roleClaims = await _tenantRepository.GetListAsync<TenantRoleClaim>(i => i.RoleId == tenantRole.Id);
            claimsIdentity.AddClaims(roleClaims.Select(i => i.ToClaim()));
        }
    }

    private void AddTenantRoles(ClaimsIdentity claimsIdentity, Employee? employee)
    {
        if (employee == null)
            return;
        
        foreach (var role in employee.Roles)
        {
            claimsIdentity.AddClaim(new Claim(CustomClaimTypes.Role, role.Name));
        }
    }
}