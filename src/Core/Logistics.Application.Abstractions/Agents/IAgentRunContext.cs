namespace Logistics.Application.Abstractions.Agents;

/// <summary>
/// Who an agent run is acting on behalf of. Tools receive only the model's JSON arguments, so a
/// tool that must attribute a write to a real person has no other way to learn who that is - and
/// it must not come from the model, which has no reliable source for a user id and should not be
/// able to name one.
/// </summary>
/// <remarks>
/// Scoped. Populated when a session starts, including inside a Hangfire worker, where
/// <c>ICurrentUserService</c> has no HTTP context to read.
/// </remarks>
public interface IAgentRunContext
{
    /// <summary>
    /// The user who triggered the run, or null for a run with no human origin (a scheduled
    /// dispatch pass). Tools that require attribution fail rather than guess.
    /// </summary>
    Guid? TriggeredByUserId { get; }

    /// <summary>
    /// The conversation the run belongs to, for tools that must link what they create back to the
    /// thread that asked for it. Also set on the approval path, which has no run of its own.
    /// </summary>
    Guid? ConversationId { get; }

    /// <summary>
    /// The decision being executed, set only on the approval path. A tool called during a live turn
    /// has no decision id yet - the decision row is written from its result.
    /// </summary>
    Guid? DecisionId { get; }

    void SetTriggeredBy(Guid? userId);

    void SetConversation(Guid? conversationId);

    void SetDecision(Guid? decisionId);
}
