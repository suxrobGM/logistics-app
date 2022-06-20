#nullable enable
using System.Collections.Specialized;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Logistics.Domain.Services;

namespace Logistics.IdentityServer.Services;

// ReSharper disable once ClassNeverInstantiated.Global
internal class UserProfileService : IProfileService
{
    private readonly UserManager<User> _userManager;
    private readonly IUserClaimsPrincipalFactory<User> _claimsFactory;
    private readonly ITenantManager _tenantManager;

    public UserProfileService(
        UserManager<User> userManager,
        IUserClaimsPrincipalFactory<User> claimsFactory,
        ITenantManager tenantManager)
    {
        _userManager = userManager;
        _claimsFactory = claimsFactory;
        _tenantManager = tenantManager;
    }
    
    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var user = await _userManager.FindByIdAsync(context.Subject.GetSubjectId());
        var principal = await _claimsFactory.CreateAsync(user);
        var tenantId = GetTenantValue(context.ValidatedRequest?.Raw);
        var claims = principal.Claims.ToList();
        claims = claims.Where(claim => context.RequestedClaimTypes.Contains(claim.Type)).ToList();
        
        if (!string.IsNullOrEmpty(tenantId))
        {
            var tenantUser = await _tenantManager.GetEmployeeAsync(tenantId, i => i.ExternalId == user.Id);
            claims.Add(new Claim("tenant", tenantId));

            if (tenantUser != null)
            {
                claims.Add(new Claim("role", tenantUser.Role.Name));
            }
        }
        else
        {
            claims.Add(new Claim("role", user.Role.Name));
        }
        
        if (!string.IsNullOrEmpty(user.FirstName))
        {
            claims.Add(new Claim("first_name", user.FirstName));
        }
        
        if (!string.IsNullOrEmpty(user.LastName))
        {
            claims.Add(new Claim("last_name", user.LastName));
        }
        context.IssuedClaims = claims;
    }

    public async Task IsActiveAsync(IsActiveContext context)
    {
        var sub = context.Subject.GetSubjectId();
        var user = await _userManager.FindByIdAsync(sub);
        context.IsActive = user != null;
    }

    private static string? GetTenantValue(NameValueCollection? collection)
    {
        var acrValues = collection?.Get("acr_values")?.Split(" ");
        var tenantValue = acrValues?.FirstOrDefault(i => i.StartsWith("tenant", StringComparison.InvariantCultureIgnoreCase));
        return tenantValue?.Split(":")[1];
    }
}