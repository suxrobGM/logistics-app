using Logistics.Application.Abstractions.AI;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.AI.Agents.Dispatch;

/// <summary>
/// The dispatch agent as an <see cref="IAgentSurface"/>: no per-caller tool scoping (the endpoint's
/// policy is the gate, as with the pre-conversation dispatch runs), and every broadcast goes to the
/// whole tenant's dispatch board - conversations here are tenant-shared, not per-user like copilot.
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
