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
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ILoadBoardProviderFactory providerFactory = Substitute.For<ILoadBoardProviderFactory>();
    private readonly ILoadBoardProviderService provider = Substitute.For<ILoadBoardProviderService>();
    private readonly LoadBoardConfiguration config;
    private readonly LoadBoardTokenService sut;

    public LoadBoardTokenServiceTests()
    {
        config = new LoadBoardConfiguration
        {
            ProviderType = LoadBoardProviderType.Truckstop,
            ApiKey = "key",
            ApiSecret = "secret"
        };

        providerFactory.GetProvider(config.ProviderType).Returns(provider);
        provider.RequiresOAuthToken.Returns(true);

        sut = new LoadBoardTokenService(tenantUow, providerFactory, NullLogger<LoadBoardTokenService>.Instance);
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
        provider.RequiresOAuthToken.Returns(false);

        var result = await sut.GetReadyProviderAsync(config);

        Assert.True(result.IsSuccess);
        Assert.Same(provider, result.Value);
        provider.Received(1).Initialize(config);
        await provider.DidNotReceiveWithAnyArgs().AcquireTokenAsync(default!, default);
        await provider.DidNotReceiveWithAnyArgs().RefreshTokenAsync(default!);
        await tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Token reuse and acquisition

    [Fact]
    public async Task GetReadyProvider_ValidToken_SkipsAcquisition()
    {
        config.AccessToken = "valid";
        config.TokenExpiresAt = DateTime.UtcNow.AddMinutes(15);

        var result = await sut.GetReadyProviderAsync(config);

        Assert.True(result.IsSuccess);
        await provider.DidNotReceiveWithAnyArgs().AcquireTokenAsync(default!, default);
        await provider.DidNotReceiveWithAnyArgs().RefreshTokenAsync(default!);
        await tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        provider.Received(1).Initialize(config);
    }

    [Fact]
    public async Task GetReadyProvider_TokenInsideExpirySkew_Reacquires()
    {
        config.AccessToken = "nearly-expired";
        config.TokenExpiresAt = DateTime.UtcNow.AddSeconds(30);
        provider.AcquireTokenAsync(config.ApiKey, config.ApiSecret).Returns(Token("fresh"));

        var result = await sut.GetReadyProviderAsync(config);

        Assert.True(result.IsSuccess);
        Assert.Equal("fresh", config.AccessToken);
    }

    [Fact]
    public async Task GetReadyProvider_MissingToken_AcquiresAndPersists()
    {
        provider.AcquireTokenAsync(config.ApiKey, config.ApiSecret).Returns(Token("acquired", "refresh-1"));

        var result = await sut.GetReadyProviderAsync(config);

        Assert.True(result.IsSuccess);
        Assert.Equal("acquired", config.AccessToken);
        Assert.Equal("refresh-1", config.RefreshToken);
        Assert.NotNull(config.TokenExpiresAt);
        await tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Received.InOrder(() =>
        {
            tenantUow.SaveChangesAsync(Arg.Any<CancellationToken>());
            provider.Initialize(config);
        });
    }

    #endregion

    #region Refresh flow

    [Fact]
    public async Task GetReadyProvider_ExpiredWithRefreshToken_RefreshesAndPersists()
    {
        config.AccessToken = "expired";
        config.RefreshToken = "refresh-old";
        config.TokenExpiresAt = DateTime.UtcNow.AddMinutes(-5);
        provider.RefreshTokenAsync("refresh-old").Returns(Token("refreshed", "refresh-new"));

        var result = await sut.GetReadyProviderAsync(config);

        Assert.True(result.IsSuccess);
        Assert.Equal("refreshed", config.AccessToken);
        Assert.Equal("refresh-new", config.RefreshToken);
        await provider.DidNotReceiveWithAnyArgs().AcquireTokenAsync(default!, default);
        await tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetReadyProvider_RefreshReturnsNoRefreshToken_KeepsStoredOne()
    {
        config.AccessToken = "expired";
        config.RefreshToken = "refresh-keep";
        config.TokenExpiresAt = DateTime.UtcNow.AddMinutes(-5);
        provider.RefreshTokenAsync("refresh-keep").Returns(Token("refreshed"));

        await sut.GetReadyProviderAsync(config);

        Assert.Equal("refresh-keep", config.RefreshToken);
    }

    [Fact]
    public async Task GetReadyProvider_RefreshFails_FallsBackToAcquisition()
    {
        config.AccessToken = "expired";
        config.RefreshToken = "refresh-dead";
        config.TokenExpiresAt = DateTime.UtcNow.AddMinutes(-5);
        provider.RefreshTokenAsync("refresh-dead").Returns((OAuthTokenResultDto?)null);
        provider.AcquireTokenAsync(config.ApiKey, config.ApiSecret).Returns(Token("reacquired"));

        var result = await sut.GetReadyProviderAsync(config);

        Assert.True(result.IsSuccess);
        Assert.Equal("reacquired", config.AccessToken);
    }

    #endregion

    #region Failure

    [Fact]
    public async Task GetReadyProvider_AcquisitionFails_ReturnsFailWithoutSaveOrInitialize()
    {
        provider.AcquireTokenAsync(config.ApiKey, config.ApiSecret).Returns((OAuthTokenResultDto?)null);

        var result = await sut.GetReadyProviderAsync(config);

        Assert.False(result.IsSuccess);
        Assert.Contains("Truckstop", result.Error);
        await tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        provider.DidNotReceiveWithAnyArgs().Initialize(default!);
    }

    #endregion
}
