using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.Integrations.Agents.Services;

internal sealed class AgentConversationAccess(ITenantUnitOfWork tenantUow) : IAgentConversationAccess
{
    public async Task<AgentConversation?> LoadAsync(
        Guid conversationId, AgentConversationScope scope, CancellationToken ct)
    {
        var conversation = await tenantUow.Repository<AgentConversation>().GetByIdAsync(conversationId, ct);

        if (conversation is null || conversation.Kind != scope.Kind)
            return null;

        if (scope.RequireOwner && conversation.CreatedById != scope.CallerId)
            return null;

        return conversation;
    }

    public IQueryable<AgentConversation> Restrict(
        IQueryable<AgentConversation> query, AgentConversationScope scope)
    {
        query = query.Where(c => c.Kind == scope.Kind);

        return scope.RequireOwner
            ? query.Where(c => c.CreatedById == scope.CallerId!.Value)
            : query;
    }
}
