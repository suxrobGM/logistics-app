using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Shared.Identity.Claims;

namespace Logistics.Infrastructure.Persistence.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string? IpAddress =>
        httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent =>
        httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();

    public Guid? GetUserId()
    {
        var user = httpContextAccessor.HttpContext?.User;

        var userIdClaim = user?.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? user?.FindFirstValue(CustomClaimTypes.Subject);

        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    public Guid? GetTenantId()
    {
        var tenantClaim = httpContextAccessor.HttpContext?.User.FindFirstValue(CustomClaimTypes.Tenant);
        return Guid.TryParse(tenantClaim, out var tenantId) ? tenantId : null;
    }

    public bool IsInRole(params string[] roles)
    {
        var user = httpContextAccessor.HttpContext?.User;
        return user is not null && roles.Any(user.IsInRole);
    }

    public string GetUserName()
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user is null)
        {
            return "Unknown";
        }

        // Try to get the name from common claims
        var name = user.FindFirstValue(ClaimTypes.Name);
        if (!string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        // Try given_name + family_name
        var givenName = user.FindFirstValue(ClaimTypes.GivenName);
        var familyName = user.FindFirstValue(ClaimTypes.Surname);
        if (!string.IsNullOrWhiteSpace(givenName) || !string.IsNullOrWhiteSpace(familyName))
        {
            return $"{givenName} {familyName}".Trim();
        }

        // Fallback to email
        var email = user.FindFirstValue(ClaimTypes.Email);
        if (!string.IsNullOrWhiteSpace(email))
        {
            return email;
        }

        return "Unknown";
    }
}
