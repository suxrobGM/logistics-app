#nullable enable
using Microsoft.AspNetCore.Identity;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Logistics.Domain.Repositories;

namespace Logistics.IdentityServer.Services;

// ReSharper disable once ClassNeverInstantiated.Global
internal class UserProfileService : IProfileService
{
    private readonly UserManager<User> _userManager;
    private readonly HttpContext? _httpContext;
    private readonly IUserClaimsPrincipalFactory<User> _claimsFactory;
    private readonly ITenantRepository<Employee> _tenantRepository;

    public UserProfileService(
        UserManager<User> userManager,
        IHttpContextAccessor httpContextAccessor,
        IUserClaimsPrincipalFactory<User> claimsFactory,
        ITenantRepository<Employee> tenantRepository)
    {
        _userManager = userManager;
        _httpContext = httpContextAccessor.HttpContext;
        _claimsFactory = claimsFactory;
        _tenantRepository = tenantRepository;
    }
    
    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var user = await _userManager.FindByIdAsync(context.Subject.GetSubjectId());
        var userAppRoles = await _userManager.GetRolesAsync(user);
        var principal = await _claimsFactory.CreateAsync(user);
        var tenantId = GetTenantId(context);
        var claims = principal.Claims.ToList();
        claims = claims.Where(claim => context.RequestedClaimTypes.Contains(claim.Type)).ToList();
        
        switch (context.Caller)
        {
            case "UserInfoEndpoint" when !string.IsNullOrEmpty(tenantId):
            {
                var role = context.Subject.GetRole();
                claims.TryAdd("role", role);
                claims.TryAdd("tenant", tenantId);
                break;
            }
            case "ClaimsProviderAccessToken" when !string.IsNullOrEmpty(tenantId):
            {
                SetRequestHeader("X-Tenant", tenantId);
                var tenantUser = await _tenantRepository.GetAsync(i => i.Id == user.Id);
                var tenantRoles = GetTenantRoles(tenantUser);

                claims.TryAdd("role", string.Join(',', userAppRoles, tenantRoles));
                claims.TryAdd("tenant", tenantId);
                break;
            }
            default:
                claims.TryAdd("role", string.Join(',', userAppRoles));
                break;
        }

        context.IssuedClaims = claims;
    }

    public async Task IsActiveAsync(IsActiveContext context)
    {
        var sub = context.Subject.GetSubjectId();
        var user = await _userManager.FindByIdAsync(sub);
        context.IsActive = user != null;
    }

    private static string? GetTenantId(ProfileDataRequestContext context)
    {
        var tenant = context.Subject.GetTenant();

        if (!string.IsNullOrEmpty(tenant))
        {
            return tenant;
        }
        
        var acrValues = context.ValidatedRequest?.Raw?.Get("acr_values")?.Split(" ");
        var tenantAcrValue = acrValues?.FirstOrDefault(i => i.StartsWith("tenant", StringComparison.InvariantCultureIgnoreCase));
        tenant = tenantAcrValue?.Split(":")[1];
        return tenant;
    }
    
    private void SetRequestHeader(string key, string? value)
    {
        var hasKey = _httpContext?.Request.Headers.ContainsKey(key);
        
        if (hasKey.HasValue)
            _httpContext?.Request.Headers.Remove(key);
        
        _httpContext?.Request.Headers.Add(key, value);
    }

    private string GetTenantRoles(Employee? employee)
    {
        if (employee == null)
            return string.Empty;
        
        var roleNames = employee.Roles.Select(i => i.Name);
        return string.Join(',', roleNames);
    }
}