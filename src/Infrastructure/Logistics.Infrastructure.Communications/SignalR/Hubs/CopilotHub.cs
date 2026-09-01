using Logistics.Infrastructure.Communications.SignalR.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.Infrastructure.Communications.SignalR.Hubs;

/// <summary>Streams private copilot events to the authenticated user.</summary>
[Authorize]
public class CopilotHub : Hub<IAICopilotHubClient>
{
    public override async Task OnConnectedAsync()
    {
        if (Context.TenantIdFromClaim() is not { } tenantId ||
            Context.UserIdFromClaim() is not { } userId)
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(tenantId, userId));
        await base.OnConnectedAsync();
    }

    /// <summary>Returns the private copilot group for a tenant user.</summary>
    public static string GroupName(Guid tenantId, Guid userId) => $"copilot:{tenantId}:{userId}";
}
