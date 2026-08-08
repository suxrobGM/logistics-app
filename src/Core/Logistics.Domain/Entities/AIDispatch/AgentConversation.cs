using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// A multi-turn agent chat. Today this is always a copilot conversation owned by one user; Phase 3
/// adds tenant-shared dispatch conversations distinguished by <see cref="Kind"/>. Each turn is an
/// <see cref="AgentSession"/> of the matching type.
/// </summary>
public class AgentConversation : AuditableEntity, ITenantEntity
{
    /// <summary>Conversations are private - every handler must verify the caller matches.</summary>
    public Guid CreatedById { get; init; }

    public AgentConversationKind Kind { get; init; } = AgentConversationKind.Copilot;

    /// <summary>Derived from the first user message.</summary>
    public string? Title { get; set; }

    public AICopilotConversationStatus Status { get; private set; } = AICopilotConversationStatus.Idle;

    /// <summary>Lets a crashed turn be taken over after a staleness window.</summary>
    public DateTime? TurnStartedAt { get; private set; }

    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

    public virtual List<AgentMessage> Messages { get; } = [];

    /// <summary>
    /// Appends a text row and bumps <see cref="LastMessageAt"/>. The only place sequence numbers
    /// are allocated - callers must not build an <see cref="AgentMessage"/> themselves, and
    /// MUST register the returned message via repository AddAsync (ids are pre-generated, so a
    /// collection-only add saves as an UPDATE and fails).
    /// </summary>
    public AgentMessage AddTextMessage(AgentMessageRole role, string text)
    {
        var message = AgentMessage.TextMessage(Id, NextSequence(), role, text);
        Messages.Add(message);
        LastMessageAt = DateTime.UtcNow;
        return message;
    }

    public int NextSequence() => Messages.Count > 0 ? Messages.Max(m => m.Sequence) + 1 : 1;

    public void BeginTurn()
    {
        Status = AICopilotConversationStatus.Running;
        TurnStartedAt = DateTime.UtcNow;
    }

    public void EndTurn()
    {
        Status = AICopilotConversationStatus.Idle;
        TurnStartedAt = null;
    }
}
