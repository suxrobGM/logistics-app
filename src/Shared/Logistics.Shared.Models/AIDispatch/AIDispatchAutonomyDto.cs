using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record AIDispatchAutonomyDto
{
    public int RequiredStreak { get; init; }
    public int MinutesSavedPerDecision { get; init; }
    public List<AutoApproveTypeDto> Types { get; init; } = [];

    /// <summary>Reverted decisions excluded.</summary>
    public int AutoExecutedLast30Days { get; init; }

    public int MinutesSavedLast30Days { get; init; }

    /// <summary>Still inside the revert window, newest first.</summary>
    public List<AgentDecisionDto> Revertable { get; init; } = [];
}

public record AutoApproveTypeDto
{
    public AgentDecisionType Type { get; init; }
    public bool IsOptedIn { get; init; }

    /// <summary>Clean approvals since the last rejection or revert.</summary>
    public int Streak { get; init; }
}
