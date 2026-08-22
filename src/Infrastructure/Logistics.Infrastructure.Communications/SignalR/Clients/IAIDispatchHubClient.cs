using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Communications.SignalR.Clients;

/// <summary>
/// Hub client for AI dispatch conversations. Tenant-shared - unlike the copilot hub, every event
/// goes to the whole tenant's dispatch board group.
/// </summary>
public interface IAIDispatchHubClient
{
    /// <summary>Receives a new transcript row (assistant reply or system note).</summary>
    Task ReceiveDispatchMessage(AgentMessageDto message);

    /// <summary>Receives progress of the in-flight turn (status, tokens, decisions).</summary>
    Task ReceiveDispatchTurnUpdate(AgentTurnUpdateDto update);

    /// <summary>Receives individual AI dispatch agent decision notifications.</summary>
    Task ReceiveAIDispatchDecision(AgentDecisionDto decision);

    /// <summary>Receives a broker negotiation thread whose state just changed.</summary>
    Task ReceiveNegotiationUpdate(RateNegotiationDto negotiation);
}
