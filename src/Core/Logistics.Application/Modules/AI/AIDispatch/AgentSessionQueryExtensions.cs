using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.AI.AIDispatch;

/// <summary>Filters to dispatch sessions only. Copilot sessions must be read through the copilot handlers, which check conversation ownership and per-tool permissions.</summary>
public static class AgentSessionQueryExtensions
{
    extension(IQueryable<AgentSession> query)
    {
        /// <summary>
        /// Filters <see cref="AgentSession"/> to only those that are dispatch-board sessions, not copilot conversations.
        /// </summary>
        public IQueryable<AgentSession> DispatchOnly() =>
            query.Where(s => s.Conversation.Kind == AgentConversationKind.Dispatch);
    }

    extension(IQueryable<AgentDecision> query)
    {
        /// <summary>
        /// Filters <see cref="AgentDecision"/> to only those that are dispatch-board decisions, not copilot suggestions.
        /// </summary>
        public IQueryable<AgentDecision> DispatchOnly() =>
            query.Where(d => d.Session.Conversation.Kind == AgentConversationKind.Dispatch);
    }
}
