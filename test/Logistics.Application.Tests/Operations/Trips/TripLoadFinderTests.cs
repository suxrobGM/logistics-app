using Logistics.Application.Modules.Operations.Trips.Services;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Trips;

public class TripLoadFinderTests
{
    private readonly ITenantUnitOfWork _uow = Substitute.For<ITenantUnitOfWork>();
    private readonly TripLoadFinder _sut;

    public TripLoadFinderTests()
    {
        _sut = new TripLoadFinder(_uow);
    }

    [Fact]
    public async Task FindAttachable_UnknownLoad_FailsWithNotFound()
    {
        _uow.Stub<Load>();
        _uow.Stub<TripStop>();

        var result = await _sut.FindAttachableAsync([Guid.NewGuid()], tripId: null, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NotFound, result.ErrorCode);
    }

    [Theory]
    [InlineData(LoadStatus.Delivered)]
    [InlineData(LoadStatus.Cancelled)]
    public async Task FindAttachable_ClosedLoad_Fails(LoadStatus status)
    {
        var load = TestData.Load();
        load.RestoreImportedStatus(status, pickedUpAt: null, deliveredAt: null);
        _uow.Stub(load);
        _uow.Stub<TripStop>();

        var result = await _sut.FindAttachableAsync([load.Id], tripId: null, CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task FindAttachable_LoadOnAnotherTrip_Fails()
    {
        var load = TestData.Load();
        var otherTrip = Trip.Create("Other", TestData.Truck(), [load]);
        _uow.Stub(load);
        _uow.Stub<TripStop>([.. otherTrip.Stops]);

        var result = await _sut.FindAttachableAsync([load.Id], Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("another trip", result.Error);
    }

    [Fact]
    public async Task FindAttachable_LoadOnThisTrip_ReturnsIt()
    {
        var load = TestData.Load();
        var trip = Trip.Create("This", TestData.Truck(), [load]);
        _uow.Stub(load);
        _uow.Stub<TripStop>([.. trip.Stops]);

        var result = await _sut.FindAttachableAsync([load.Id], trip.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal([load], result.Value!);
    }
}
