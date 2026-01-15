namespace Logistics.Shared.Models.Messaging;

public record SendMessageRequest
{
    public Guid ConversationId { get; init; }
    public required string Content { get; init; }
}
