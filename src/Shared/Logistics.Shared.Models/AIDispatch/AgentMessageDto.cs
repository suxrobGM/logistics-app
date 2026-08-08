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

    public DateTime CreatedAt { get; set; }
}
