using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Xunit;

namespace Logistics.Application.Tests.Operations.Containers;

public class ContainerDrayageLifecycleTests
{
    private static readonly DateTime Now = new(2026, 10, 9, 12, 0, 0, DateTimeKind.Utc);

    private static Terminal Port() => new()
    {
        Name = "Los Angeles",
        Code = "USLAX",
        CountryCode = "US",
        Type = TerminalType.SeaPort,
        Address = new Address { Line1 = "1 Pier", City = "San Pedro", State = "CA", ZipCode = "90731", Country = "US" }
    };

    private static Container NewContainer() => new() { Number = "MSCU1234567", IsoType = ContainerIsoType.Hc40 };

    #region Status machine timestamps

    [Theory]
    [InlineData(ContainerStatus.InTransit)]
    [InlineData(ContainerStatus.Loaded)]
    [InlineData(ContainerStatus.Empty)]
    public void Apply_LeavingAtPort_StampsPickedUpAt(ContainerStatus target)
    {
        var state = ContainerStatusMachine.Apply(
            ContainerStatus.AtPort, target, false, new ContainerStatusTimestamps(null, null, null, null), Now);

        Assert.Equal(Now, state.Timestamps.PickedUpAt);
    }

    [Fact]
    public void Apply_EnteringAtPort_LeavesPickedUpAtAlone()
    {
        var state = ContainerStatusMachine.Apply(
            ContainerStatus.Empty, ContainerStatus.AtPort, false, new ContainerStatusTimestamps(null, null, null, null), Now);

        Assert.Null(state.Timestamps.PickedUpAt);
    }

    [Fact]
    public void Apply_LeavingReturned_ClearsThePreviousMoveTimestamps()
    {
        var old = new ContainerStatusTimestamps(Now.AddDays(-9), Now.AddDays(-5), Now.AddDays(-2), Now.AddDays(-10));

        var state = ContainerStatusMachine.Apply(ContainerStatus.Returned, ContainerStatus.Empty, false, old, Now);

        Assert.Equal(ContainerStatus.Empty, state.Status);
        Assert.Equal(new ContainerStatusTimestamps(null, null, null, null), state.Timestamps);
    }

    #endregion

    #region New move

    [Fact]
    public void UpdateStatus_ReturnedToEmpty_ResetsPerMoveDataButKeepsLineAndRates()
    {
        var container = NewContainer();
        container.SteamshipLineScac = "MSCU";
        container.PerDiemRatePerDay = 100m;
        container.LastFreeDay = new DateOnly(2026, 10, 1);
        container.EmptyReturnDeadline = new DateOnly(2026, 10, 5);
        container.CustomsHold = true;
        container.ChassisNumber = "DCLZ123456";
        container.PickupAppointmentNumber = "APT-1";
        container.MarkReturned(Port());

        container.MarkAsEmpty();

        Assert.Null(container.LastFreeDay);
        Assert.Null(container.EmptyReturnDeadline);
        Assert.Null(container.ReturnedAt);
        Assert.False(container.IsOnHold);
        Assert.Null(container.ChassisNumber);
        Assert.Null(container.PickupAppointmentNumber);
        Assert.Equal("MSCU", container.SteamshipLineScac);
        Assert.Equal(100m, container.PerDiemRatePerDay);
    }

    #endregion

    #region Street turn

    [Fact]
    public void MarkStreetTurned_EmptyBox_RecordsBookingAndTime()
    {
        var container = NewContainer();

        container.MarkStreetTurned(" EXP-42 ", Now);

        Assert.Equal(Now, container.StreetTurnedAt);
        Assert.Equal("EXP-42", container.StreetTurnBookingReference);
        Assert.Equal(ContainerStatus.Empty, container.Status);
    }

    [Fact]
    public void MarkStreetTurned_LadenBox_Throws()
    {
        var container = NewContainer();
        container.MarkAsLoaded();

        Assert.Throws<InvalidOperationException>(() => container.MarkStreetTurned("EXP-42", Now));
    }

    [Fact]
    public void MarkStreetTurned_ReturnedBox_Throws()
    {
        var container = NewContainer();
        container.MarkReturned(Port());

        Assert.Throws<InvalidOperationException>(() => container.MarkStreetTurned("EXP-42", Now));
    }

    #endregion

    #region Holds

    [Fact]
    public void IsAvailableForPickup_OnHold_IsFalseEvenAfterAvailableDate()
    {
        var container = NewContainer();
        container.AvailableForPickupAt = Now.AddHours(-1);
        container.LineHold = true;

        Assert.True(container.IsOnHold);
        Assert.False(container.IsAvailableForPickup(Now));
    }

    [Fact]
    public void IsAvailableForPickup_NoHoldsAndDatePassed_IsTrue()
    {
        var container = NewContainer();
        container.AvailableForPickupAt = Now.AddHours(-1);

        Assert.True(container.IsAvailableForPickup(Now));
        Assert.False(container.IsAvailableForPickup(Now.AddHours(-2)));
    }

    #endregion
}
