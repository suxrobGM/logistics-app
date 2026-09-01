using Logistics.Shared.Identity.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.Infrastructure.Communications.SignalR.Hubs;

/// <summary>Reads authenticated identity claims from a SignalR connection.</summary>
public static class HubCallerContextExtensions
{
    /// <summary>Gets the caller's tenant ID, or null for a missing or invalid claim.</summary>
    public static Guid? TenantIdFromClaim(this HubCallerContext context)
    {
        return context.User.GetTenantId();
    }

    /// <summary>Gets the caller's user ID, or null for a missing or invalid claim.</summary>
    public static Guid? UserIdFromClaim(this HubCallerContext context)
    {
        return context.User.GetUserId();
    }
}
