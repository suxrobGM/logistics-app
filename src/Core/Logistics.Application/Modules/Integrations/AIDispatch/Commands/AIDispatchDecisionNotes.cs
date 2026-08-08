using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

/// <summary>
/// Shared by dispatch decision approval and rejection: records the outcome in the conversation
/// transcript and pushes it to the board. Old sessions may predate conversations - only note the
/// transcript when this decision's session belongs to one.
/// </summary>
internal static class AIDispatchDecisionNotes
{
    public static async Task AppendAsync(
        ITenantUnitOfWork tenantUow,
        IAIDispatchBroadcastService broadcastService,
        AgentDecision decision,
        string note,
        CancellationToken ct)
    {
        if (decision.Session.ConversationId is not { } conversationId)
        {
            await tenantUow.SaveChangesAsync(ct);
            return;
        }

        var conversation = await tenantUow.Repository<AgentConversation>().GetByIdAsync(conversationId, ct);
        if (conversation is null)
        {
            await tenantUow.SaveChangesAsync(ct);
            return;
        }

        var message = conversation.AddTextMessage(AgentMessageRole.System, note);
        await tenantUow.Repository<AgentMessage>().AddAsync(message, ct);
        await tenantUow.SaveChangesAsync(ct);

        await broadcastService.BroadcastMessageAsync(tenantUow.GetCurrentTenant().Id, message.ToDto());
    }
}
