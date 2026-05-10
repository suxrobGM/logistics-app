using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public class AiDispatchUpdateDto
{
    public Guid SessionId { get; set; }
    public AiDispatchSessionStatus Status { get; set; }
    public AiDispatchMode Mode { get; set; }
    public int DecisionCount { get; set; }
    public string? Summary { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
