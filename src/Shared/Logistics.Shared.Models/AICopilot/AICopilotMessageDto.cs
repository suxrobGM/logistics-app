using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record AICopilotMessageDto
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public int Sequence { get; set; }
    public AICopilotMessageRole Role { get; set; }

    /// <summary>
    ///     Rendered text (markdown for assistant messages). Null for internal tool-result rows,
    ///     which the UI does not display.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>The turn (session) that produced this message.</summary>
    public Guid? SessionId { get; set; }

    public DateTime CreatedAt { get; set; }
}
