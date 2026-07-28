using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// A multi-turn copilot chat owned by one user. Each turn is recorded as an
/// <see cref="AIDispatchSession"/> of type Copilot; the transcript lives in <see cref="Messages"/>.
/// </summary>
public class AICopilotConversation : AuditableEntity, ITenantEntity
{
    /// <summary>
    /// The owning user. Every handler must verify the caller matches - conversations are private.
    /// </summary>
    public Guid CreatedById { get; init; }

    /// <summary>
    /// Derived from the first user message; shown in the conversation list.
    /// </summary>
    public string? Title { get; set; }

    public AICopilotConversationStatus Status { get; private set; } = AICopilotConversationStatus.Idle;

    /// <summary>
    /// When the in-flight turn began. Lets a crashed turn be taken over after a staleness window
    /// instead of locking the conversation forever.
    /// </summary>
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
