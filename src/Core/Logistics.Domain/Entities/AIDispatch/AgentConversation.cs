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
    /// Highest sequence handed out so far. Held here so allocating the next one never reads the
    /// Messages navigation, which lazy-loads the whole transcript.
    /// </summary>
    public int LastSequence { get; private set; }

    /// <summary>
    /// The only allocator of sequence numbers - never build an <see cref="AgentMessage"/> directly.
    /// Callers MUST also register the returned row via repository AddAsync (pre-generated ids make
    /// a collection-only add save as an UPDATE and fail).
    /// </summary>
    public AgentMessage AddTextMessage(AgentMessageRole role, string text)
    {
        var message = AgentMessage.TextMessage(Id, NextSequence(), role, text);
        Messages.Add(message);
        LastMessageAt = DateTime.UtcNow;
        return message;
    }

    public int NextSequence() => ++LastSequence;

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
