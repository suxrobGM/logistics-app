using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Xunit;

namespace Logistics.Application.Tests.Operations.Trips;

public class TripTests
{
    private readonly Truck _truck = TestData.Truck();

    private readonly Employee _dispatcher = TestData.Driver();

    private Load NewLoad() => Load.Create(
        $"Load {Guid.NewGuid():N}", LoadType.GeneralFreight, 1000, TestData.Address(), new GeoPoint(0, 0), TestData.Address(), new GeoPoint(1, 1),
        new Customer { Name = "Acme" }, _truck, _dispatcher);

    #region Loads

    /// <summary>Before the insert every load number is 0, so loads must be told apart by id.</summary>
    [Fact]
    public void GetLoads_TwoUnsavedLoadsWithSameName_ReturnsBoth()
    {
        var first = NewLoad();
        var second = NewLoad();
        second.Name = first.Name;
        var trip = Trip.Create("Trip", _truck, [first, second]);
        first.Distance = 100;
        second.Distance = 50;

        trip.RecalculateDistance();

        Assert.Equal(2, trip.GetLoads().Count);
        Assert.Equal(150, trip.TotalDistance);
    }

    /// <summary>The trip decides its loads' truck, so moving the trip moves every load with it.</summary>
    [Fact]
    public void AssignToTruck_MovesEveryLoadToTheNewTruck()
    {
        var trip = Trip.Create("Trip", _truck, [NewLoad(), NewLoad()]);
        var newTruck = TestData.Truck(number: "TRK-002");

        trip.AssignToTruck(newTruck);

        Assert.All(trip.GetLoads(), load => Assert.Equal(newTruck.Id, load.AssignedTruckId));
    }

    [Fact]
    public void Create_WithTruck_AssignsItToEveryLoad()
    {
        var load = NewLoad();
        load.AssignedTruckId = null;

        Trip.Create("Trip", _truck, [load]);

        Assert.Equal(_truck.Id, load.AssignedTruckId);
    }

    #endregion

    #region Dispatch

    [Fact]
    public void Dispatch_NoTruck_Throws()
    {
        var trip = Trip.Create("Trip", truck: null, [NewLoad()]);

        Assert.Throws<InvalidOperationException>(trip.Dispatch);
        Assert.Equal(TripStatus.Draft, trip.Status);
    }

    [Fact]
    public void LoadDispatch_NoTruck_Throws()
    {
        var load = NewLoad();
        load.AssignedTruckId = null;

        Assert.Throws<InvalidOperationException>(() => load.Dispatch());
        Assert.Equal(LoadStatus.Draft, load.Status);
    }

    #endregion

    #region Dispatch

    [Fact]
    public void Dispatch_DraftLoads_DispatchesLoadsAndIssuesInvoices()
    {
        var load = NewLoad();
        var trip = Trip.Create("Trip", _truck, [load]);

        trip.Dispatch();

        Assert.Equal(TripStatus.Dispatched, trip.Status);
        Assert.Equal(LoadStatus.Dispatched, load.Status);
        Assert.NotNull(load.DispatchedAt);
        Assert.Equal(InvoiceStatus.Issued, load.Invoice!.Status);
    }

    [Fact]
    public void Dispatch_LoadAlreadyPastDraft_KeepsItsStatus()
    {
        var load = NewLoad();
        load.Dispatch();
        load.ConfirmPickup();
        var trip = Trip.Create("Trip", _truck, [load]);

        trip.Dispatch();

        Assert.Equal(LoadStatus.PickedUp, load.Status);
    }

    [Fact]
    public void Dispatch_RecordsTheTrucksDrivers_AndKeepsThemWhenTheTruckChangesDrivers()
    {
        var main = Guid.NewGuid();
        var team = Guid.NewGuid();
        _truck.MainDriverId = main;
        _truck.SecondaryDriverId = team;
        var trip = Trip.Create("Trip", _truck, [NewLoad()]);

        trip.Dispatch();
        _truck.MainDriverId = Guid.NewGuid();

        Assert.Equal(main, trip.DriverId);
        Assert.Equal(team, trip.SecondaryDriverId);
    }

    [Fact]
    public void AssignToTruck_DispatchedTrip_RecordsTheNewTrucksDrivers()
    {
        var trip = Trip.Create("Trip", _truck, [NewLoad()]);
        trip.Dispatch();
        var newDriver = Guid.NewGuid();

        trip.AssignToTruck(new Truck { Number = "TRK-002", Type = TruckType.FreightTruck, MainDriverId = newDriver });

        Assert.Equal(newDriver, trip.DriverId);
        Assert.Null(trip.SecondaryDriverId);
    }

    [Fact]
    public void AssignToTruck_DraftTrip_RecordsNoDrivers()
    {
        var trip = Trip.Create("Trip", _truck, [NewLoad()]);

        trip.AssignToTruck(new Truck { Number = "TRK-002", Type = TruckType.FreightTruck, MainDriverId = Guid.NewGuid() });

        Assert.Null(trip.DriverId);
    }

    #endregion

    #region MarkStopArrived

    private static TripStop StopOf(Trip trip, TripStopType type) => trip.Stops.First(s => s.Type == type);

    [Fact]
    public void MarkStopArrived_PickupThenDropOff_DeliversLoadAndCompletesTrip()
    {
        var load = NewLoad();
        var trip = Trip.Create("Trip", _truck, [load]);
        trip.Dispatch();

        trip.MarkStopArrived(StopOf(trip, TripStopType.PickUp).Id);
        Assert.Equal(LoadStatus.PickedUp, load.Status);
        Assert.Equal(TripStatus.InTransit, trip.Status);

        trip.MarkStopArrived(StopOf(trip, TripStopType.DropOff).Id);
        Assert.Equal(LoadStatus.Delivered, load.Status);
        Assert.NotNull(load.PickedUpAt);
        Assert.Equal(TripStatus.Completed, trip.Status);
    }

    [Fact]
    public void MarkStopArrived_DraftTrip_Throws()
    {
        var load = NewLoad();
        var trip = Trip.Create("Trip", _truck, [load]);

        Assert.Throws<InvalidOperationException>(() => trip.MarkStopArrived(StopOf(trip, TripStopType.PickUp).Id));
        Assert.Equal(TripStatus.Draft, trip.Status);
        Assert.Equal(LoadStatus.Draft, load.Status);
    }

    [Fact]
    public void MarkStopArrived_CancelledTrip_Throws()
    {
        var load = NewLoad();
        var trip = Trip.Create("Trip", _truck, [load]);
        trip.Dispatch();
        trip.Cancel();

        Assert.Throws<InvalidOperationException>(() => trip.MarkStopArrived(StopOf(trip, TripStopType.PickUp).Id));
        Assert.Equal(LoadStatus.Cancelled, load.Status);
    }

    [Fact]
    public void MarkStopArrived_StopAlreadyArrived_Throws()
    {
        var load = NewLoad();
        var other = NewLoad();
        var trip = Trip.Create("Trip", _truck, [load, other]);
        trip.Dispatch();
        var pickup = trip.Stops.First(s => s.Load == load && s.Type == TripStopType.PickUp);
        trip.MarkStopArrived(pickup.Id);
        trip.MarkStopArrived(trip.Stops.First(s => s.Load == load && s.Type == TripStopType.DropOff).Id);

        Assert.Throws<InvalidOperationException>(() => trip.MarkStopArrived(pickup.Id));
        Assert.Equal(LoadStatus.Delivered, load.Status);
    }

    [Fact]
    public void MarkStopArrived_LoadCancelledOnActiveTrip_StaysCancelled()
    {
        var load = NewLoad();
        var other = NewLoad();
        var trip = Trip.Create("Trip", _truck, [load, other]);
        trip.Dispatch();
        load.Cancel();

        trip.MarkStopArrived(trip.Stops.First(s => s.Load == load && s.Type == TripStopType.PickUp).Id);

        Assert.Equal(LoadStatus.Cancelled, load.Status);
    }

    [Fact]
    public void RefreshStatus_DraftTripWithPickedUpLoad_StaysDraft()
    {
        var load = NewLoad();
        var trip = Trip.Create("Trip", _truck, [load]);
        load.Dispatch();
        load.ConfirmPickup();

        trip.RefreshStatus();

        Assert.Equal(TripStatus.Draft, trip.Status);
    }

    #endregion

    #region Status from loads

    private static void Deliver(Trip trip, Load load)
    {
        trip.MarkStopArrived(trip.Stops.First(s => s.Load == load && s.Type == TripStopType.PickUp).Id);
        trip.MarkStopArrived(trip.Stops.First(s => s.Load == load && s.Type == TripStopType.DropOff).Id);
    }

    [Fact]
    public void RefreshStatus_OtherLoadsDeliveredAndOneCancelled_CompletesTrip()
    {
        var cancelled = NewLoad();
        var delivered = NewLoad();
        var trip = Trip.Create("Trip", _truck, [cancelled, delivered]);
        trip.Dispatch();
        cancelled.Cancel();

        Deliver(trip, delivered);

        Assert.Equal(TripStatus.Completed, trip.Status);
        Assert.NotNull(trip.CompletedAt);
    }

    [Fact]
    public void RefreshStatus_LastUndeliveredLoadCancelled_CompletesTrip()
    {
        var delivered = NewLoad();
        var remaining = NewLoad();
        var trip = Trip.Create("Trip", _truck, [delivered, remaining]);
        trip.Dispatch();
        Deliver(trip, delivered);
        Assert.Equal(TripStatus.InTransit, trip.Status);

        remaining.Cancel();
        trip.RefreshStatus();

        Assert.Equal(TripStatus.Completed, trip.Status);
    }

    [Fact]
    public void RefreshStatus_OneLoadCancelledAndOtherPickedUp_StaysInTransit()
    {
        var cancelled = NewLoad();
        var pickedUp = NewLoad();
        var trip = Trip.Create("Trip", _truck, [cancelled, pickedUp]);
        trip.Dispatch();
        trip.MarkStopArrived(trip.Stops.First(s => s.Load == pickedUp && s.Type == TripStopType.PickUp).Id);

        cancelled.Cancel();
        trip.RefreshStatus();

        Assert.Equal(TripStatus.InTransit, trip.Status);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RefreshStatus_EveryLoadCancelled_CancelsTrip(bool dispatched)
    {
        var first = NewLoad();
        var second = NewLoad();
        var trip = Trip.Create("Trip", _truck, [first, second]);
        if (dispatched)
        {
            trip.Dispatch();
        }

        first.Cancel();
        second.Cancel();
        trip.RefreshStatus();

        Assert.Equal(TripStatus.Cancelled, trip.Status);
        Assert.NotNull(trip.CancelledAt);
    }

    #endregion

    #region Cancel

    [Fact]
    public void Cancel_TripWithDeliveredLoad_ThrowsAndChangesNothing()
    {
        var delivered = NewLoad();
        var remaining = NewLoad();
        var trip = Trip.Create("Trip", _truck, [delivered, remaining]);
        trip.Dispatch();
        Deliver(trip, delivered);

        var ex = Assert.Throws<InvalidOperationException>(trip.Cancel);

        Assert.Equal("This trip has delivered loads. Cancel its remaining loads individually instead.", ex.Message);
        Assert.Equal(TripStatus.InTransit, trip.Status);
        Assert.Equal(LoadStatus.Dispatched, remaining.Status);
    }

    [Fact]
    public void Cancel_NoLoadDelivered_CancelsTripAndLoads()
    {
        var load = NewLoad();
        var trip = Trip.Create("Trip", _truck, [load]);
        trip.Dispatch();
        trip.MarkStopArrived(StopOf(trip, TripStopType.PickUp).Id);

        trip.Cancel();

        Assert.Equal(TripStatus.Cancelled, trip.Status);
        Assert.Equal(LoadStatus.Cancelled, load.Status);
    }

    #endregion
}
