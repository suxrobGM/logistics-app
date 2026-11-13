using System.Linq.Expressions;
using Logistics.Application.Modules.AI.Negotiation.Services;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AI.Negotiation;

public class LaneRateFloorResolverTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<LaneRateFloor, Guid> _floorRepo =
        Substitute.For<ITenantRepository<LaneRateFloor, Guid>>();
    private readonly Tenant _tenant = TestTenant.Create();

    private readonly LaneRateFloorResolver _sut;

    public LaneRateFloorResolverTests()
    {
        _tenantUow.Repository<LaneRateFloor>().Returns(_floorRepo);
        _tenantUow.GetCurrentTenant().Returns(_tenant);
        _sut = new LaneRateFloorResolver(_tenantUow);
    }

    private static LoadBoardListing Listing(
        string originState = "TX",
        string destinationState = "IL",
        string originCountry = "US",
        string destinationCountry = "US",
        double? distance = 900,
        decimal? ratePerKm = 2.00m,
        Money? totalRate = null) => new()
        {
            ExternalListingId = "EXT-1",
            ProviderType = LoadBoardProviderType.Demo,
            OriginAddress = new Address
            {
                Line1 = "1 St",
                City = "Dallas",
                State = originState,
                ZipCode = "75001",
                Country = originCountry
            },
            OriginLocation = new GeoPoint(-96.8, 32.8),
            DestinationAddress = new Address
            {
                Line1 = "2 St",
                City = "Chicago",
                State = destinationState,
                ZipCode = "60601",
                Country = destinationCountry
            },
            DestinationLocation = new GeoPoint(-87.6, 41.9),
            RatePerKm = ratePerKm,
            TotalRate = totalRate,
            Distance = distance,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

    private void SetupLanes(params LaneRateFloor[] lanes) =>
        _floorRepo.GetListAsync(Arg.Any<CancellationToken>())
            .Returns(lanes.ToList());

    [Fact]
    public async Task ResolveAsync_ExactAndPartialLanesConfigured_ExactWins()
    {
        var exact = new LaneRateFloor { OriginState = "TX", DestinationState = "IL", MinRatePerKm = 3.00m };
        var originAny = new LaneRateFloor { OriginState = "TX", DestinationState = null, MinRatePerKm = 1.50m };
        var destinationAny = new LaneRateFloor { OriginState = null, DestinationState = "IL", MinRatePerKm = 1.00m };
        SetupLanes(exact, originAny, destinationAny);

        var result = await _sut.ResolveAsync(Listing(), CancellationToken.None);

        Assert.True(result.HasFloor);
        Assert.Equal(RateFloorSource.LaneExact, result.Source);
        Assert.Equal(3.00m, result.MinRatePerKm);
    }

    [Fact]
    public async Task ResolveAsync_OnlyOriginAnyAndDestinationAnyConfigured_OriginAnyWinsOverDestinationAny()
    {
        var originAny = new LaneRateFloor { OriginState = "TX", DestinationState = null, MinRatePerKm = 1.50m };
        var destinationAny = new LaneRateFloor { OriginState = null, DestinationState = "IL", MinRatePerKm = 1.00m };
        SetupLanes(originAny, destinationAny);

        var result = await _sut.ResolveAsync(Listing(), CancellationToken.None);

        Assert.Equal(RateFloorSource.LaneOriginAny, result.Source);
    }

    [Fact]
    public async Task ResolveAsync_OnlyDestinationAnyConfigured_DestinationAnyMatches()
    {
        var destinationAny = new LaneRateFloor { OriginState = null, DestinationState = "IL", MinRatePerKm = 1.00m };
        SetupLanes(destinationAny);

        var result = await _sut.ResolveAsync(Listing(), CancellationToken.None);

        Assert.Equal(RateFloorSource.LaneDestinationAny, result.Source);
    }

    [Fact]
    public async Task ResolveAsync_ListingStatesLowerCaseWithWhitespace_StillMatchesUpperCaseLane()
    {
        var exact = new LaneRateFloor { OriginState = "TX", DestinationState = "IL", MinRatePerKm = 3.00m };
        SetupLanes(exact);

        var result = await _sut.ResolveAsync(
            Listing(originState: " tx ", destinationState: " il "), CancellationToken.None);

        Assert.Equal(RateFloorSource.LaneExact, result.Source);
    }

    [Fact]
    public async Task ResolveAsync_NoLaneMatch_FallsBackToTenantDefault()
    {
        SetupLanes();
        _tenant.Settings.DefaultRateFloorPerKm = 1.75m;

        var result = await _sut.ResolveAsync(Listing(), CancellationToken.None);

        Assert.True(result.HasFloor);
        Assert.Equal(RateFloorSource.TenantDefault, result.Source);
        Assert.Equal(1.75m, result.MinRatePerKm);
        Assert.Null(result.MinTotalRate);
    }

    [Fact]
    public async Task ResolveAsync_NoLaneMatchAndNoTenantDefault_HasFloorFalse()
    {
        SetupLanes();

        var result = await _sut.ResolveAsync(Listing(), CancellationToken.None);

        Assert.False(result.HasFloor);
        Assert.Equal(RateFloorSource.None, result.Source);
        Assert.Null(result.MinRatePerKm);
    }

    [Fact]
    public async Task ResolveAsync_ListingRateBelowFloor_FlagsBelowFloorWithPositiveGap()
    {
        var lane = new LaneRateFloor { OriginState = "TX", DestinationState = "IL", MinRatePerKm = 3.00m };
        SetupLanes(lane);

        var result = await _sut.ResolveAsync(
            Listing(distance: 1000, ratePerKm: 2.00m), CancellationToken.None);

        Assert.True(result.ListingBelowFloor);
        Assert.Equal(1.00m, result.GapPerKm);
    }

    [Fact]
    public async Task ResolveAsync_ListingRateAtOrAboveFloor_NotBelowFloor()
    {
        var lane = new LaneRateFloor { OriginState = "TX", DestinationState = "IL", MinRatePerKm = 2.00m };
        SetupLanes(lane);

        var result = await _sut.ResolveAsync(
            Listing(distance: 1000, ratePerKm: 2.50m), CancellationToken.None);

        Assert.False(result.ListingBelowFloor);
    }

    [Fact]
    public async Task ResolveAsync_MissingDistanceWithOnlyPerKmFloor_ComparesPerKmDirectly()
    {
        var lane = new LaneRateFloor { OriginState = "TX", DestinationState = "IL", MinRatePerKm = 3.00m };
        SetupLanes(lane);

        var result = await _sut.ResolveAsync(
            Listing(distance: null, ratePerKm: 2.00m), CancellationToken.None);

        Assert.True(result.ListingBelowFloor);
        Assert.Equal(1.00m, result.GapPerKm);
    }

    [Fact]
    public async Task ResolveAsync_MissingDistanceWithMinTotalRate_UsesMinTotalRateAloneAndNoGapPerKm()
    {
        var lane = new LaneRateFloor
        {
            OriginState = "TX",
            DestinationState = "IL",
            MinRatePerKm = 3.00m,
            MinTotalRate = 1000m
        };
        SetupLanes(lane);

        var result = await _sut.ResolveAsync(
            Listing(distance: null, ratePerKm: null, totalRate: new Money { Amount = 800m, Currency = "USD" }),
            CancellationToken.None);

        Assert.True(result.ListingBelowFloor);
        Assert.Null(result.GapPerKm);
    }
}
