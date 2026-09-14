using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Operations.Trips.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Identity.Roles;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Trips;

public class MarkStopDepartedHandlerTests
{
    private static readonly Guid DriverId = Guid.NewGuid();

    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ICurrentUserService currentUser = Substitute.For<ICurrentUserService>();
    private readonly ITenantRepository<Trip, Guid> tripRepo = Substitute.For<ITenantRepository<Trip, Guid>>();

    private readonly Trip trip;
    private readonly TripStop stop;
    private readonly MarkStopDepartedHandler sut;

    public MarkStopDepartedHandlerTests()
    {
        var truck = new Truck { Number = "TRK-001", Type = TruckType.FreightTruck, MainDriverId = DriverId };
        var address = new Address { Line1 = "1 Main", City = "City", State = "ST", ZipCode = "00000", Country = "US" };
        var load = new Load
        {
            Name = "Test Load",
            Type = LoadType.GeneralFreight,
            Customer = null!,
            OriginAddress = address,
            OriginLocation = new GeoPoint(0, 0),
            DestinationAddress = address,
            DestinationLocation = new GeoPoint(0, 0),
            DeliveryCost = Money.Zero("USD")
        };

        trip = Trip.Create("Trip 1", truck, [load]);
        stop = trip.Stops[0];

        tenantUow.Repository<Trip>().Returns(tripRepo);
        tripRepo.GetByIdAsync(trip.Id, Arg.Any<CancellationToken>()).Returns(trip);
        currentUser.IsInRole(Arg.Is<string[]>(r => r.Contains(TenantRoles.Driver))).Returns(true);
        currentUser.GetUserId().Returns(DriverId);

        sut = new MarkStopDepartedHandler(tenantUow, currentUser, NullLogger<MarkStopDepartedHandler>.Instance);
    }

    private Task<Result> Depart() =>
        sut.Handle(new MarkStopDepartedCommand { TripId = trip.Id, StopId = stop.Id }, CancellationToken.None);

    [Fact]
    public async Task Handle_DriverNotOnTruck_IsRejected()
    {
        currentUser.GetUserId().Returns(Guid.NewGuid());
        trip.MarkStopArrived(stop.Id);

        var result = await Depart();

        Assert.Equal("This trip isn't assigned to your truck.", result.Error);
        Assert.Null(stop.DepartedAt);
    }

    [Fact]
    public async Task Handle_StopNotArrived_IsRejected()
    {
        var result = await Depart();

        Assert.Equal("Mark the stop as arrived before departing.", result.Error);
        await tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ArrivedStop_SetsDepartedAt()
    {
        trip.MarkStopArrived(stop.Id);

        var result = await Depart();

        Assert.True(result.IsSuccess);
        Assert.NotNull(stop.DepartedAt);
    }
}
