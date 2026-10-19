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
}
