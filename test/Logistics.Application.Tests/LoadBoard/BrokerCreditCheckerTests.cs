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

namespace Logistics.Application.Tests.LoadBoard;

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

    /// <summary>
    /// A null <paramref name="liveScore"/> with a null <paramref name="authorityActive"/> means the
    /// credit service has no data; the listing's stored score is then the one checked.
    /// </summary>
    [Theory]
    [InlineData(null, 10, true, null, false, true)]
    [InlineData(70, 50, true, null, false, false)]
    [InlineData(70, 70, true, null, false, true)]
    [InlineData(70, 50, true, null, true, true)]
    [InlineData(null, 90, false, null, false, false)]
    [InlineData(null, 90, false, null, true, true)]
    [InlineData(70, null, null, null, false, true)]
    [InlineData(70, null, null, 50, false, false)]
    public async Task CheckAsync_ScoreAuthorityAndOverride_DecideTheVerdict(
        int? minScore, int? liveScore, bool? authorityActive, int? storedScore, bool overrideCheck, bool allowed)
    {
        _tenant.Settings.MinBrokerCreditScore = minScore;
        var address = new Address { Line1 = "1 St", City = "Dallas", State = "TX", ZipCode = "75001", Country = "US" };
        var listing = new LoadBoardListing
        {
            ExternalListingId = "EXT-1",
            ProviderType = LoadBoardProviderType.Demo,
            OriginAddress = address,
            OriginLocation = new GeoPoint(-96.8, 32.8),
            DestinationAddress = address,
            DestinationLocation = new GeoPoint(-87.6, 41.9),
            BrokerName = "Test Broker",
            BrokerMcNumber = "MC123456",
            BrokerCreditScore = storedScore,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };
        var hasCreditData = liveScore is not null || authorityActive is not null;
        if (hasCreditData)
        {
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
        }

        var result = await _sut.CheckAsync(listing, overrideCheck, CancellationToken.None);

        Assert.Equal(allowed, result.IsSuccess);
        if (!allowed)
        {
            Assert.Equal(ErrorCodes.BrokerCreditBelowThreshold, result.ErrorCode);
        }

        if (hasCreditData)
        {
            // Stamped even when the check blocks, so the listing shows why.
            Assert.Equal(liveScore ?? storedScore, listing.BrokerCreditScore);
            Assert.Equal(30, listing.BrokerDaysToPay);
            Assert.NotNull(listing.BrokerCreditCheckedAt);
            await _tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
