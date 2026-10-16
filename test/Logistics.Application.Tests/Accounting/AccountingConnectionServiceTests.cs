using System.Linq.Expressions;
using Logistics.Application.Abstractions.Accounting;
using Logistics.Application.Modules.Integrations.Accounting.Services;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Logistics.Application.Tests.Accounting;

public class AccountingConnectionServiceTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IAccountingProviderFactory _factory = Substitute.For<IAccountingProviderFactory>();
    private readonly IAccountingProviderService _provider = Substitute.For<IAccountingProviderService>();
    private readonly AccountingConnectionService _sut;

    private readonly AccountingProviderConfiguration _config = new()
    {
        ProviderType = AccountingProviderType.QuickBooksOnline,
        RealmId = "realm",
        AccessToken = "old-access",
        RefreshToken = "old-refresh",
        TokenExpiresAt = DateTime.UtcNow.AddMinutes(1),
        RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(3)
    };

    public AccountingConnectionServiceTests()
    {
        _tenantUow.GetCurrentTenant().Returns(TestTenant.Create());
        _factory.GetProvider(AccountingProviderType.QuickBooksOnline).Returns(_provider);
        _factory.GetProvider(_config).Returns(_provider);
        _sut = new AccountingConnectionService(_tenantUow, _factory, NullLogger<AccountingConnectionService>.Instance);
    }

    [Fact]
    public async Task GetProviderAsync_ExpiringToken_StoresRotatedTokensAndRefreshExpiry()
    {
        var refreshExpiry = DateTime.UtcNow.AddDays(100);
        _provider.RefreshTokenAsync("old-refresh", Arg.Any<CancellationToken>()).Returns(new OAuthTokenResultDto
        {
            AccessToken = "new-access",
            RefreshToken = "new-refresh",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            RefreshTokenExpiresAt = refreshExpiry
        });

        var provider = await _sut.GetProviderAsync(_config, CancellationToken.None);

        Assert.Same(_provider, provider);
        Assert.Equal("new-access", _config.AccessToken);
        Assert.Equal("new-refresh", _config.RefreshToken);
        Assert.Equal(refreshExpiry, _config.RefreshTokenExpiresAt);
        await _tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetProviderAsync_RefreshTokenRejected_FlagsReconnectAndReturnsNull()
    {
        _provider.RefreshTokenAsync("old-refresh", Arg.Any<CancellationToken>())
            .ThrowsAsync(new AccountingReconnectRequiredException("invalid_grant"));

        var provider = await _sut.GetProviderAsync(_config, CancellationToken.None);

        Assert.Null(provider);
        Assert.True(_config.NeedsReconnect);
        Assert.Null(_config.AccessToken);
        await _tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetProviderAsync_NeedsReconnect_DoesNotCallTheProvider()
    {
        _config.NeedsReconnect = true;

        var provider = await _sut.GetProviderAsync(_config, CancellationToken.None);

        Assert.Null(provider);
        await _provider.DidNotReceive().RefreshTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetProviderAsync_TransientRefreshError_PropagatesWithoutFlaggingReconnect()
    {
        _provider.RefreshTokenAsync("old-refresh", Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("timeout"));

        await Assert.ThrowsAsync<HttpRequestException>(() => _sut.GetProviderAsync(_config, CancellationToken.None));

        Assert.False(_config.NeedsReconnect);
    }

    [Fact]
    public async Task GetProviderAsync_ValidToken_SkipsRefresh()
    {
        _config.TokenExpiresAt = DateTime.UtcNow.AddMinutes(50);

        var provider = await _sut.GetProviderAsync(_config, CancellationToken.None);

        Assert.Same(_provider, provider);
        await _provider.DidNotReceive().RefreshTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_NotConnected_FailsWithoutCallingTheProvider()
    {
        StoreConfig(null);

        var result = await _sut.ExecuteAsync((_, _) => Task.FromResult(Result.Ok()), CancellationToken.None);

        Assert.Equal("QuickBooks is not connected.", result.Error);
        _factory.DidNotReceive().GetProvider(Arg.Any<AccountingProviderConfiguration>());
    }

    [Fact]
    public async Task ExecuteAsync_AccessTokenRejected_FailsAndForgetsTheToken()
    {
        StoreConfig(_config);
        _config.TokenExpiresAt = DateTime.UtcNow.AddMinutes(50);
        _provider.GetAccountsAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new AccountingReconnectRequiredException("401"));

        var result = await _sut.ExecuteAsync((p, _) => p.GetAccountsAsync(CancellationToken.None), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Null(_config.TokenExpiresAt);
        await _tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private void StoreConfig(AccountingProviderConfiguration? config)
    {
        var configs = Substitute.For<ITenantRepository<AccountingProviderConfiguration, Guid>>();
        configs.GetAsync(Arg.Any<Expression<Func<AccountingProviderConfiguration, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(config);
        _tenantUow.Repository<AccountingProviderConfiguration>().Returns(configs);
    }
}
