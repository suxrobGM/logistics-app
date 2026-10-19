using System.Net;
using Logistics.Application.Abstractions.LoadBoard;
using Logistics.Application.Modules.Integrations.LoadBoard.Commands;
using Logistics.Application.Modules.Integrations.LoadBoard.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Logistics.Application.Tests.LoadBoard;

/// <summary>
/// Runs through the real gateway so a provider's vendor error is checked all the way to the
/// per-provider error the dispatcher sees.
/// </summary>
public class SearchLoadBoardHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ILoadBoardProviderFactory _factory = Substitute.For<ILoadBoardProviderFactory>();
    private readonly ILoadBoardProviderService _demo = Substitute.For<ILoadBoardProviderService>();
    private readonly ILoadBoardProviderService _dat = Substitute.For<ILoadBoardProviderService>();
    private readonly ITenantRepository<LoadBoardConfiguration, Guid> _configRepo =
        Substitute.For<ITenantRepository<LoadBoardConfiguration, Guid>>();
    private readonly ITenantRepository<LoadBoardListing, Guid> _listingRepo =
        Substitute.For<ITenantRepository<LoadBoardListing, Guid>>();
    private readonly SearchLoadBoardHandler _sut;

    private readonly LoadBoardConfiguration _demoConfig = new()
    {
        ProviderType = LoadBoardProviderType.Demo,
        ApiKey = "demo",
        AccessToken = "demo-token",
        TokenExpiresAt = DateTime.UtcNow.AddHours(1)
    };
    private readonly LoadBoardConfiguration _datConfig = new()
    {
        ProviderType = LoadBoardProviderType.Dat,
        ApiKey = "svc@carrier.example",
        AccessToken = "user-token",
        TokenExpiresAt = DateTime.UtcNow.AddHours(1)
    };

    public SearchLoadBoardHandlerTests()
    {
        _tenantUow.Repository<LoadBoardConfiguration>().Returns(_configRepo);
        _tenantUow.Repository<LoadBoardListing>().Returns(_listingRepo);
        _configRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<LoadBoardConfiguration, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns([_demoConfig, _datConfig]);
        _listingRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<LoadBoardListing, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        _factory.IsProviderSupported(Arg.Any<LoadBoardProviderType>()).Returns(true);
        _factory.GetProvider(LoadBoardProviderType.Demo).Returns(_demo);
        _factory.GetProvider(LoadBoardProviderType.Dat).Returns(_dat);

        var gateway = new LoadBoardProviderGateway(_tenantUow, _factory, NullLogger<LoadBoardProviderGateway>.Instance);
        _sut = new SearchLoadBoardHandler(_tenantUow, gateway, NullLogger<SearchLoadBoardHandler>.Instance);
    }

    private static LoadBoardListingDto CreateListing(string externalId, DateTime? expiresAt = null) => new()
    {
        ExternalListingId = externalId,
        ProviderType = LoadBoardProviderType.Demo,
        OriginAddress = new Address { Line1 = "", City = "Dallas", State = "TX", ZipCode = "", Country = "US" },
        OriginLocation = new GeoPoint(-96.8, 32.8),
        DestinationAddress = new Address { Line1 = "", City = "Chicago", State = "IL", ZipCode = "", Country = "US" },
        DestinationLocation = new GeoPoint(-87.6, 41.9),
        RatePerMile = 2.5m,
        EquipmentType = LoadBoardEquipmentType.Van,
        ExpiresAt = expiresAt
    };

    [Fact]
    public async Task Handle_ProviderApiFails_SurfacesVendorErrorAndKeepsOtherListings()
    {
        _demo.SearchLoadsAsync(Arg.Any<LoadBoardSearchCriteria>(), Arg.Any<CancellationToken>())
            .Returns([CreateListing("EXT-1"), CreateListing("EXT-2")]);
        _dat.SearchLoadsAsync(Arg.Any<LoadBoardSearchCriteria>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new LoadBoardApiException("DAT returned 400 BadRequest - invalid origin", HttpStatusCode.BadRequest));

        var result = await _sut.Handle(new SearchLoadBoardCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.TotalCount);
        Assert.Equal(0, result.Value.CountByProvider[LoadBoardProviderType.Dat]);
        Assert.Equal("DAT: DAT returned 400 BadRequest - invalid origin", result.Value.Errors![LoadBoardProviderType.Dat]);
    }

    [Fact]
    public async Task Handle_AllProvidersHealthy_NoErrors()
    {
        _demo.SearchLoadsAsync(Arg.Any<LoadBoardSearchCriteria>(), Arg.Any<CancellationToken>())
            .Returns([CreateListing("EXT-1")]);
        _dat.SearchLoadsAsync(Arg.Any<LoadBoardSearchCriteria>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await _sut.Handle(new SearchLoadBoardCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.Errors);
    }

    [Fact]
    public async Task Handle_ListingWithoutExpiry_GetsAShelfLifeInsteadOfNever()
    {
        _demo.SearchLoadsAsync(Arg.Any<LoadBoardSearchCriteria>(), Arg.Any<CancellationToken>())
            .Returns([CreateListing("EXT-1")]);
        _dat.SearchLoadsAsync(Arg.Any<LoadBoardSearchCriteria>(), Arg.Any<CancellationToken>()).Returns([]);

        var result = await _sut.Handle(new SearchLoadBoardCommand(), CancellationToken.None);

        var expiresAt = Assert.Single(result.Value!.Listings).ExpiresAt;
        Assert.NotNull(expiresAt);
        Assert.InRange(expiresAt.Value, DateTime.UtcNow.AddHours(23), DateTime.UtcNow.AddHours(25));
        await _listingRepo.Received(1).AddAsync(
            Arg.Is<LoadBoardListing>(l => l.ExpiresAt == expiresAt.Value), Arg.Any<CancellationToken>());
    }

    /// <summary>An expired listing the board still lists used to keep Expired, so booking and countering refused it.</summary>
    [Theory]
    [InlineData(LoadBoardListingStatus.Expired, LoadBoardListingStatus.Available)]
    [InlineData(LoadBoardListingStatus.Booked, LoadBoardListingStatus.Booked)]
    public async Task Handle_ListingFoundAgain_ReopensOnlyAnExpiredOne(
        LoadBoardListingStatus stored, LoadBoardListingStatus expected)
    {
        var listing = new LoadBoardListing
        {
            ExternalListingId = "EXT-1",
            ProviderType = LoadBoardProviderType.Demo,
            OriginAddress = new Address { Line1 = "", City = "Dallas", State = "TX", ZipCode = "", Country = "US" },
            OriginLocation = new GeoPoint(-96.8, 32.8),
            DestinationAddress = new Address { Line1 = "", City = "Chicago", State = "IL", ZipCode = "", Country = "US" },
            DestinationLocation = new GeoPoint(-87.6, 41.9),
            Status = stored,
            ExpiresAt = DateTime.UtcNow.AddHours(-1)
        };
        _listingRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<LoadBoardListing, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns([listing]);
        _demo.SearchLoadsAsync(Arg.Any<LoadBoardSearchCriteria>(), Arg.Any<CancellationToken>())
            .Returns([CreateListing("EXT-1", DateTime.UtcNow.AddHours(6))]);
        _dat.SearchLoadsAsync(Arg.Any<LoadBoardSearchCriteria>(), Arg.Any<CancellationToken>()).Returns([]);

        var result = await _sut.Handle(new SearchLoadBoardCommand(), CancellationToken.None);

        Assert.Equal(expected, listing.Status);
        Assert.Equal(expected, Assert.Single(result.Value!.Listings).Status);
    }

    [Fact]
    public async Task Handle_MinRatePerMile_DropsCheaperListingsButKeepsUnpriced()
    {
        var cheap = CreateListing("CHEAP") with { RatePerMile = 1.5m };
        var unpriced = CreateListing("UNPRICED") with { RatePerMile = null };
        _demo.SearchLoadsAsync(Arg.Any<LoadBoardSearchCriteria>(), Arg.Any<CancellationToken>())
            .Returns([CreateListing("GOOD"), cheap, unpriced]);
        _dat.SearchLoadsAsync(Arg.Any<LoadBoardSearchCriteria>(), Arg.Any<CancellationToken>()).Returns([]);

        var result = await _sut.Handle(new SearchLoadBoardCommand { MinRatePerMile = 2m }, CancellationToken.None);

        Assert.Equal(["GOOD", "UNPRICED"], result.Value!.Listings.Select(l => l.ExternalListingId).Order());
    }

    [Fact]
    public async Task Handle_MarketRatesEnabled_StampsLaneRateOnListings()
    {
        _datConfig.MarketRatesEnabled = true;
        _demo.SearchLoadsAsync(Arg.Any<LoadBoardSearchCriteria>(), Arg.Any<CancellationToken>()).Returns([]);
        _dat.SearchLoadsAsync(Arg.Any<LoadBoardSearchCriteria>(), Arg.Any<CancellationToken>())
            .Returns([CreateListing("A") with { ProviderType = LoadBoardProviderType.Dat }]);
        _dat.GetMarketRatesAsync(Arg.Any<IReadOnlyList<MarketRateLane>>(), Arg.Any<CancellationToken>())
            .Returns([new MarketRate(2.75m, 2500m, 2.4m, 3.1m)]);

        var result = await _sut.Handle(new SearchLoadBoardCommand(), CancellationToken.None);

        var listing = Assert.Single(result.Value!.Listings);
        Assert.Equal(2.75m, listing.MarketRatePerMile);
        Assert.Null(result.Value.Errors);
    }

    [Fact]
    public async Task Handle_NoConfiguredProviders_Fails()
    {
        _configRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<LoadBoardConfiguration, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await _sut.Handle(new SearchLoadBoardCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
