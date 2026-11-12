using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

/// <summary>
/// The tenant's auto-approve opt-in, each eligible decision type's progress toward it, and what it saved.
/// </summary>
public record AIDispatchAutonomyDto
{
    public int RequiredStreak { get; init; }
    public int MinutesSavedPerDecision { get; init; }
    public List<AutoApproveTypeDto> Types { get; init; } = [];

    /// <summary>Decisions the agent executed on its own in the last 30 days, reverted ones excluded.</summary>
    public int AutoExecutedLast30Days { get; init; }

    public int MinutesSavedLast30Days { get; init; }

    /// <summary>Auto-executed decisions still inside the revert window, newest first.</summary>
    public List<AgentDecisionDto> Revertable { get; init; } = [];
}

public record AutoApproveTypeDto
{
    public AgentDecisionType Type { get; init; }
    public bool IsOptedIn { get; init; }

    /// <summary>Clean approvals since the last rejection or revert.</summary>
    public int Streak { get; init; }
}
