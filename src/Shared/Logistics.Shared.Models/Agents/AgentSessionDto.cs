using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public class AgentSessionDto
{
    public Guid Id { get; set; }
    public AgentSessionStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int DecisionCount { get; set; }
    public string? ErrorMessage { get; set; }
    public int TotalTokensUsed { get; set; }
}
