using Logistics.Shared.Identity.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.Infrastructure.Communications.SignalR.Hubs;

/// <summary>Reads authenticated identity claims from a SignalR connection.</summary>
public static class HubCallerContextExtensions
{
    extension(HubCallerContext context)
    {
        /// <summary>Gets the caller's tenant ID, or null for a missing or invalid claim.</summary>
        public Guid? TenantIdFromClaim()
        {
            return context.User.GetTenantId();
        }

        /// <summary>Gets the caller's user ID, or null for a missing or invalid claim.</summary>
        public Guid? UserIdFromClaim()
        {
            return context.User.GetUserId();
        }
    }

}
