using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public class DispatchAgentUpdateDto
{
    public Guid SessionId { get; set; }
    public DispatchSessionStatus Status { get; set; }
    public DispatchAgentMode Mode { get; set; }
    public int DecisionCount { get; set; }
    public string? Summary { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
