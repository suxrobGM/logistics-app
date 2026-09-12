using Logistics.Application.Abstractions.Realtime;
using Logistics.Domain.Entities.Messaging;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.Integrations.Messaging.Services;

internal sealed class ConversationAccess(ITenantUnitOfWork tenantUow) : IConversationAccess
{
    public async Task<bool> CanUserJoinConversationAsync(
        Guid tenantId, Guid conversationId, Guid userId, CancellationToken ct = default)
    {
        // Resolving against the caller's own tenant database makes a cross-tenant id unfindable.
        await tenantUow.SetCurrentTenantByIdAsync(tenantId);

        var conversation = await tenantUow.Repository<Conversation>().GetByIdAsync(conversationId, ct);
        if (conversation is null)
        {
            return false;
        }

        if (conversation.IsTenantChat)
        {
            return true;
        }

        var participant = await tenantUow.Repository<ConversationParticipant>()
            .GetAsync(p => p.ConversationId == conversationId && p.EmployeeId == userId, ct);

        return participant is not null;
    }
}
