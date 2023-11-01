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
        var tenantIds = user.GetJoinedTenantIds();
        var tenantId = _httpContext.GetTenantId() ?? tenantIds.FirstOrDefault();

        AddProfileClaims(claimsIdentity, user);
        await AddAppRoleClaimsAsync(claimsIdentity, user);

        if (string.IsNullOrEmpty(tenantId)) 
            return claimsIdentity;

        _tenantRepository.SetCurrentTenantById(tenantId);
        var employee = await _tenantRepository.GetAsync<Employee>(user.Id);

        AddTenantIdClaims(claimsIdentity, tenantIds);
        await AddTenantRoleClaimsAsync(claimsIdentity, employee);
        return claimsIdentity;
    }
    
    private async Task AddAppRoleClaimsAsync(ClaimsIdentity claimsIdentity, User user)
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
    
    private async Task AddTenantRoleClaimsAsync(ClaimsIdentity claimsIdentity, Employee? employee)
    {
        if (employee == null)
            return;

        foreach (var tenantRole in employee.Roles)
        {
            var roleClaims = await _tenantRepository.GetListAsync<TenantRoleClaim>(i => i.RoleId == tenantRole.Id);
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

    private static void AddTenantIdClaims(ClaimsIdentity claimsIdentity, IEnumerable<string> tenantIds)
    {
        foreach (var tenantId in tenantIds)
        {
            claimsIdentity.AddClaim(new Claim(CustomClaimTypes.Tenant, tenantId));
        }
    }
}
