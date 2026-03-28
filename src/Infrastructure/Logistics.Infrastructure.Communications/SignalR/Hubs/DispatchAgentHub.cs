using Logistics.Infrastructure.Communications.SignalR.Clients;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.Infrastructure.Communications.SignalR.Hubs;

public class DispatchAgentHub : Hub<IDispatchAgentHubClient>
{
    private const string DispatchBoardGroupPrefix = "dispatch-board:";

    public async Task RegisterTenant(string tenantId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, tenantId);
    }

    public async Task UnregisterTenant(string tenantId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, tenantId);
    }

    /// <summary>
    ///     Subscribe to dispatch board updates for a tenant.
    /// </summary>
    public async Task SubscribeToDispatchBoard(string tenantId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"{DispatchBoardGroupPrefix}{tenantId}");
    }

    /// <summary>
    ///     Unsubscribe from dispatch board updates.
    /// </summary>
    public async Task UnsubscribeFromDispatchBoard(string tenantId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{DispatchBoardGroupPrefix}{tenantId}");
    }
}
