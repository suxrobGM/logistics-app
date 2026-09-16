using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.LoadBoard.Credit;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.LoadBoard;

public class BrokerCreditServiceTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly FmcsaClient _fmcsaClient;

    private readonly ITenantRepository<BrokerCreditRecord, Guid> _recordRepo =
        Substitute.For<ITenantRepository<BrokerCreditRecord, Guid>>();
    private readonly ITenantRepository<LoadBoardConfiguration, Guid> _configRepo =
        Substitute.For<ITenantRepository<LoadBoardConfiguration, Guid>>();

    private readonly BrokerCreditService _sut;

    public BrokerCreditServiceTests()
    {
        _fmcsaClient = Substitute.For<FmcsaClient>(
            new HttpClient(),
            Options.Create(new FmcsaOptions()),
            NullLogger<FmcsaClient>.Instance);

        _tenantUow.Repository<BrokerCreditRecord>().Returns(_recordRepo);
        _tenantUow.Repository<LoadBoardConfiguration>().Returns(_configRepo);

        _sut = new BrokerCreditService(_tenantUow, _fmcsaClient, NullLogger<BrokerCreditService>.Instance);
    }

    private void SetupDemoConfig(bool present)
    {
        _configRepo.GetAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<LoadBoardConfiguration, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns(present
                ? new LoadBoardConfiguration { ProviderType = LoadBoardProviderType.Demo, ApiKey = "demo" }
                : null);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("MC-")]
    public async Task GetBrokerCredit_MissingOrInvalidMc_ReturnsNull(string? mcNumber)
    {
        var result = await _sut.GetBrokerCreditAsync(mcNumber);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetBrokerCredit_FreshCachedRecord_ReturnsCacheWithoutLookup()
    {
        _recordRepo.GetAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<BrokerCreditRecord, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns(new BrokerCreditRecord
            {
                McNumber = "123456",
                CreditScore = 77,
                Source = BrokerCreditSource.Dat,
                CheckedAt = DateTime.UtcNow.AddHours(-1)
            });

        var result = await _sut.GetBrokerCreditAsync("MC123456");

        Assert.NotNull(result);
        Assert.Equal(77, result.CreditScore);
        Assert.Equal(BrokerCreditSource.Dat, result.Source);
        await _fmcsaClient.DidNotReceiveWithAnyArgs().GetAuthorityActiveAsync(default!);
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetBrokerCredit_DemoProvider_ReturnsDeterministicScoreAndPersists()
    {
        SetupDemoConfig(present: true);
        _recordRepo.GetAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<BrokerCreditRecord, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns((BrokerCreditRecord?)null);

        var first = await _sut.GetBrokerCreditAsync("MC123456");
        var second = await _sut.GetBrokerCreditAsync("123456");

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal(first.CreditScore, second.CreditScore);
        Assert.Equal(first.DaysToPay, second.DaysToPay);
        Assert.InRange(first.CreditScore!.Value, 30, 100);
        Assert.Equal(BrokerCreditSource.Demo, first.Source);
        await _recordRepo.Received().AddAsync(Arg.Any<BrokerCreditRecord>(), Arg.Any<CancellationToken>());
        await _tenantUow.Received().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetBrokerCredit_NoDemoProvider_FallsBackToFmcsa()
    {
        SetupDemoConfig(present: false);
        _recordRepo.GetAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<BrokerCreditRecord, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns((BrokerCreditRecord?)null);
        _fmcsaClient.GetAuthorityActiveAsync("123456", Arg.Any<CancellationToken>()).Returns(false);

        var result = await _sut.GetBrokerCreditAsync("MC123456");

        Assert.NotNull(result);
        Assert.False(result.AuthorityActive);
        Assert.Null(result.CreditScore);
        Assert.Equal(BrokerCreditSource.Fmcsa, result.Source);
    }

    [Fact]
    public async Task GetBrokerCredit_NoSourceAvailable_ServesStaleRecord()
    {
        SetupDemoConfig(present: false);
        _recordRepo.GetAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<BrokerCreditRecord, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns(new BrokerCreditRecord
            {
                McNumber = "123456",
                CreditScore = 61,
                Source = BrokerCreditSource.Truckstop,
                CheckedAt = DateTime.UtcNow.AddDays(-3)
            });
        _fmcsaClient.GetAuthorityActiveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((bool?)null);

        var result = await _sut.GetBrokerCreditAsync("MC123456");

        Assert.NotNull(result);
        Assert.Equal(61, result.CreditScore);
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
