#nullable enable
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Logistics.Domain.Repositories;

namespace Logistics.IdentityServer.Services;

public class UserCustomClaimsFactory : UserClaimsPrincipalFactory<User, AppRole>
{
    private readonly HttpContext _httpContext;
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
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
    {
        var claimsIdentity = await base.GenerateClaimsAsync(user);
        var tenantId = _httpContext.GetTenantId();

        if (string.IsNullOrEmpty(tenantId)) 
            return claimsIdentity;
        
        var tenantRoles = await GetTenantRolesAsync(user.Id);

        foreach (var role in tenantRoles)
        {
            claimsIdentity.AddClaim(new Claim("role", role));
        }
        
        claimsIdentity.AddClaim(new Claim("tenant", tenantId));
        return claimsIdentity;
    }

    private async Task<IEnumerable<string>> GetTenantRolesAsync(string userId)
    {
        var employee = await _tenantRepository.GetAsync<Employee>(userId);
        var roleNames = employee?.Roles.Select(i => i.Name!);
        return roleNames ?? new []{""};
    }
}