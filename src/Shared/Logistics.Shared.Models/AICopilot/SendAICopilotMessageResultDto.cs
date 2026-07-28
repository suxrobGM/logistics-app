namespace Logistics.Shared.Models;

/// <summary>
///     Returned from a 202-accepted copilot send: the turn runs in the background and its
///     progress arrives over SignalR.
/// </summary>
public record SendAICopilotMessageResultDto
{
    public Guid ConversationId { get; set; }
    public Guid UserMessageId { get; set; }
}
