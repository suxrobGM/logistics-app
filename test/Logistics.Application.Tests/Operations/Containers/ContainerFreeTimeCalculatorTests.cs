using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Xunit;

namespace Logistics.Application.Tests.Operations.Containers;

public class ContainerFreeTimeCalculatorTests
{
    private static readonly DateOnly Lfd = new(2026, 10, 9); // a Friday

    private static DateTime At(int day, int hour = 12) => new(2026, 10, day, hour, 0, 0, DateTimeKind.Utc);

    #region Running clock

    [Fact]
    public void Clock_NoDeadline_IsNotSet()
    {
        var clock = ContainerFreeTimeCalculator.Clock(null, null, 100m, At(9));

        Assert.Equal(FreeTimeStatus.NotSet, clock.Status);
        Assert.Equal(0, clock.Charge);
    }

    [Theory]
    [InlineData(5, FreeTimeStatus.Running, 4)]
    [InlineData(7, FreeTimeStatus.Running, 2)]
    [InlineData(8, FreeTimeStatus.DueSoon, 1)]
    [InlineData(9, FreeTimeStatus.DueSoon, 0)]
    public void Clock_BeforeOrOnDeadline_CountsDaysRemainingWithoutCharge(int today, FreeTimeStatus status, int remaining)
    {
        var clock = ContainerFreeTimeCalculator.Clock(Lfd, null, 150m, At(today));

        Assert.Equal(status, clock.Status);
        Assert.Equal(remaining, clock.DaysRemaining);
        Assert.Equal(0, clock.DaysOver);
        Assert.Equal(0, clock.Charge);
    }

    [Fact]
    public void Clock_LastMinuteOfLastFreeDay_IsStillFree()
    {
        var clock = ContainerFreeTimeCalculator.Clock(Lfd, null, 150m, new DateTime(2026, 10, 9, 23, 59, 0, DateTimeKind.Utc));

        Assert.Equal(FreeTimeStatus.DueSoon, clock.Status);
        Assert.Equal(0, clock.Charge);
    }

    [Fact]
    public void Clock_FirstMinuteAfterLastFreeDay_ChargesAWholeDay()
    {
        var clock = ContainerFreeTimeCalculator.Clock(Lfd, null, 150m, new DateTime(2026, 10, 10, 0, 1, 0, DateTimeKind.Utc));

        Assert.Equal(FreeTimeStatus.Overdue, clock.Status);
        Assert.Equal(1, clock.DaysOver);
        Assert.Equal(150m, clock.Charge);
    }

    /// <summary>Friday LFD, checked Monday: Saturday, Sunday and Monday all bill.</summary>
    [Fact]
    public void Clock_OverAWeekend_CountsWeekendDays()
    {
        var clock = ContainerFreeTimeCalculator.Clock(Lfd, null, 150m, At(12));

        Assert.Equal(3, clock.DaysOver);
        Assert.Equal(450m, clock.Charge);
        Assert.Equal(0, clock.DaysRemaining);
    }

    [Fact]
    public void Clock_OverdueWithoutRate_CountsDaysButChargesNothing()
    {
        var clock = ContainerFreeTimeCalculator.Clock(Lfd, null, null, At(12));

        Assert.Equal(FreeTimeStatus.Overdue, clock.Status);
        Assert.Equal(3, clock.DaysOver);
        Assert.Equal(0, clock.Charge);
    }

    #endregion

    #region Stopped clock

    [Fact]
    public void Clock_StoppedOnLastFreeDay_OwesNothing()
    {
        var clock = ContainerFreeTimeCalculator.Clock(Lfd, At(9, 22), 150m, At(20));

        Assert.Equal(FreeTimeStatus.Stopped, clock.Status);
        Assert.Null(clock.DaysRemaining);
        Assert.Equal(0, clock.Charge);
    }

    [Fact]
    public void Clock_StoppedAfterDeadline_FreezesTheCharge()
    {
        var clock = ContainerFreeTimeCalculator.Clock(Lfd, At(11, 1), 150m, At(25));

        Assert.Equal(FreeTimeStatus.Stopped, clock.Status);
        Assert.Equal(2, clock.DaysOver);
        Assert.Equal(300m, clock.Charge);
    }

    #endregion

    #region Chassis

    [Fact]
    public void Chassis_NotPickedUp_HasNoDays()
    {
        var usage = ContainerFreeTimeCalculator.Chassis(null, null, ChassisProvider.Dcli, 30m, At(9));

        Assert.Equal(0, usage.Days);
    }

    [Fact]
    public void Chassis_SameDayReturn_IsOneDay()
    {
        var usage = ContainerFreeTimeCalculator.Chassis(At(9, 8), At(9, 17), ChassisProvider.Dcli, 30m, At(20));

        Assert.Equal(1, usage.Days);
        Assert.Equal(30m, usage.Charge);
    }

    [Fact]
    public void Chassis_StillOut_CountsBothEndsThroughToday()
    {
        var usage = ContainerFreeTimeCalculator.Chassis(At(9, 23), null, ChassisProvider.Trac, 30m, At(12, 1));

        Assert.Equal(4, usage.Days);
        Assert.Equal(120m, usage.Charge);
    }

    [Fact]
    public void Chassis_OwnChassis_IsCountedButNotCharged()
    {
        var usage = ContainerFreeTimeCalculator.Chassis(At(9), At(12), ChassisProvider.Own, 30m, At(20));

        Assert.Equal(4, usage.Days);
        Assert.Equal(0, usage.Charge);
    }

    #endregion

    #region Whole container

    [Fact]
    public void Calculate_PickedUpBox_StopsDemurrageAndRunsPerDiem()
    {
        var container = new Container
        {
            Number = "MSCU1234567",
            IsoType = ContainerIsoType.Hc40,
            LastFreeDay = Lfd,
            DemurrageRatePerDay = 150m,
            PickedUpAt = At(10),
            EmptyReturnDeadline = new DateOnly(2026, 10, 14),
            PerDiemRatePerDay = 100m,
            ChassisProvider = ChassisProvider.Dcli,
            ChassisRatePerDay = 30m
        };

        var freeTime = container.GetFreeTime(At(16));

        Assert.Equal(FreeTimeStatus.Stopped, freeTime.Demurrage.Status);
        Assert.Equal(150m, freeTime.Demurrage.Charge);
        Assert.Equal(FreeTimeStatus.Overdue, freeTime.PerDiem.Status);
        Assert.Equal(200m, freeTime.PerDiem.Charge);
        Assert.Equal(7, freeTime.Chassis.Days);
        Assert.Equal(150m + 200m + 210m, freeTime.TotalCharge);
    }

    [Fact]
    public void Calculate_StreetTurned_StopsPerDiemAndChassis()
    {
        var container = new Container
        {
            Number = "MSCU1234567",
            IsoType = ContainerIsoType.Hc40,
            PickedUpAt = At(10),
            EmptyReturnDeadline = new DateOnly(2026, 10, 11),
            PerDiemRatePerDay = 100m,
            ChassisRatePerDay = 30m
        };
        container.MarkStreetTurned("EXP-1", At(13));

        var freeTime = container.GetFreeTime(At(25));

        Assert.Equal(FreeTimeStatus.Stopped, freeTime.PerDiem.Status);
        Assert.Equal(2, freeTime.PerDiem.DaysOver);
        Assert.Equal(4, freeTime.Chassis.Days);
    }

    #endregion
}
