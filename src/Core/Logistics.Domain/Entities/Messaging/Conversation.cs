using Logistics.Domain.Core;

namespace Logistics.Domain.Entities.Messaging;

/// <summary>
/// Represents a conversation between participants (dispatcher and driver, or multiple users).
/// </summary>
public class Conversation : Entity, ITenantEntity
{
    /// <summary>
    /// Display name for the conversation (e.g., "Load #1234 Discussion" or participant name for 1:1).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Optional load ID if this conversation is related to a specific load.
    /// </summary>
    public Guid? LoadId { get; set; }

    /// <summary>
    /// When the conversation was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp of the last message in this conversation (for sorting).
    /// </summary>
    public DateTime? LastMessageAt { get; set; }

    /// <summary>
    /// Navigation property to the related load.
    /// </summary>
    public virtual Load? Load { get; set; }

    /// <summary>
    /// Participants in this conversation.
    /// </summary>
    public virtual List<ConversationParticipant> Participants { get; set; } = [];

    /// <summary>
    /// Messages in this conversation.
    /// </summary>
    public virtual List<Message> Messages { get; set; } = [];

    /// <summary>
    /// Creates a new conversation between two participants.
    /// </summary>
    public static Conversation CreateDirectConversation(Guid participant1Id, Guid participant2Id, Guid? loadId = null)
    {
        var conversation = new Conversation
        {
            LoadId = loadId,
            Participants =
            [
                new ConversationParticipant { EmployeeId = participant1Id },
                new ConversationParticipant { EmployeeId = participant2Id }
            ]
        };
        return conversation;
    }
}
