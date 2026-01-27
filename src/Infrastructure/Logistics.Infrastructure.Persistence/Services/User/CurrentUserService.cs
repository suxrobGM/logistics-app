using System.Security.Claims;
using Logistics.Application.Services;
using Microsoft.AspNetCore.Http;

namespace Logistics.Infrastructure.Persistence.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? GetUserId()
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
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
