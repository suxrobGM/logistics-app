namespace Logistics.Shared.Models.Messaging;

public record ConversationParticipantDto
{
    public Guid EmployeeId { get; init; }
    public string? EmployeeName { get; init; }
    public DateTime JoinedAt { get; init; }
    public DateTime? LastReadAt { get; init; }
    public bool IsMuted { get; init; }
}
