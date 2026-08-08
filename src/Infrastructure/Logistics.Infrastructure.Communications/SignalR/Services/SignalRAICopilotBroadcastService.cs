using Logistics.Application.Abstractions.AICopilot;
using Logistics.Infrastructure.Communications.SignalR.Clients;
using Logistics.Infrastructure.Communications.SignalR.Hubs;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.Infrastructure.Communications.SignalR.Services;

internal sealed class SignalRAICopilotBroadcastService(
    IHubContext<CopilotHub, IAICopilotHubClient> hubContext) : IAICopilotBroadcastService
{
    public Task BroadcastMessageAsync(Guid tenantId, Guid userId, AgentMessageDto message) =>
        Group(tenantId, userId).ReceiveCopilotMessage(message);

    public Task BroadcastTurnUpdateAsync(Guid tenantId, Guid userId, AgentTurnUpdateDto update) =>
        Group(tenantId, userId).ReceiveCopilotTurnUpdate(update);

    public Task BroadcastDecisionAsync(Guid tenantId, Guid userId, AgentDecisionDto decision) =>
        Group(tenantId, userId).ReceiveCopilotDecision(decision);

    private IAICopilotHubClient Group(Guid tenantId, Guid userId) =>
        hubContext.Clients.Group(CopilotHub.GroupName(tenantId, userId));
}
