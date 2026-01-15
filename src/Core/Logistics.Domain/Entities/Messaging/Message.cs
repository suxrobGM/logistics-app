using Logistics.Domain.Core;

namespace Logistics.Domain.Entities.Messaging;

/// <summary>
/// Represents a message in a conversation.
/// </summary>
public class Message : Entity, ITenantEntity
{
    /// <summary>
    /// The conversation this message belongs to.
    /// </summary>
    public Guid ConversationId { get; set; }

    /// <summary>
    /// The employee who sent this message.
    /// </summary>
    public Guid SenderId { get; set; }

    /// <summary>
    /// The message content (max 2000 characters).
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// When the message was sent.
    /// </summary>
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the message has been deleted (soft delete).
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// When the message was deleted (if applicable).
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Navigation property to the conversation.
    /// </summary>
    public virtual Conversation Conversation { get; set; } = null!;

    /// <summary>
    /// Navigation property to the sender.
    /// </summary>
    public virtual Employee Sender { get; set; } = null!;

    /// <summary>
    /// Read receipts for this message.
    /// </summary>
    public virtual List<MessageReadReceipt> ReadReceipts { get; set; } = [];

    /// <summary>
    /// Creates a new message.
    /// </summary>
    public static Message Create(Guid conversationId, Guid senderId, string content)
    {
        return new Message
        {
            ConversationId = conversationId,
            SenderId = senderId,
            Content = content
        };
    }
}
