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

    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly ITenantRepository<Trip, Guid> _tripRepo = Substitute.For<ITenantRepository<Trip, Guid>>();

    private readonly Trip _trip;
    private readonly TripStop _stop;
    private readonly MarkStopDepartedHandler _sut;

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

        _trip = Trip.Create("Trip 1", truck, [load]);
        _stop = _trip.Stops[0];

        _tenantUow.Repository<Trip>().Returns(_tripRepo);
        _tripRepo.GetByIdAsync(_trip.Id, Arg.Any<CancellationToken>()).Returns(_trip);
        _currentUser.IsInRole(Arg.Is<string[]>(r => r.Contains(TenantRoles.Driver))).Returns(true);
        _currentUser.GetUserId().Returns(DriverId);

        _sut = new MarkStopDepartedHandler(_tenantUow, _currentUser, NullLogger<MarkStopDepartedHandler>.Instance);
    }

    private Task<Result> Depart() =>
        _sut.Handle(new MarkStopDepartedCommand { TripId = _trip.Id, StopId = _stop.Id }, CancellationToken.None);

    [Fact]
    public async Task Handle_DriverNotOnTruck_IsRejected()
    {
        _currentUser.GetUserId().Returns(Guid.NewGuid());
        _trip.MarkStopArrived(_stop.Id);

        var result = await Depart();

        Assert.Equal("This trip isn't assigned to your truck.", result.Error);
        Assert.Null(_stop.DepartedAt);
    }

    [Fact]
    public async Task Handle_StopNotArrived_IsRejected()
    {
        var result = await Depart();

        Assert.Equal("Mark the stop as arrived before departing.", result.Error);
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ArrivedStop_SetsDepartedAt()
    {
        _trip.MarkStopArrived(_stop.Id);

        var result = await Depart();

        Assert.True(result.IsSuccess);
        Assert.NotNull(_stop.DepartedAt);
    }
}
