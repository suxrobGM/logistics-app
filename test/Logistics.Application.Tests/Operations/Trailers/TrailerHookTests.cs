using System.Linq.Expressions;
using Logistics.Application.Modules.Operations.Trailers.Commands;
using Logistics.Application.Modules.Operations.Trailers.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Trailers;

public class TrailerHookTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<Truck, Guid> _truckRepo = Substitute.For<ITenantRepository<Truck, Guid>>();
    private readonly ITenantRepository<Trailer, Guid> _trailerRepo = Substitute.For<ITenantRepository<Trailer, Guid>>();
    private readonly TrailerHookService _sut;

    public TrailerHookTests()
    {
        _tenantUow.Repository<Truck>().Returns(_truckRepo);
        _tenantUow.Repository<Trailer>().Returns(_trailerRepo);
        _sut = new TrailerHookService(_tenantUow);
    }

    #region Truck

    [Fact]
    public void HookTrailer_ActiveTrailer_Hooks()
    {
        var truck = Truck("TRK-1");
        var trailer = Trailer(TrailerStatus.Active);

        truck.HookTrailer(trailer);

        Assert.Same(trailer, truck.CurrentTrailer);
        Assert.Equal(trailer.Id, truck.CurrentTrailerId);
    }

    [Theory]
    [InlineData(TrailerStatus.OutOfService)]
    [InlineData(TrailerStatus.Inactive)]
    public void HookTrailer_NotActive_Throws(TrailerStatus status)
    {
        var truck = Truck("TRK-1");

        Assert.Throws<InvalidOperationException>(() => truck.HookTrailer(Trailer(status)));
        Assert.Null(truck.CurrentTrailerId);
    }

    [Fact]
    public void HookTrailer_Null_Unhooks()
    {
        var truck = Truck("TRK-1");
        truck.HookTrailer(Trailer(TrailerStatus.Active));

        truck.HookTrailer(null);

        Assert.Null(truck.CurrentTrailer);
        Assert.Null(truck.CurrentTrailerId);
    }

    #endregion

    #region Trip

    [Fact]
    public void Create_TruckWithTrailer_TripTakesTheTrailer()
    {
        var truck = Truck("TRK-1");
        var trailer = Trailer(TrailerStatus.Active);
        truck.HookTrailer(trailer);

        var trip = Trip.Create("Trip", truck);

        Assert.Equal(trailer.Id, trip.TrailerId);
    }

    [Fact]
    public void AssignToTruck_TripTakesTheNewTrucksTrailer()
    {
        var trip = Trip.Create("Trip", Truck("TRK-1"));
        var newTruck = Truck("TRK-2");
        var trailer = Trailer(TrailerStatus.Active);
        newTruck.HookTrailer(trailer);

        trip.AssignToTruck(newTruck);

        Assert.Equal(trailer.Id, trip.TrailerId);
    }

    [Fact]
    public void AssignTrailer_NotActive_Throws()
    {
        var trip = Trip.Create("Trip", Truck("TRK-1"));

        Assert.Throws<InvalidOperationException>(() => trip.AssignTrailer(Trailer(TrailerStatus.OutOfService)));
        Assert.Null(trip.TrailerId);
    }

    [Fact]
    public void AssignTrailer_Active_OverridesTheTrucksTrailer()
    {
        var truck = Truck("TRK-1");
        truck.HookTrailer(Trailer(TrailerStatus.Active));
        var trip = Trip.Create("Trip", truck);
        var other = Trailer(TrailerStatus.Active);

        trip.AssignTrailer(other);

        Assert.Equal(other.Id, trip.TrailerId);
    }

    #endregion

    #region Hook service

    [Fact]
    public async Task HookAsync_TrailerOnAnotherTruck_UnhooksItThereFirst()
    {
        var trailer = Trailer(TrailerStatus.Active);
        var previousTruck = Truck("TRK-1");
        previousTruck.HookTrailer(trailer);
        var truck = Truck("TRK-2");
        _truckRepo.GetAsync(Arg.Any<Expression<Func<Truck, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(previousTruck);

        await _sut.HookAsync(truck, trailer);

        Assert.Null(previousTruck.CurrentTrailerId);
        Assert.Equal(trailer.Id, truck.CurrentTrailerId);
        await _tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HookAsync_NotActive_ThrowsAndLeavesTheOtherTruckAlone()
    {
        var trailer = Trailer(TrailerStatus.Active);
        var previousTruck = Truck("TRK-1");
        previousTruck.HookTrailer(trailer);
        trailer.Status = TrailerStatus.OutOfService;
        _truckRepo.GetAsync(Arg.Any<Expression<Func<Truck, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(previousTruck);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.HookAsync(Truck("TRK-2"), trailer));

        Assert.Equal(trailer.Id, previousTruck.CurrentTrailerId);
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UnhookUnlessActiveAsync_Retired_UnhooksFromItsTruck()
    {
        var trailer = Trailer(TrailerStatus.Active);
        var truck = Truck("TRK-1");
        truck.HookTrailer(trailer);
        trailer.Status = TrailerStatus.Inactive;
        _truckRepo.GetAsync(Arg.Any<Expression<Func<Truck, bool>>>(), Arg.Any<CancellationToken>()).Returns(truck);

        await _sut.UnhookUnlessActiveAsync(trailer);

        Assert.Null(truck.CurrentTrailerId);
    }

    [Fact]
    public async Task UnhookUnlessActiveAsync_Active_DoesNotLookUpTrucks()
    {
        await _sut.UnhookUnlessActiveAsync(Trailer(TrailerStatus.Active));

        await _truckRepo.DidNotReceive()
            .GetAsync(Arg.Any<Expression<Func<Truck, bool>>>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region Update trailer

    [Fact]
    public async Task UpdateTrailer_RetiringAHookedTrailer_UnhooksIt()
    {
        var trailer = Trailer(TrailerStatus.Active);
        var truck = Truck("TRK-1");
        truck.HookTrailer(trailer);
        _trailerRepo.GetByIdAsync(trailer.Id, Arg.Any<CancellationToken>()).Returns(trailer);
        _trailerRepo.GetAsync(Arg.Any<Expression<Func<Trailer, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((Trailer?)null);
        _truckRepo.GetAsync(Arg.Any<Expression<Func<Truck, bool>>>(), Arg.Any<CancellationToken>()).Returns(truck);
        var handler = new UpdateTrailerHandler(_tenantUow, _sut);

        var result = await handler.Handle(new UpdateTrailerCommand
        {
            Id = trailer.Id,
            Number = trailer.Number,
            Type = trailer.Type,
            Status = TrailerStatus.Inactive
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(truck.CurrentTrailerId);
    }

    #endregion

    private static Truck Truck(string number) => new() { Number = number, Type = TruckType.FreightTruck };

    private static Trailer Trailer(TrailerStatus status) =>
        new() { Number = "TR-1", Type = TrailerType.DryVan, Status = status };
}
