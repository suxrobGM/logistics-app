namespace Logistics.Shared.Models;

/// <summary>Returned from a 202-accepted copilot send; progress then arrives over SignalR.</summary>
public record SendAICopilotMessageResultDto
{
    public Guid ConversationId { get; set; }
    public Guid UserMessageId { get; set; }

    /// <summary>Server-assigned timestamp for the user message, to reconcile the optimistic echo.</summary>
    public DateTime UserMessageCreatedAt { get; set; }
}
