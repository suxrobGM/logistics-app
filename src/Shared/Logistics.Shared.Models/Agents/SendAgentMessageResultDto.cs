namespace Logistics.Shared.Models;

/// <summary>Returned from a 202-accepted agent send; progress then arrives over SignalR.</summary>
public record SendAgentMessageResultDto
{
    public Guid ConversationId { get; set; }
    public Guid UserMessageId { get; set; }

    /// <summary>Server-assigned timestamp for the user message, to reconcile the optimistic echo.</summary>
    public DateTime UserMessageCreatedAt { get; set; }

    /// <summary>
    /// Server-assigned sequence for the user message. Without it the optimistic echo keeps its
    /// sort-last tail key and renders below the reply once the turn's sequenced messages arrive.
    /// </summary>
    public int UserMessageSequence { get; set; }
}
