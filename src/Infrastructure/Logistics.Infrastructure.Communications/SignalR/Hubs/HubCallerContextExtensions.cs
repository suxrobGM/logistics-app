using System.Security.Claims;
using Logistics.Shared.Identity.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.Infrastructure.Communications.SignalR.Hubs;

/// <summary>Reads authenticated identity claims from a SignalR connection.</summary>
public static class HubCallerContextExtensions
{
    /// <summary>Gets the caller's tenant ID, or null for a missing or invalid claim.</summary>
    public static Guid? TenantIdFromClaim(this HubCallerContext context)
    {
        var claim = context.User?.FindFirst(CustomClaimTypes.Tenant)?.Value;
        return Guid.TryParse(claim, out var tenantId) ? tenantId : null;
    }

    /// <summary>Gets the caller's user ID, or null for a missing or invalid claim.</summary>
    public static Guid? UserIdFromClaim(this HubCallerContext context)
    {
        var claim = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? context.User?.FindFirst(CustomClaimTypes.Subject)?.Value;

        return Guid.TryParse(claim, out var userId) ? userId : null;
    }
}
