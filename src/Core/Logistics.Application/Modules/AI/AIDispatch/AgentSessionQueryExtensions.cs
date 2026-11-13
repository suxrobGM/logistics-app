using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.AI.AIDispatch;

/// <summary>
/// The dispatch board and the TMS copilot share <see cref="AgentSession"/>, so every
/// dispatch-side read has to exclude copilot turns. Route them through here rather than repeating
/// the predicate: a copilot session reached from a dispatch handler skips the conversation-owner
/// and per-tool permission checks that <c>ApproveAICopilotDecisionHandler</c> exists to enforce.
/// </summary>
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
