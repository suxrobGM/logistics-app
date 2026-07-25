using Logistics.Infrastructure.Communications.SignalR.Clients;
using Logistics.Infrastructure.Communications.SignalR.Hubs;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using Logistics.Application.Abstractions.AIDispatch;

namespace Logistics.Infrastructure.Communications.SignalR.Services;

internal sealed class SignalRAIDispatchBroadcastService(
    IHubContext<AIDispatchHub, IAIDispatchHubClient> hubContext) : IAIDispatchBroadcastService
{
    private const string DispatchBoardGroupPrefix = "dispatch-board:";

    public async Task BroadcastSessionUpdateAsync(Guid tenantId, AIDispatchUpdateDto update)
    {
        var group = $"{DispatchBoardGroupPrefix}{tenantId}";
        await hubContext.Clients.Group(group).ReceiveAIDispatchUpdate(update);
    }

    public async Task BroadcastDecisionAsync(Guid tenantId, AIDispatchDecisionDto decision)
    {
        var group = $"{DispatchBoardGroupPrefix}{tenantId}";
        await hubContext.Clients.Group(group).ReceiveAIDispatchDecision(decision);
    }

}
