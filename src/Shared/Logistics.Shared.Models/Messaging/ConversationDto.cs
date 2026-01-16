namespace Logistics.Shared.Models.Messaging;

public record ConversationDto
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public Guid? LoadId { get; init; }
    public bool IsTenantChat { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastMessageAt { get; init; }
    public int UnreadCount { get; init; }
    public MessageDto? LastMessage { get; init; }
    public List<ConversationParticipantDto> Participants { get; init; } = [];
}
