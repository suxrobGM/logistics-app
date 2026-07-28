using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// A multi-turn copilot chat owned by one user. Each turn is an <see cref="AIDispatchSession"/>
/// of type Copilot.
/// </summary>
public class AICopilotConversation : AuditableEntity, ITenantEntity
{
    /// <summary>Conversations are private - every handler must verify the caller matches.</summary>
    public Guid CreatedById { get; init; }

    /// <summary>Derived from the first user message.</summary>
    public string? Title { get; set; }

    public AICopilotConversationStatus Status { get; private set; } = AICopilotConversationStatus.Idle;

    /// <summary>Lets a crashed turn be taken over after a staleness window.</summary>
    public DateTime? TurnStartedAt { get; private set; }

    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

    public virtual List<AICopilotMessage> Messages { get; } = [];

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
