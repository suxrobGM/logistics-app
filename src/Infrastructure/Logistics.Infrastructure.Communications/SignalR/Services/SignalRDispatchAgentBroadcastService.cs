using Logistics.Application.Services;
using Logistics.Infrastructure.Communications.SignalR.Clients;
using Logistics.Infrastructure.Communications.SignalR.Hubs;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.Infrastructure.Communications.SignalR.Services;

internal sealed class SignalRDispatchAgentBroadcastService(
    IHubContext<DispatchAgentHub, IDispatchAgentHubClient> hubContext) : IDispatchAgentBroadcastService
{
    private const string DispatchBoardGroupPrefix = "dispatch-board:";

    public async Task BroadcastSessionUpdateAsync(Guid tenantId, DispatchAgentUpdateDto update)
    {
        var group = $"{DispatchBoardGroupPrefix}{tenantId}";
        await hubContext.Clients.Group(group).ReceiveDispatchAgentUpdate(update);
    }

    public async Task BroadcastDecisionAsync(Guid tenantId, DispatchDecisionDto decision)
    {
        var group = $"{DispatchBoardGroupPrefix}{tenantId}";
        await hubContext.Clients.Group(group).ReceiveDispatchDecision(decision);
    }

}
