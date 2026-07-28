namespace Logistics.Shared.Models;

/// <summary>Returned from a 202-accepted copilot send; progress then arrives over SignalR.</summary>
public record SendAICopilotMessageResultDto
{
    public Guid ConversationId { get; set; }
    public Guid UserMessageId { get; set; }
}
