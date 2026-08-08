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

    public Task BroadcastMessageAsync(Guid tenantId, AgentMessageDto message) =>
        Group(tenantId).ReceiveDispatchMessage(message);

    public Task BroadcastTurnUpdateAsync(Guid tenantId, AIDispatchTurnUpdateDto update) =>
        Group(tenantId).ReceiveDispatchTurnUpdate(update);

    public Task BroadcastDecisionAsync(Guid tenantId, AgentDecisionDto decision) =>
        Group(tenantId).ReceiveAIDispatchDecision(decision);

    private IAIDispatchHubClient Group(Guid tenantId) =>
        hubContext.Clients.Group($"{DispatchBoardGroupPrefix}{tenantId}");
}
