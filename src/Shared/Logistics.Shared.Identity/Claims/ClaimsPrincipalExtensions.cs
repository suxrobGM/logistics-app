using System.Security.Claims;

namespace Logistics.Shared.Identity.Claims;

/// <summary>Reads the identity claims that every authenticated surface shares.</summary>
public static class ClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal? principal)
    {
        public Guid? GetTenantId()
        {
            return ParseGuid(principal?.FindFirst(CustomClaimTypes.Tenant)?.Value);
        }

        public Guid? GetUserId()
        {
            var userId = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? principal?.FindFirst(CustomClaimTypes.Subject)?.Value;

            return ParseGuid(userId);
        }
    }

    private static Guid? ParseGuid(string? value)
    {
        return Guid.TryParse(value, out var parsed) ? parsed : null;
    }
}
