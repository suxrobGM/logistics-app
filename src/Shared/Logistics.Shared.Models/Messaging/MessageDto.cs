namespace Logistics.Shared.Models.Messaging;

public record MessageDto
{
    public Guid Id { get; init; }
    public Guid ConversationId { get; init; }
    public Guid SenderId { get; init; }
    public string? SenderName { get; init; }
    public required string Content { get; init; }
    public DateTime SentAt { get; init; }
    public bool IsRead { get; init; }
    public bool IsDeleted { get; init; }
}
