using Logistics.Application.Abstractions.AI;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.AI.Agents.Dispatch;

/// <summary>Dispatch turn setup: the conversation is shared by the tenant, updates go to every dispatch board client, and tool access is enforced by the endpoint's authorization policy.</summary>
internal sealed class DispatchAgentSurface(
    AIDispatchConversationBuilder conversationBuilder,
    IAIDispatchBroadcastService broadcastService) : IAgentSurface
{
    public AgentConversationKind Kind => AgentConversationKind.Dispatch;

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
        broadcastService.BroadcastTurnUpdateAsync(request.TenantId, new AgentTurnUpdateDto
        {
            ConversationId = conversation.Id,
            SessionId = session.Id,
            Status = session.Status,
            TotalTokensUsed = session.TotalTokensUsed,
            DecisionCount = session.DecisionCount,
            ErrorMessage = session.ErrorMessage
        });
}
