using Logistics.Application.Abstractions.LoadBoard;
using Logistics.Application.Modules.Integrations.LoadBoard.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.LoadBoard;

public class LoadBoardTokenServiceTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ILoadBoardProviderFactory _providerFactory = Substitute.For<ILoadBoardProviderFactory>();
    private readonly ILoadBoardProviderService _provider = Substitute.For<ILoadBoardProviderService>();
    private readonly LoadBoardConfiguration _config;
    private readonly LoadBoardTokenService _sut;

    public LoadBoardTokenServiceTests()
    {
        _config = new LoadBoardConfiguration
        {
            ProviderType = LoadBoardProviderType.Truckstop,
            ApiKey = "key",
            ApiSecret = "secret"
        };

        _providerFactory.GetProvider(_config.ProviderType).Returns(_provider);
        _provider.RequiresOAuthToken.Returns(true);

        _sut = new LoadBoardTokenService(_tenantUow, _providerFactory, NullLogger<LoadBoardTokenService>.Instance);
    }

    private static OAuthTokenResultDto Token(string access, string? refresh = null) => new()
    {
        AccessToken = access,
        RefreshToken = refresh,
        ExpiresAt = DateTime.UtcNow.AddMinutes(20)
    };

    #region Non-OAuth providers

    [Fact]
    public async Task GetReadyProvider_NonOAuthProvider_InitializesWithoutTokenWork()
    {
        _provider.RequiresOAuthToken.Returns(false);

        var result = await _sut.GetReadyProviderAsync(_config);

        Assert.True(result.IsSuccess);
        Assert.Same(_provider, result.Value);
        _provider.Received(1).Initialize(_config);
        await _provider.DidNotReceiveWithAnyArgs().AcquireTokenAsync(default!, default);
        await _provider.DidNotReceiveWithAnyArgs().RefreshTokenAsync(default!);
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Token reuse and acquisition

    [Fact]
    public async Task GetReadyProvider_ValidToken_SkipsAcquisition()
    {
        _config.AccessToken = "valid";
        _config.TokenExpiresAt = DateTime.UtcNow.AddMinutes(15);

        var result = await _sut.GetReadyProviderAsync(_config);

        Assert.True(result.IsSuccess);
        await _provider.DidNotReceiveWithAnyArgs().AcquireTokenAsync(default!, default);
        await _provider.DidNotReceiveWithAnyArgs().RefreshTokenAsync(default!);
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _provider.Received(1).Initialize(_config);
    }

    [Fact]
    public async Task GetReadyProvider_TokenInsideExpirySkew_Reacquires()
    {
        _config.AccessToken = "nearly-expired";
        _config.TokenExpiresAt = DateTime.UtcNow.AddSeconds(30);
        _provider.AcquireTokenAsync(_config.ApiKey, _config.ApiSecret).Returns(Token("fresh"));

        var result = await _sut.GetReadyProviderAsync(_config);

        Assert.True(result.IsSuccess);
        Assert.Equal("fresh", _config.AccessToken);
    }

    [Fact]
    public async Task GetReadyProvider_MissingToken_AcquiresAndPersists()
    {
        _provider.AcquireTokenAsync(_config.ApiKey, _config.ApiSecret).Returns(Token("acquired", "refresh-1"));

        var result = await _sut.GetReadyProviderAsync(_config);

        Assert.True(result.IsSuccess);
        Assert.Equal("acquired", _config.AccessToken);
        Assert.Equal("refresh-1", _config.RefreshToken);
        Assert.NotNull(_config.TokenExpiresAt);
        await _tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Received.InOrder(() =>
        {
            _tenantUow.SaveChangesAsync(Arg.Any<CancellationToken>());
            _provider.Initialize(_config);
        });
    }

    #endregion

    #region Refresh flow

    [Fact]
    public async Task GetReadyProvider_ExpiredWithRefreshToken_RefreshesAndPersists()
    {
        _config.AccessToken = "expired";
        _config.RefreshToken = "refresh-old";
        _config.TokenExpiresAt = DateTime.UtcNow.AddMinutes(-5);
        _provider.RefreshTokenAsync("refresh-old").Returns(Token("refreshed", "refresh-new"));

        var result = await _sut.GetReadyProviderAsync(_config);

        Assert.True(result.IsSuccess);
        Assert.Equal("refreshed", _config.AccessToken);
        Assert.Equal("refresh-new", _config.RefreshToken);
        await _provider.DidNotReceiveWithAnyArgs().AcquireTokenAsync(default!, default);
        await _tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetReadyProvider_RefreshReturnsNoRefreshToken_KeepsStoredOne()
    {
        _config.AccessToken = "expired";
        _config.RefreshToken = "refresh-keep";
        _config.TokenExpiresAt = DateTime.UtcNow.AddMinutes(-5);
        _provider.RefreshTokenAsync("refresh-keep").Returns(Token("refreshed"));

        await _sut.GetReadyProviderAsync(_config);

        Assert.Equal("refresh-keep", _config.RefreshToken);
    }

    [Fact]
    public async Task GetReadyProvider_RefreshFails_FallsBackToAcquisition()
    {
        _config.AccessToken = "expired";
        _config.RefreshToken = "refresh-dead";
        _config.TokenExpiresAt = DateTime.UtcNow.AddMinutes(-5);
        _provider.RefreshTokenAsync("refresh-dead").Returns((OAuthTokenResultDto?)null);
        _provider.AcquireTokenAsync(_config.ApiKey, _config.ApiSecret).Returns(Token("reacquired"));

        var result = await _sut.GetReadyProviderAsync(_config);

        Assert.True(result.IsSuccess);
        Assert.Equal("reacquired", _config.AccessToken);
    }

    #endregion

    #region Failure

    [Fact]
    public async Task GetReadyProvider_AcquisitionFails_ReturnsFailWithoutSaveOrInitialize()
    {
        _provider.AcquireTokenAsync(_config.ApiKey, _config.ApiSecret).Returns((OAuthTokenResultDto?)null);

        var result = await _sut.GetReadyProviderAsync(_config);

        Assert.False(result.IsSuccess);
        Assert.Contains("Truckstop", result.Error);
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _provider.DidNotReceiveWithAnyArgs().Initialize(default!);
    }

    #endregion
}
