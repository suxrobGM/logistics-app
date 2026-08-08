using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

/// <summary>Progress of an in-flight dispatch turn, pushed tenant-wide over SignalR while the agent works.</summary>
public record AIDispatchTurnUpdateDto
{
    public Guid ConversationId { get; set; }
    public Guid SessionId { get; set; }
    public AgentSessionStatus Status { get; set; }
    public int TotalTokensUsed { get; set; }
    public int DecisionCount { get; set; }
    public string? ErrorMessage { get; set; }
}
