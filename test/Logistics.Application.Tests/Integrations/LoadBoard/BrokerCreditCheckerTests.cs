using Logistics.Application.Abstractions.LoadBoard;
using Logistics.Application.Modules.Integrations.LoadBoard.Services;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Integrations.LoadBoard;

public class BrokerCreditCheckerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IBrokerCreditService _brokerCreditService = Substitute.For<IBrokerCreditService>();
    private readonly Tenant _tenant = TestTenant.Create();
    private readonly BrokerCreditChecker _sut;

    public BrokerCreditCheckerTests()
    {
        _tenantUow.GetCurrentTenant().Returns(_tenant);
        _sut = new BrokerCreditChecker(_tenantUow, _brokerCreditService);
    }

    [Theory]
    [InlineData(null, 10, true, false)]
    [InlineData(70, 70, true, false)]
    [InlineData(70, 50, true, true)]
    [InlineData(null, 90, false, true)]
    public async Task CheckAsync_ScoreAuthorityAndOverride_Allows(
        int? minScore, int? liveScore, bool? authorityActive, bool overrideCheck)
    {
        _tenant.Settings.MinBrokerCreditScore = minScore;
        SetupCredit(liveScore, authorityActive);

        var result = await _sut.CheckAsync(Listing(), overrideCheck, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(70, 50, true)]
    [InlineData(null, 90, false)]
    public async Task CheckAsync_ScoreOrAuthorityFailsWithoutOverride_Refuses(
        int? minScore, int? liveScore, bool? authorityActive)
    {
        _tenant.Settings.MinBrokerCreditScore = minScore;
        SetupCredit(liveScore, authorityActive);

        var result = await _sut.CheckAsync(Listing(), overrideCheck: false, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.BrokerCreditBelowThreshold, result.ErrorCode);
    }

    /// <summary>Stamped even when the check blocks, so the listing shows why.</summary>
    [Theory]
    [InlineData(90, true)]
    [InlineData(50, true)]
    [InlineData(null, false)]
    public async Task CheckAsync_CreditData_StampsTheListing(int? liveScore, bool? authorityActive)
    {
        _tenant.Settings.MinBrokerCreditScore = 70;
        SetupCredit(liveScore, authorityActive);
        var listing = Listing();

        await _sut.CheckAsync(listing, overrideCheck: false, CancellationToken.None);

        Assert.Equal(liveScore, listing.BrokerCreditScore);
        Assert.Equal(30, listing.BrokerDaysToPay);
        Assert.NotNull(listing.BrokerCreditCheckedAt);
        await _tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>With no data from the credit service, the listing's stored score is the one checked.</summary>
    [Fact]
    public async Task CheckAsync_NoCreditDataAndNoStoredScore_Allows()
    {
        _tenant.Settings.MinBrokerCreditScore = 70;

        var result = await _sut.CheckAsync(Listing(), overrideCheck: false, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CheckAsync_NoCreditDataAndStoredScoreBelowMinimum_Refuses()
    {
        _tenant.Settings.MinBrokerCreditScore = 70;
        var listing = Listing();
        listing.BrokerCreditScore = 50;

        var result = await _sut.CheckAsync(listing, overrideCheck: false, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.BrokerCreditBelowThreshold, result.ErrorCode);
    }

    private void SetupCredit(int? liveScore, bool? authorityActive) =>
        _brokerCreditService.GetBrokerCreditAsync("MC123456", Arg.Any<CancellationToken>())
            .Returns(new BrokerCreditDto
            {
                McNumber = "123456",
                CreditScore = liveScore,
                DaysToPay = 30,
                AuthorityActive = authorityActive,
                Source = BrokerCreditSource.Demo,
                CheckedAt = DateTime.UtcNow
            });

    private static LoadBoardListing Listing() => new()
    {
        ExternalListingId = "EXT-1",
        ProviderType = LoadBoardProviderType.Demo,
        OriginAddress = TestData.Address(),
        OriginLocation = new GeoPoint(-96.8, 32.8),
        DestinationAddress = TestData.Address(),
        DestinationLocation = new GeoPoint(-87.6, 41.9),
        BrokerName = "Test Broker",
        BrokerMcNumber = "MC123456",
        ExpiresAt = DateTime.UtcNow.AddDays(1)
    };
}
