using System.Linq.Expressions;
using Logistics.Application.Modules.Integrations.Negotiation.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Specifications;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Negotiation;

public class LaneRateFloorResolverTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<LaneRateFloor, Guid> floorRepo =
        Substitute.For<ITenantRepository<LaneRateFloor, Guid>>();
    private readonly Tenant tenant = new()
    {
        Name = "test",
        ConnectionString = "test",
        BillingEmail = "billing@test.com",
        CompanyAddress = new Address { Line1 = "1 St", City = "Test", State = "TX", ZipCode = "00000", Country = "US" }
    };

    private readonly LaneRateFloorResolver sut;

    public LaneRateFloorResolverTests()
    {
        tenantUow.Repository<LaneRateFloor>().Returns(floorRepo);
        tenantUow.GetCurrentTenant().Returns(tenant);
        sut = new LaneRateFloorResolver(tenantUow);
    }

    private static LoadBoardListing Listing(
        string originState = "TX",
        string destinationState = "IL",
        string originCountry = "US",
        string destinationCountry = "US",
        double? distance = 900,
        decimal? ratePerMile = 2.00m,
        Money? totalRate = null) => new()
    {
        ExternalListingId = "EXT-1",
        ProviderType = LoadBoardProviderType.Demo,
        OriginAddress = new Address
        {
            Line1 = "1 St", City = "Dallas", State = originState, ZipCode = "75001", Country = originCountry
        },
        OriginLocation = new GeoPoint(-96.8, 32.8),
        DestinationAddress = new Address
        {
            Line1 = "2 St", City = "Chicago", State = destinationState, ZipCode = "60601", Country = destinationCountry
        },
        DestinationLocation = new GeoPoint(-87.6, 41.9),
        RatePerMile = ratePerMile,
        TotalRate = totalRate,
        Distance = distance,
        ExpiresAt = DateTime.UtcNow.AddDays(1)
    };

    private void SetupLanes(params LaneRateFloor[] lanes) =>
        floorRepo.GetListAsync(Arg.Any<ISpecification<LaneRateFloor>?>(), Arg.Any<CancellationToken>())
            .Returns(lanes.ToList());

    [Fact]
    public async Task ResolveAsync_ExactAndPartialLanesConfigured_ExactWins()
    {
        var exact = new LaneRateFloor { OriginState = "TX", DestinationState = "IL", MinRatePerMile = 3.00m };
        var originAny = new LaneRateFloor { OriginState = "TX", DestinationState = null, MinRatePerMile = 1.50m };
        var destinationAny = new LaneRateFloor { OriginState = null, DestinationState = "IL", MinRatePerMile = 1.00m };
        SetupLanes(exact, originAny, destinationAny);

        var result = await sut.ResolveAsync(Listing(), CancellationToken.None);

        Assert.True(result.HasFloor);
        Assert.Equal(RateFloorSource.LaneExact, result.Source);
        Assert.Equal(3.00m, result.MinRatePerMile);
    }

    [Fact]
    public async Task ResolveAsync_OnlyOriginAnyAndDestinationAnyConfigured_OriginAnyWinsOverDestinationAny()
    {
        var originAny = new LaneRateFloor { OriginState = "TX", DestinationState = null, MinRatePerMile = 1.50m };
        var destinationAny = new LaneRateFloor { OriginState = null, DestinationState = "IL", MinRatePerMile = 1.00m };
        SetupLanes(originAny, destinationAny);

        var result = await sut.ResolveAsync(Listing(), CancellationToken.None);

        Assert.Equal(RateFloorSource.LaneOriginAny, result.Source);
    }

    [Fact]
    public async Task ResolveAsync_OnlyDestinationAnyConfigured_DestinationAnyMatches()
    {
        var destinationAny = new LaneRateFloor { OriginState = null, DestinationState = "IL", MinRatePerMile = 1.00m };
        SetupLanes(destinationAny);

        var result = await sut.ResolveAsync(Listing(), CancellationToken.None);

        Assert.Equal(RateFloorSource.LaneDestinationAny, result.Source);
    }

    [Fact]
    public async Task ResolveAsync_ListingStatesLowerCaseWithWhitespace_StillMatchesUpperCaseLane()
    {
        var exact = new LaneRateFloor { OriginState = "TX", DestinationState = "IL", MinRatePerMile = 3.00m };
        SetupLanes(exact);

        var result = await sut.ResolveAsync(
            Listing(originState: " tx ", destinationState: " il "), CancellationToken.None);

        Assert.Equal(RateFloorSource.LaneExact, result.Source);
    }

    [Fact]
    public async Task ResolveAsync_NoLaneMatch_FallsBackToTenantDefault()
    {
        SetupLanes();
        tenant.Settings.DefaultRateFloorPerMile = 1.75m;

        var result = await sut.ResolveAsync(Listing(), CancellationToken.None);

        Assert.True(result.HasFloor);
        Assert.Equal(RateFloorSource.TenantDefault, result.Source);
        Assert.Equal(1.75m, result.MinRatePerMile);
        Assert.Null(result.MinTotalRate);
    }

    [Fact]
    public async Task ResolveAsync_NoLaneMatchAndNoTenantDefault_HasFloorFalse()
    {
        SetupLanes();

        var result = await sut.ResolveAsync(Listing(), CancellationToken.None);

        Assert.False(result.HasFloor);
        Assert.Equal(RateFloorSource.None, result.Source);
        Assert.Null(result.MinRatePerMile);
    }

    [Fact]
    public async Task ResolveAsync_ListingRateBelowFloor_FlagsBelowFloorWithPositiveGap()
    {
        var lane = new LaneRateFloor { OriginState = "TX", DestinationState = "IL", MinRatePerMile = 3.00m };
        SetupLanes(lane);

        var result = await sut.ResolveAsync(
            Listing(distance: 1000, ratePerMile: 2.00m), CancellationToken.None);

        Assert.True(result.ListingBelowFloor);
        Assert.Equal(1.00m, result.GapPerMile);
    }

    [Fact]
    public async Task ResolveAsync_ListingRateAtOrAboveFloor_NotBelowFloor()
    {
        var lane = new LaneRateFloor { OriginState = "TX", DestinationState = "IL", MinRatePerMile = 2.00m };
        SetupLanes(lane);

        var result = await sut.ResolveAsync(
            Listing(distance: 1000, ratePerMile: 2.50m), CancellationToken.None);

        Assert.False(result.ListingBelowFloor);
    }

    [Fact]
    public async Task ResolveAsync_MissingDistanceWithOnlyPerMileFloor_ComparesPerMileDirectly()
    {
        var lane = new LaneRateFloor { OriginState = "TX", DestinationState = "IL", MinRatePerMile = 3.00m };
        SetupLanes(lane);

        var result = await sut.ResolveAsync(
            Listing(distance: null, ratePerMile: 2.00m), CancellationToken.None);

        Assert.True(result.ListingBelowFloor);
        Assert.Equal(1.00m, result.GapPerMile);
    }

    [Fact]
    public async Task ResolveAsync_MissingDistanceWithMinTotalRate_UsesMinTotalRateAloneAndNoGapPerMile()
    {
        var lane = new LaneRateFloor
        {
            OriginState = "TX", DestinationState = "IL", MinRatePerMile = 3.00m,
            MinTotalRate = new Money { Amount = 1000m, Currency = "USD" }
        };
        SetupLanes(lane);

        var result = await sut.ResolveAsync(
            Listing(distance: null, ratePerMile: null, totalRate: new Money { Amount = 800m, Currency = "USD" }),
            CancellationToken.None);

        Assert.True(result.ListingBelowFloor);
        Assert.Null(result.GapPerMile);
    }
}
