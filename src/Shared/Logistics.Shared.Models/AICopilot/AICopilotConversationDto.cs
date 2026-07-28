using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record AICopilotConversationDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public AICopilotConversationStatus Status { get; set; }
    public DateTime LastMessageAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
