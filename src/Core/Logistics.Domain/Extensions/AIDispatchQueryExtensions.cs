using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Persistence;

/// <summary>
/// The dispatch board and the TMS copilot share <see cref="AIDispatchSession"/>, so every
/// dispatch-side read has to exclude copilot turns. Route them through here rather than repeating
/// the predicate: a copilot session reached from a dispatch handler skips the conversation-owner
/// and per-tool permission checks that <c>ApproveAICopilotDecisionHandler</c> exists to enforce.
/// </summary>
public static class AIDispatchQueryExtensions
{
    /// <summary>
    /// Filters <see cref="AIDispatchSession"/> to only those that are dispatch-board sessions, not copilot conversations.
    /// </summary>
    public static IQueryable<AIDispatchSession> DispatchOnly(this IQueryable<AIDispatchSession> query) =>
        query.Where(s => s.Type == AIDispatchSessionType.Dispatch);

    /// <summary>
    /// Filters <see cref="AIDispatchDecision"/> to only those that are dispatch-board decisions, not copilot suggestions.
    /// </summary>
    public static IQueryable<AIDispatchDecision> DispatchOnly(this IQueryable<AIDispatchDecision> query) =>
        query.Where(d => d.Session.Type == AIDispatchSessionType.Dispatch);
}
