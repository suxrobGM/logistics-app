using Logistics.Application.Modules.AI.Agents.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Tests.TestKit;

/// <summary>
/// The real claim is an <c>ExecuteUpdateAsync</c>, which in-memory query mocks cannot run. This applies
/// the same rule to the tracked entity instead.
/// </summary>
internal sealed class FakeTurnClaim : IAgentTurnClaim
{
    public Task<bool> TryClaimAsync(AgentConversation conversation, CancellationToken ct)
    {
        var busy = conversation.Status == AgentConversationStatus.Running
                   && conversation.TurnStartedAt > DateTime.UtcNow - AgentConversation.StaleTurnWindow;
        if (busy)
        {
            return Task.FromResult(false);
        }

        conversation.BeginTurn();
        return Task.FromResult(true);
    }
}
