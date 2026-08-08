using Logistics.Application.Abstractions.AI;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.AI.Agents.Dispatch;

/// <summary>
/// The dispatch agent's <see cref="IAgentSurface"/>: tenant-shared conversations, so broadcasts go
/// to the whole dispatch board and tools are gated by the endpoint policy, not per caller.
/// </summary>
internal sealed class DispatchAgentSurface(
    AIDispatchConversationBuilder conversationBuilder,
    IAIDispatchBroadcastService broadcastService) : IAgentSurface
{
    public AgentSessionType SessionType => AgentSessionType.Dispatch;

    public async Task<AgentTurnSetup> PrepareAsync(
        AgentSession session,
        AgentConversation conversation,
        AgentTurnRequest request,
        LlmOptions config,
        CancellationToken ct)
    {
        var llmConversation = await conversationBuilder.BuildAsync(session, conversation, config, ct);

        // CallerPermissions null and no broadcast override: the endpoint policy gates tools, and
        // decisions fall through AgentDecisionProcessor's default tenant-wide broadcast.
        return new AgentTurnSetup(llmConversation, new ToolCallContext());
    }

    public Task BroadcastMessageAsync(AgentTurnRequest request, AgentConversation conversation, AgentMessageDto message) =>
        broadcastService.BroadcastMessageAsync(request.TenantId, message);

    public Task BroadcastTurnUpdateAsync(AgentTurnRequest request, AgentConversation conversation, AgentSession session) =>
        broadcastService.BroadcastTurnUpdateAsync(request.TenantId, new AIDispatchTurnUpdateDto
        {
            ConversationId = conversation.Id,
            SessionId = session.Id,
            Status = session.Status,
            TotalTokensUsed = session.TotalTokensUsed,
            DecisionCount = session.DecisionCount,
            ErrorMessage = session.ErrorMessage
        });
}
