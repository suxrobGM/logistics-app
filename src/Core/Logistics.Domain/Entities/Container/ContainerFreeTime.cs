using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// One demurrage or per-diem clock. <see cref="DaysRemaining"/> is only set while the clock runs
/// (0 = the deadline is today); <see cref="DaysOver"/> counts the chargeable days past it.
/// </summary>
public sealed record FreeTimeClock(
    DateOnly? Deadline,
    FreeTimeStatus Status,
    int? DaysRemaining,
    int DaysOver,
    decimal? RatePerDay,
    decimal Charge)
{
    public static FreeTimeClock NotSet { get; } = new(null, FreeTimeStatus.NotSet, null, 0, null, 0);

    /// <summary>True when the deadline is within a day or already passed and the clock still runs.</summary>
    public bool NeedsAttention => Status is FreeTimeStatus.DueSoon or FreeTimeStatus.Overdue;
}

/// <summary>Chassis rental days for the move - every calendar day touched counts as one.</summary>
public sealed record ChassisUsage(int Days, decimal? RatePerDay, decimal Charge)
{
    public static ChassisUsage None { get; } = new(0, null, 0);
}

public sealed record ContainerFreeTime(FreeTimeClock Demurrage, FreeTimeClock PerDiem, ChassisUsage Chassis)
{
    public decimal TotalCharge => Demurrage.Charge + PerDiem.Charge + Chassis.Charge;
}
