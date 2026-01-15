using Logistics.Domain.Core;

namespace Logistics.Domain.Entities.Messaging;

/// <summary>
/// Represents a participant in a conversation.
/// </summary>
public class ConversationParticipant : Entity, ITenantEntity
{
    /// <summary>
    /// The conversation this participant belongs to.
    /// </summary>
    public Guid ConversationId { get; set; }

    /// <summary>
    /// The employee who is participating.
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// When the participant joined the conversation.
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the participant last read the conversation (for unread count).
    /// </summary>
    public DateTime? LastReadAt { get; set; }

    /// <summary>
    /// Whether the participant has muted notifications for this conversation.
    /// </summary>
    public bool IsMuted { get; set; }

    /// <summary>
    /// Navigation property to the conversation.
    /// </summary>
    public virtual Conversation Conversation { get; set; } = null!;

    /// <summary>
    /// Navigation property to the employee.
    /// </summary>
    public virtual Employee Employee { get; set; } = null!;
}
