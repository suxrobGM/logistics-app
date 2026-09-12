using Logistics.Application.Abstractions.Realtime;
using Logistics.Domain.Entities.Messaging;
using Logistics.Domain.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.Messaging.Services;

internal sealed class ConversationAccess(ITenantUnitOfWork tenantUow) : IConversationAccess
{
    public async Task<bool> CanUserJoinConversationAsync(
        Guid tenantId, Guid conversationId, Guid userId, CancellationToken ct = default)
    {
        // Resolving against the caller's own tenant database makes a cross-tenant id unfindable.
        await tenantUow.SetCurrentTenantByIdAsync(tenantId);

        return await tenantUow.Repository<Conversation>().Query().AnyAsync(
            c => c.Id == conversationId &&
                 (c.IsTenantChat || c.Participants.Any(p => p.EmployeeId == userId)),
            ct);
    }
}
