namespace Logistics.Shared.Models;

/// <summary>Returned from a 202-accepted copilot send; progress then arrives over SignalR.</summary>
public record SendAICopilotMessageResultDto
{
    public Guid ConversationId { get; set; }
    public Guid UserMessageId { get; set; }

    /// <summary>
    /// Server clock. The client stamps its optimistic echo from the browser clock, which sorts the
    /// message below its own reply whenever the two clocks disagree - this is what it swaps in.
    /// </summary>
    public DateTime UserMessageCreatedAt { get; set; }
}
