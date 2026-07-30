using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AICopilot.Commands;

internal sealed record CopilotDecisionContext(
    AgentDecision Decision,
    AICopilotConversation Conversation);

/// <summary>
/// Shared by copilot decision approval and rejection: the decision must exist, belong to a copilot
/// turn, still be Suggested, and its conversation must be owned by the caller.
/// </summary>
internal static class AICopilotDecisionGuard
{
    public static async Task<Result<CopilotDecisionContext>> LoadAsync(
        ITenantUnitOfWork tenantUow, Guid decisionId, Guid? userId, CancellationToken ct)
    {
        if (userId is null)
            return Result<CopilotDecisionContext>.Fail("User is not authenticated");

        var decision = await tenantUow.Repository<AgentDecision>().GetByIdAsync(decisionId, ct);
        if (decision is null
            || decision.Session.Type != AgentSessionType.Copilot
            || decision.Session.ConversationId is not { } conversationId)
        {
            return Result<CopilotDecisionContext>.Fail("Decision not found");
        }

        var conversation = await tenantUow.Repository<AICopilotConversation>()
            .GetByIdAsync(conversationId, ct);
        if (conversation is null || conversation.CreatedById != userId.Value)
            return Result<CopilotDecisionContext>.Fail("Decision not found");

        if (decision.Status != AgentDecisionStatus.Suggested)
            return Result<CopilotDecisionContext>.Fail("Decision is not in a suggested state");

        return Result<CopilotDecisionContext>.Ok(new CopilotDecisionContext(decision, conversation));
    }
}
