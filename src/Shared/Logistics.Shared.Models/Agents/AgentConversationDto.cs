using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record AgentConversationDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public AgentConversationStatus Status { get; set; }
    public DateTime LastMessageAt { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>Populated by the detail query only; null in list responses.</summary>
    public List<AgentMessageDto>? Messages { get; set; }

    /// <summary>Across all of the conversation's turns. Populated by the detail query only.</summary>
    public List<AgentDecisionDto>? Decisions { get; set; }

    /// <summary>
    /// One row per turn. Populated by the dispatch detail query only; null for copilot.
    /// </summary>
    public List<AgentSessionDto>? Sessions { get; set; }
}
