using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record AgentMessageDto
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public int Sequence { get; set; }
    public AgentMessageRole Role { get; set; }

    /// <summary>Markdown for assistant messages. Null for tool-result rows, which the UI hides.</summary>
    public string? Text { get; set; }

    /// <summary>The turn that produced this message.</summary>
    public Guid? SessionId { get; set; }

    /// <summary>
    /// The person this row is attributable to. Null for the agent's own messages and for anything
    /// no person caused - a shared dispatch transcript renders no author for those.
    /// </summary>
    public Guid? SentByUserId { get; set; }

    /// <summary>Resolved at read time; null when the sender has no employee row any more.</summary>
    public string? SentByName { get; set; }

    public DateTime CreatedAt { get; set; }
}
