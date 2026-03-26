using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public class DispatchSessionDto
{
    public Guid Id { get; set; }
    public long Number { get; set; }
    public DispatchAgentMode Mode { get; set; }
    public DispatchSessionStatus Status { get; set; }
    public Guid? TriggeredByUserId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TotalTokensUsed { get; set; }
    public int DecisionCount { get; set; }
    public string? Summary { get; set; }
    public string? ErrorMessage { get; set; }
    public List<DispatchDecisionDto> Decisions { get; set; } = [];
}
