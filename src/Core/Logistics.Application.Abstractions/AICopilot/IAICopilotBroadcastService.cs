using Logistics.Shared.Models;

namespace Logistics.Application.Abstractions.AICopilot;

/// <summary>
///     Pushes copilot events to the conversation owner's SignalR group. Conversations are private,
///     so every broadcast targets one tenant + user pair - never the whole tenant.
/// </summary>
public interface IAICopilotBroadcastService
{
    Task BroadcastMessageAsync(Guid tenantId, Guid userId, AICopilotMessageDto message);
    Task BroadcastTurnUpdateAsync(Guid tenantId, Guid userId, AICopilotTurnUpdateDto update);
    Task BroadcastDecisionAsync(Guid tenantId, Guid userId, AgentDecisionDto decision);
}
