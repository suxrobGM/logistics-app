namespace Logistics.Shared.Models.Messaging;

public record CreateConversationRequest
{
    public string? Name { get; init; }
    public Guid? LoadId { get; init; }
    public required List<Guid> ParticipantIds { get; init; }
}
