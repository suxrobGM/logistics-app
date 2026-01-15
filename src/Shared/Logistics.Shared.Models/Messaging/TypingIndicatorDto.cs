namespace Logistics.Shared.Models.Messaging;

public record TypingIndicatorDto
{
    public Guid ConversationId { get; init; }
    public Guid UserId { get; init; }
    public string? UserName { get; init; }
    public bool IsTyping { get; init; }
}
