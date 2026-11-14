using Logistics.Application.Modules.Operations.Loads.Services;
using Logistics.Application.Modules.Operations.Trips.Commands;
using Logistics.Application.Modules.Operations.Trips.Services;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Trips;

public class UpdateTripHandlerTests
{
    private readonly ITenantUnitOfWork _uow = Substitute.For<ITenantUnitOfWork>();
    private readonly Truck _truck = TestData.Truck();
    private readonly Trip _trip;
    private readonly UpdateTripHandler _sut;

    public UpdateTripHandlerTests()
    {
        _trip = Trip.Create("Trip", _truck, [TestData.Load()]);
        _uow.Stub(_trip);
        _sut = new UpdateTripHandler(_uow, Substitute.For<ILoadService>(), new TripLoadFinder(_uow), NullLogger<UpdateTripHandler>.Instance);
    }

    [Fact]
    public async Task Handle_AttachedLoad_AddsItToTheTripWithTheTripsTruck()
    {
        var load = TestData.Load();
        _uow.Stub(load);
        var stopRepo = _uow.Stub<TripStop>([.. _trip.Stops]);

        var result = await _sut.Handle(new UpdateTripCommand { TripId = _trip.Id, AttachedLoadIds = [load.Id] }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Contains(load, _trip.GetLoads());
        Assert.Equal(_truck.Id, load.AssignedTruckId);
        await stopRepo.Received(1).AddRangeAsync(
            Arg.Is<IEnumerable<TripStop>>(stops => stops.Count() == 2 && stops.All(s => s.LoadId == load.Id)),
            Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownAttachedLoad_FailsWithNotFound()
    {
        _uow.Stub<Load>();
        _uow.Stub<TripStop>([.. _trip.Stops]);

        var result = await _sut.Handle(new UpdateTripCommand { TripId = _trip.Id, AttachedLoadIds = [Guid.NewGuid()] }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NotFound, result.ErrorCode);
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
