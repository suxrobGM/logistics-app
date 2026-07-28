using System.Security.Claims;
using Logistics.Infrastructure.Communications.SignalR.Clients;
using Logistics.Shared.Identity.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.Infrastructure.Communications.SignalR.Hubs;

/// <summary>
///     Copilot conversations are private, so this hub - unlike <see cref="AIDispatchHub"/> - is
///     authorized and takes its group membership from the caller's JWT claims. Clients never
///     supply tenant or user ids.
/// </summary>
[Authorize]
public class CopilotHub : Hub<IAICopilotHubClient>
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? Context.User?.FindFirst("sub")?.Value;
        var tenantId = Context.User?.FindFirst(CustomClaimTypes.Tenant)?.Value;

        if (userId is null || tenantId is null)
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(tenantId, userId));
        await base.OnConnectedAsync();
    }

    /// <summary>The single source of the group-name shape, shared with the broadcaster.</summary>
    public static string GroupName(Guid tenantId, Guid userId) =>
        GroupName(tenantId.ToString(), userId.ToString());

    private static string GroupName(string tenantId, string userId) => $"copilot:{tenantId}:{userId}";
}
