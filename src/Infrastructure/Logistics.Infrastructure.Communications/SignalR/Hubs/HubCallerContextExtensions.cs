using System.Security.Claims;
using Logistics.Shared.Identity.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.Infrastructure.Communications.SignalR.Hubs;

/// <summary>
/// Reads the caller's identity from the connection's JWT. Hubs must never take a tenant or user
/// id from a client argument: the tenant id selects the database a geolocation write lands in.
/// </summary>
public static class HubCallerContextExtensions
{
    /// <summary>
    /// The caller's tenant, or null when the claim is missing or unparseable. Tenant group names
    /// are built from the parsed value so they match the <c>Guid.ToString()</c> form the
    /// server-side broadcasters use.
    /// </summary>
    public static Guid? TenantIdFromClaim(this HubCallerContext context)
    {
        var claim = context.User?.FindFirst(CustomClaimTypes.Tenant)?.Value;
        return Guid.TryParse(claim, out var tenantId) ? tenantId : null;
    }

    /// <summary>
    /// The caller's user id. The API's JWT handler maps <c>sub</c> onto
    /// <see cref="ClaimTypes.NameIdentifier"/>, but not every issuer does, so both are accepted.
    /// </summary>
    public static Guid? UserIdFromClaim(this HubCallerContext context)
    {
        var claim = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? context.User?.FindFirst(CustomClaimTypes.Subject)?.Value;

        return Guid.TryParse(claim, out var userId) ? userId : null;
    }
}
