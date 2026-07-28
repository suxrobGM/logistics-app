using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.AICopilot.Commands;

/// <summary>
/// The checks shared by copilot decision approval and rejection: the decision must exist, belong
/// to a copilot turn, still be Suggested, and its conversation must be owned by the caller.
/// </summary>
internal static class AICopilotDecisionGuard
{
    public sealed record GuardResult(
        AIDispatchDecision? Decision,
        AICopilotConversation? Conversation,
        string? Error);

    public static async Task<GuardResult> LoadAsync(
        ITenantUnitOfWork tenantUow, Guid decisionId, Guid? userId, CancellationToken ct)
    {
        if (userId is null)
            return new GuardResult(null, null, "User is not authenticated");

        var decision = await tenantUow.Repository<AIDispatchDecision>().GetByIdAsync(decisionId, ct);
        if (decision is null
            || decision.Session.Type != AIDispatchSessionType.Copilot
            || decision.Session.ConversationId is not { } conversationId)
        {
            return new GuardResult(null, null, "Decision not found");
        }

        var conversation = await tenantUow.Repository<AICopilotConversation>()
            .GetByIdAsync(conversationId, ct);
        if (conversation is null || conversation.CreatedById != userId.Value)
            return new GuardResult(null, null, "Decision not found");

        if (decision.Status != AIDispatchDecisionStatus.Suggested)
            return new GuardResult(null, null, "Decision is not in a suggested state");

        return new GuardResult(decision, conversation, null);
    }

    /// <summary>Appends the approval/rejection outcome as a System note the next turn replays.</summary>
    public static AICopilotMessage AppendOutcomeNote(AICopilotConversation conversation, string note)
    {
        var message = AICopilotMessage.TextMessage(
            conversation.Id,
            conversation.Messages.Count > 0 ? conversation.Messages.Max(m => m.Sequence) + 1 : 1,
            AICopilotMessageRole.System,
            note);
        conversation.Messages.Add(message);
        conversation.LastMessageAt = DateTime.UtcNow;
        return message;
    }
}
