namespace Logistics.Shared.Models;

/// <summary>
/// Display-only regulatory ceilings for the active HOS rule set. Returned by
/// <c>GET /api/eld/rules/limits</c>; never used to compute violations.
/// </summary>
public record HosLimitsDto
{
    public string RuleSetCode { get; init; } = "FMCSA";
    public int MaxDailyDrivingMinutes { get; init; }
    public int MaxDailyOnDutyMinutes { get; init; }
    public int MaxWeeklyDrivingMinutes { get; init; }
    public int MaxBiweeklyDrivingMinutes { get; init; }
    public int? MaxContinuousDrivingMinutes { get; init; }
    public int MinDailyRestMinutes { get; init; }
    public int MinWeeklyRestMinutes { get; init; }
    public int? RequiredBreakAfterMinutes { get; init; }
    public int CycleDays { get; init; }
}
