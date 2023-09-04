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
        var tenantIds = user.GetJoinedTenantsIds();
        var tenantId = _httpContext.GetTenantId() ?? tenantIds.FirstOrDefault();
        
        await AddAppRoleClaims(claimsIdentity, user);

        if (string.IsNullOrEmpty(tenantId)) 
            return claimsIdentity;

        _tenantRepository.SetTenantId(tenantId);
        var employee = await _tenantRepository.GetAsync<Employee>(user.Id);

        AddTenantIdsClaim(claimsIdentity, tenantIds);
        AddTenantRoles(claimsIdentity, employee);
        await AddTenantRoleClaims(claimsIdentity, employee);
        
        claimsIdentity.AddClaim(new Claim(CustomClaimTypes.Tenant, tenantId));
        return claimsIdentity;
    }

    private async Task AddAppRoleClaims(ClaimsIdentity claimsIdentity, User user)
    {
        var appRoles = await _userManager.GetRolesAsync(user);

        foreach (var roleName in appRoles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);

            if (role == null)
                continue;
            
            var claims = await _roleManager.GetClaimsAsync(role);
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

    private static void AddTenantIdsClaim(ClaimsIdentity claimsIdentity, IEnumerable<string> tenantIds)
    {
        foreach (var tenantId in tenantIds)
        {
            claimsIdentity.AddClaim(new Claim(CustomClaimTypes.Tenant, tenantId));
        }
    }

    private static void AddTenantRoles(ClaimsIdentity claimsIdentity, Employee? employee)
    {
        if (employee == null)
            return;
        
        foreach (var role in employee.Roles)
        {
            claimsIdentity.AddClaim(new Claim(CustomClaimTypes.Role, role.Name));
        }
    }
}