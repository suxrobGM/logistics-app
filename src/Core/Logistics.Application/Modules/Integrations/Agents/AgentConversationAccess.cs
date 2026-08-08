using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.Integrations.Agents;

internal static class AgentConversationAccess
{
    /// <summary>
    /// Loads a conversation the surface may act on, or null when it does not exist, belongs to the
    /// other surface, or - for an owner-restricted scope - was created by someone else.
    /// </summary>
    public static async Task<AgentConversation?> LoadAsync(
        ITenantUnitOfWork tenantUow,
        Guid conversationId,
        AgentConversationScope scope,
        CancellationToken ct)
    {
        var conversation = await tenantUow.Repository<AgentConversation>().GetByIdAsync(conversationId, ct);

        if (conversation is null || conversation.Kind != scope.Kind)
            return null;

        if (scope.RequireOwner && conversation.CreatedById != scope.CallerId)
            return null;

        return conversation;
    }

    /// <summary>Narrows a conversation query to the same rows <see cref="LoadAsync"/> would return.</summary>
    public static IQueryable<AgentConversation> Restrict(
        IQueryable<AgentConversation> query, AgentConversationScope scope)
    {
        query = query.Where(c => c.Kind == scope.Kind);

        return scope.RequireOwner
            ? query.Where(c => c.CreatedById == scope.CallerId!.Value)
            : query;
    }
}
