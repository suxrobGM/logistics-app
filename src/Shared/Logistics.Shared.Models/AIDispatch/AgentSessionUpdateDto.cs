using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public class AgentSessionUpdateDto
{
    public Guid SessionId { get; set; }
    public AgentSessionStatus Status { get; set; }
    public int DecisionCount { get; set; }
    public string? Summary { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
