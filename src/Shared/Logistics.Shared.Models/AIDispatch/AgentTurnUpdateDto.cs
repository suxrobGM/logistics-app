using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

/// <summary>Progress of an in-flight agent turn, pushed over SignalR while the agent works.</summary>
public record AgentTurnUpdateDto
{
    public Guid ConversationId { get; set; }
    public Guid SessionId { get; set; }
    public AgentSessionStatus Status { get; set; }
    public int TotalTokensUsed { get; set; }
    public int DecisionCount { get; set; }
    public string? ErrorMessage { get; set; }
}
