#nullable enable
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Logistics.Domain.Repositories;

namespace Logistics.IdentityServer.Services;

public class UserCustomClaimsFactory : UserClaimsPrincipalFactory<User, AppRole>
{
    private readonly HttpContext _httpContext;
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
        _userManager = userManager;
        _httpContext = httpContextAccessor.HttpContext!;
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
    {
        var claimsIdentity = await base.GenerateClaimsAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        var tenantId = _httpContext.GetTenantId();

        if (!string.IsNullOrEmpty(tenantId))
        {
            var tenantRoles = await GetTenantRolesAsync(user.Id);
            TryAddRange(roles, tenantRoles);
            claimsIdentity.AddClaim(new Claim("tenant", tenantId));
        }

        claimsIdentity.AddClaim(new Claim("role", string.Join(',', roles)));
        return claimsIdentity;
    }

    private async Task<IEnumerable<string>?> GetTenantRolesAsync(string userId)
    {
        var employee = await _tenantRepository.GetAsync<Employee>(userId);
        var roleNames = employee?.Roles.Select(i => i.Name!);
        return roleNames;
    }

    private static void TryAddRange<T>(ICollection<T>? list, IEnumerable<T>? itemsToAdd)
    {
        if (list == null || itemsToAdd == null)
            return;

        foreach (var item in itemsToAdd)
        {
            list.Add(item);
        }
    }
}