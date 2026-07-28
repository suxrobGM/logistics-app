using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record AICopilotConversationDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public AICopilotConversationStatus Status { get; set; }
    public DateTime LastMessageAt { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>Populated by the detail query only; null in list responses.</summary>
    public List<AICopilotMessageDto>? Messages { get; set; }

    /// <summary>All decisions across the conversation's turns. Populated by the detail query only.</summary>
    public List<AIDispatchDecisionDto>? Decisions { get; set; }
}
