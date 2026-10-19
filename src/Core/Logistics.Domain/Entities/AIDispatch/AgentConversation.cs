using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// A multi-turn agent chat; each turn is an <see cref="AgentSession"/> of the matching type.
/// <see cref="Kind"/> separates per-user copilot conversations from tenant-shared dispatch ones.
/// </summary>
public class AgentConversation : AuditableEntity, ITenantEntity
{
    /// <summary>
    /// For Copilot conversations this is an ownership boundary every handler must verify;
    /// Dispatch conversations are tenant-shared and keep it for audit only.
    /// </summary>
    public Guid CreatedById { get; init; }

    public AgentConversationKind Kind { get; init; } = AgentConversationKind.Copilot;

    /// <summary>Derived from the first user message.</summary>
    public string? Title { get; set; }

    public AgentConversationStatus Status { get; private set; } = AgentConversationStatus.Idle;

    /// <summary>Lets a crashed turn be taken over after a staleness window.</summary>
    public DateTime? TurnStartedAt { get; private set; }

    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

    public virtual List<AgentMessage> Messages { get; } = [];

    /// <summary>
    /// <paramref name="sequence"/> must be reserved in the database first: several writers append to
    /// one conversation concurrently, so no in-memory counter can hand it out. Never touches
    /// <see cref="Messages"/>, which lazy-loads the whole transcript. Callers MUST also register the
    /// returned row via repository AddAsync (pre-generated ids make a collection-only add save as an
    /// UPDATE and fail).
    /// </summary>
    public AgentMessage AddTextMessage(int sequence, AgentMessageRole role, string text, Guid? sentByUserId = null)
    {
        var message = AgentMessage.TextMessage(Id, sequence, role, text, sentByUserId);
        LastMessageAt = DateTime.UtcNow;
        return message;
    }

    /// <summary>
    /// A row from an agent turn: provider replay content plus the text the transcript shows, if any.
    /// Same sequence and registration rules as <see cref="AddTextMessage"/>.
    /// </summary>
    public AgentMessage AddTurnMessage(
        int sequence, Guid sessionId, AgentMessageRole role, string contentJson, string? displayText)
    {
        return new AgentMessage
        {
            ConversationId = Id,
            Sequence = sequence,
            Role = role,
            ContentJson = contentJson,
            DisplayText = displayText,
            SessionId = sessionId
        };
    }

    public void BeginTurn()
    {
        Status = AgentConversationStatus.Running;
        TurnStartedAt = DateTime.UtcNow;
    }

    public void EndTurn()
    {
        Status = AgentConversationStatus.Idle;
        TurnStartedAt = null;
    }
}
