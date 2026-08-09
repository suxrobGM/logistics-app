using Logistics.Application.Abstractions.Common;
using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.Integrations.Agents.Services;

internal interface IAgentConversationAccess : IApplicationService
{
    /// <summary>
    /// Loads a conversation the surface may act on, or null when it does not exist, belongs to the
    /// other surface, or - for an owner-restricted scope - was created by someone else.
    /// </summary>
    Task<AgentConversation?> LoadAsync(
        Guid conversationId, AgentConversationScope scope, CancellationToken ct);

    /// <summary>Narrows a conversation query to the same rows <see cref="LoadAsync"/> would return.</summary>
    IQueryable<AgentConversation> Restrict(
        IQueryable<AgentConversation> query, AgentConversationScope scope);
}
