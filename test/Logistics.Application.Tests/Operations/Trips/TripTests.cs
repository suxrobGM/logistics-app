using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Xunit;

namespace Logistics.Application.Tests.Operations.Trips;

public class TripTests
{
    private static readonly Address Address = new()
    {
        Line1 = "1 Main", City = "City", State = "ST", ZipCode = "00000", Country = "US"
    };

    private readonly Truck _truck = new() { Number = "TRK-001", Type = TruckType.FreightTruck };

    private readonly Employee _dispatcher = new()
    {
        Email = "d@x.com", FirstName = "Dana", LastName = "Disp", Salary = Money.Zero("USD")
    };

    private Load NewLoad() => Load.Create(
        "Test Load", LoadType.GeneralFreight, 1000, Address, new GeoPoint(0, 0), Address, new GeoPoint(1, 1),
        new Customer { Name = "Acme" }, _truck, _dispatcher, "USD");

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
}
