using Logistics.Application.Abstractions.AI;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.AI.Agents;

internal interface IAgentSurface
{
    AgentConversationKind Kind { get; }

    /// <summary>Builds the <see cref="AgentTurnSetup"/> for a turn; called once, before the agent loop runs.</summary>
    Task<AgentTurnSetup> PrepareAsync(
        AgentSession session,
        AgentConversation conversation,
        AgentTurnRequest request,
        LlmOptions config,
        CancellationToken ct);

    /// <summary>Pushes one transcript row to the conversation's listeners.</summary>
    Task BroadcastMessageAsync(AgentTurnRequest request, AgentConversation conversation, AgentMessageDto message);

    /// <summary>Pushes turn progress (status, tokens, decisions) to the conversation's listeners.</summary>
    Task BroadcastTurnUpdateAsync(AgentTurnRequest request, AgentConversation conversation, AgentSession session);
}
