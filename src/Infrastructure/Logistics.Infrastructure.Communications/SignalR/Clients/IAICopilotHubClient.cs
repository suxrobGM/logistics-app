using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Communications.SignalR.Clients;

/// <summary>Events the copilot chat drawer receives while a turn runs.</summary>
public interface IAICopilotHubClient
{
    Task ReceiveCopilotMessage(AICopilotMessageDto message);
    Task ReceiveCopilotDecision(AgentDecisionDto decision);
    Task ReceiveCopilotTurnUpdate(AICopilotTurnUpdateDto update);
}
