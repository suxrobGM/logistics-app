using Logistics.Domain.Core;

namespace Logistics.Domain.Entities.Messaging;

/// <summary>
/// Tracks when a message was read by a participant.
/// </summary>
public class MessageReadReceipt : Entity, ITenantEntity
{
    /// <summary>
    /// The message that was read.
    /// </summary>
    public Guid MessageId { get; set; }

    /// <summary>
    /// The employee who read the message.
    /// </summary>
    public Guid ReadById { get; set; }

    /// <summary>
    /// When the message was read.
    /// </summary>
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to the message.
    /// </summary>
    public virtual Message Message { get; set; } = null!;

    /// <summary>
    /// Navigation property to the reader.
    /// </summary>
    public virtual Employee ReadBy { get; set; } = null!;
}
