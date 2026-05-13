namespace Logistics.Domain.Primitives.ValueObjects;

/// <summary>
/// Display-only regulatory ceilings for the active HOS rule set. Returned by
/// <c>GET /api/eld/rules/limits</c> so the UI can render thresholds and percentages.
/// Never used to compute or override violations — those come from the certified
/// ELD or tachograph device via its provider API.
/// </summary>
public record HosLimits(
    string RuleSetCode,
    int MaxDailyDrivingMinutes,
    int MaxDailyOnDutyMinutes,
    int MaxWeeklyDrivingMinutes,
    int MaxBiweeklyDrivingMinutes,
    int? MaxContinuousDrivingMinutes,
    int MinDailyRestMinutes,
    int MinWeeklyRestMinutes,
    int? RequiredBreakAfterMinutes,
    int CycleDays)
{
    public const string FmcsaCode = "FMCSA";
    public const string Eu561Code = "EU_561_2006";

    public static HosLimits Fmcsa() => new(
        RuleSetCode: FmcsaCode,
        MaxDailyDrivingMinutes: 11 * 60,
        MaxDailyOnDutyMinutes: 14 * 60,
        MaxWeeklyDrivingMinutes: 60 * 60,
        MaxBiweeklyDrivingMinutes: 70 * 60,
        MaxContinuousDrivingMinutes: null,
        MinDailyRestMinutes: 10 * 60,
        MinWeeklyRestMinutes: 34 * 60,
        RequiredBreakAfterMinutes: 8 * 60,
        CycleDays: 8);

    public static HosLimits Eu561_2006() => new(
        RuleSetCode: Eu561Code,
        MaxDailyDrivingMinutes: 9 * 60,
        MaxDailyOnDutyMinutes: 13 * 60,
        MaxWeeklyDrivingMinutes: 56 * 60,
        MaxBiweeklyDrivingMinutes: 90 * 60,
        MaxContinuousDrivingMinutes: (int)(4.5 * 60),
        MinDailyRestMinutes: 11 * 60,
        MinWeeklyRestMinutes: 45 * 60,
        RequiredBreakAfterMinutes: (int)(4.5 * 60),
        CycleDays: 14);
}
