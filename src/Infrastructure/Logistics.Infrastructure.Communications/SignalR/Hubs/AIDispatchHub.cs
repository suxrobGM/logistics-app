using Logistics.Infrastructure.Communications.SignalR.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.Infrastructure.Communications.SignalR.Hubs;

/// <summary>Streams dispatch-board updates to the caller's tenant.</summary>
[Authorize]
public class AIDispatchHub : Hub<IAIDispatchHubClient>
{
    public override async Task OnConnectedAsync()
    {
        if (Context.TenantIdFromClaim() is not { } tenantId)
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(tenantId));
        await base.OnConnectedAsync();
    }

    /// <summary>Returns the dispatch-board group for a tenant.</summary>
    public static string GroupName(Guid tenantId) => $"dispatch-board:{tenantId}";

    [Obsolete("The board group is joined from the JWT on connect; this call does nothing.")]
    public Task SubscribeToDispatchBoard(string tenantId) => Task.CompletedTask;

    [Obsolete("The board group is joined from the JWT on connect; this call does nothing.")]
    public Task UnsubscribeFromDispatchBoard(string tenantId) => Task.CompletedTask;
}
