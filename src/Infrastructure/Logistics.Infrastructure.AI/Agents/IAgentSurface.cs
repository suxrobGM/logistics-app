using Logistics.Application.Abstractions.AI;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.AI.Agents;

/// <summary>
/// The per-kind seam <see cref="AgentTurnService"/> runs every turn through. Copilot supplies
/// today's behavior (permission-scoped catalogue, per-user broadcasts); Phase 3 adds a dispatch
/// surface here without touching the turn lifecycle itself.
/// </summary>
internal interface IAgentSurface
{
    /// <summary>Stamped on the <see cref="AgentSession"/> the turn service creates.</summary>
    AgentSessionType SessionType { get; }

    /// <summary>
    /// Resolves the LLM conversation and tool-call context for one turn. Called once, before the
    /// agent loop runs.
    /// </summary>
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
