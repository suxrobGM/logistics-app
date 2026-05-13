using Logistics.Infrastructure.Communications.SignalR.Clients;
using Logistics.Infrastructure.Communications.SignalR.Hubs;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using Logistics.Application.Abstractions.AiDispatch;

namespace Logistics.Infrastructure.Communications.SignalR.Services;

internal sealed class SignalRAiDispatchBroadcastService(
    IHubContext<AiDispatchHub, IAiDispatchHubClient> hubContext) : IAiDispatchBroadcastService
{
    private const string DispatchBoardGroupPrefix = "dispatch-board:";

    public async Task BroadcastSessionUpdateAsync(Guid tenantId, AiDispatchUpdateDto update)
    {
        var group = $"{DispatchBoardGroupPrefix}{tenantId}";
        await hubContext.Clients.Group(group).ReceiveAiDispatchUpdate(update);
    }

    public async Task BroadcastDecisionAsync(Guid tenantId, AiDispatchDecisionDto decision)
    {
        var group = $"{DispatchBoardGroupPrefix}{tenantId}";
        await hubContext.Clients.Group(group).ReceiveAiDispatchDecision(decision);
    }

}
