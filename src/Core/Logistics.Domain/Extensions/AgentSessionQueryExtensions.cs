using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Persistence;

/// <summary>
/// The dispatch board and the TMS copilot share <see cref="AgentSession"/>, so every
/// dispatch-side read has to exclude copilot turns. Route them through here rather than repeating
/// the predicate: a copilot session reached from a dispatch handler skips the conversation-owner
/// and per-tool permission checks that <c>ApproveAICopilotDecisionHandler</c> exists to enforce.
/// </summary>
public static class AgentSessionQueryExtensions
{
    /// <summary>
    /// Filters <see cref="AgentSession"/> to only those that are dispatch-board sessions, not copilot conversations.
    /// </summary>
    public static IQueryable<AgentSession> DispatchOnly(this IQueryable<AgentSession> query) =>
        query.Where(s => s.Type == AgentSessionType.Dispatch);

    /// <summary>
    /// Filters <see cref="AgentDecision"/> to only those that are dispatch-board decisions, not copilot suggestions.
    /// </summary>
    public static IQueryable<AgentDecision> DispatchOnly(this IQueryable<AgentDecision> query) =>
        query.Where(d => d.Session.Type == AgentSessionType.Dispatch);
}
