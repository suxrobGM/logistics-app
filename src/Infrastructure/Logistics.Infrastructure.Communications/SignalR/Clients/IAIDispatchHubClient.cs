using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Communications.SignalR.Clients;

/// <summary>
/// Hub client for AI dispatch agent real-time operations.
/// </summary>
public interface IAIDispatchHubClient
{
    /// <summary>
    /// Receives AI dispatch agent session updates (started, completed, failed).
    /// </summary>
    Task ReceiveAIDispatchUpdate(AIDispatchUpdateDto update);

    /// <summary>
    /// Receives individual AI dispatch agent decision notifications.
    /// </summary>
    Task ReceiveAIDispatchDecision(AIDispatchDecisionDto decision);

}
