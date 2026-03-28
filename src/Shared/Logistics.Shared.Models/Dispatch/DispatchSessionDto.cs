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
    public int InputTokensUsed { get; set; }
    public int OutputTokensUsed { get; set; }
    public int CacheReadTokens { get; set; }
    public int CacheCreationTokens { get; set; }
    public decimal EstimatedCostUsd { get; set; }
    public string? ModelUsed { get; set; }
    public int DecisionCount { get; set; }
    public string? Summary { get; set; }
    public string? Instructions { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsOverage { get; set; }
    public List<DispatchDecisionDto> Decisions { get; set; } = [];
}
