using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public class AIDispatchUpdateDto
{
    public Guid SessionId { get; set; }
    public AIDispatchSessionStatus Status { get; set; }
    public AIDispatchMode Mode { get; set; }
    public int DecisionCount { get; set; }
    public string? Summary { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
