using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Demurrage, per-diem and chassis arithmetic for one container move.
/// <para>
/// Days are calendar days on the UTC date - weekends and holidays count, because the common
/// terminal and steamship-line tariffs count them, and any part of a day counts as a whole day.
/// The deadline day itself is free: a box picked up on its last free day owes nothing.
/// </para>
/// </summary>
public static class ContainerFreeTimeCalculator
{
    public static ContainerFreeTime Calculate(Container container, DateTime now)
    {
        var moveEnded = container.ReturnedAt ?? container.StreetTurnedAt;

        return new ContainerFreeTime(
            Clock(container.LastFreeDay, container.PickedUpAt, container.DemurrageRatePerDay, now),
            Clock(container.EmptyReturnDeadline, moveEnded, container.PerDiemRatePerDay, now),
            Chassis(container.PickedUpAt, moveEnded, container.ChassisProvider, container.ChassisRatePerDay, now));
    }

    /// <param name="deadline">The last free day.</param>
    /// <param name="stoppedAt">When the clock stopped, or null while it still runs.</param>
    public static FreeTimeClock Clock(DateOnly? deadline, DateTime? stoppedAt, decimal? ratePerDay, DateTime now)
    {
        if (deadline is not { } lastFreeDay)
        {
            return FreeTimeClock.NotSet;
        }

        var end = DateOnly.FromDateTime(stoppedAt ?? now);
        var daysOver = Math.Max(0, end.DayNumber - lastFreeDay.DayNumber);
        var charge = daysOver * (ratePerDay ?? 0);

        if (stoppedAt is not null)
        {
            return new FreeTimeClock(lastFreeDay, FreeTimeStatus.Stopped, null, daysOver, ratePerDay, charge);
        }

        var remaining = lastFreeDay.DayNumber - end.DayNumber;
        var status = remaining switch
        {
            < 0 => FreeTimeStatus.Overdue,
            <= 1 => FreeTimeStatus.DueSoon,
            _ => FreeTimeStatus.Running
        };

        return new FreeTimeClock(lastFreeDay, status, Math.Max(0, remaining), daysOver, ratePerDay, charge);
    }

    /// <summary>
    /// Days from outgate to the end of the move, both days included. An own chassis is counted
    /// but never charged.
    /// </summary>
    public static ChassisUsage Chassis(
        DateTime? pickedUpAt,
        DateTime? endedAt,
        ChassisProvider? provider,
        decimal? ratePerDay,
        DateTime now)
    {
        if (pickedUpAt is not { } start)
        {
            return ChassisUsage.None;
        }

        var end = endedAt ?? now;
        if (end < start)
        {
            return ChassisUsage.None;
        }

        var days = DateOnly.FromDateTime(end).DayNumber - DateOnly.FromDateTime(start).DayNumber + 1;
        var charge = provider == ChassisProvider.Own ? 0 : days * (ratePerDay ?? 0);
        return new ChassisUsage(days, ratePerDay, charge);
    }
}
