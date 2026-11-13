namespace Logistics.Domain.Entities;

/// <summary>Detention at one stop, as computed by <see cref="DetentionCalculator"/>.</summary>
/// <param name="OverMinutes">Minutes past the free time; positive means the truck is (or was) on detention.</param>
/// <param name="IsOngoing">The truck has not departed, so the numbers grow until it does.</param>
public sealed record Detention(
    int DwellMinutes,
    int FreeMinutes,
    int OverMinutes,
    decimal BillableHours,
    decimal? RatePerHour,
    decimal Charge,
    bool IsOngoing)
{
    public bool IsOverFreeTime => OverMinutes > 0;
}
