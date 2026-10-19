using Logistics.Application.Modules.Operations.Trips.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Trips;

public class CancelTripHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<Trip, Guid> _tripRepo = Substitute.For<ITenantRepository<Trip, Guid>>();
    private readonly CancelTripHandler _sut;

    public CancelTripHandlerTests()
    {
        _tenantUow.Repository<Trip>().Returns(_tripRepo);
        _sut = new CancelTripHandler(_tenantUow, NullLogger<CancelTripHandler>.Instance);
    }

    [Fact]
    public async Task Handle_TripWithDeliveredLoad_FailsWithClearMessageAndSavesNothing()
    {
        var truck = new Truck { Number = "TRK-001", Type = TruckType.FreightTruck };
        var dispatcher = new Employee
        {
            Email = "d@x.com", FirstName = "Dana", LastName = "Disp", Salary = Money.Zero("USD")
        };
        var address = new Address { Line1 = "1 Main", City = "City", State = "ST", ZipCode = "00000", Country = "US" };
        Load NewLoad() => Load.Create($"Load {Guid.NewGuid():N}", LoadType.GeneralFreight, 1000, address, new GeoPoint(0, 0), address,
            new GeoPoint(1, 1), new Customer { Name = "Acme" }, truck, dispatcher, "USD");

        var delivered = NewLoad();
        var trip = Trip.Create("Trip", truck, [delivered, NewLoad()]);
        trip.Dispatch();
        trip.MarkStopArrived(trip.Stops.First(s => s.Load == delivered && s.Type == TripStopType.DropOff).Id);
        _tripRepo.GetByIdAsync(trip.Id, Arg.Any<CancellationToken>()).Returns(trip);

        var result = await _sut.Handle(new CancelTripCommand { TripId = trip.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("This trip has delivered loads. Cancel its remaining loads individually instead.", result.Error);
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
