using System.Net;
using Logistics.Application.Abstractions.LoadBoard;
using Logistics.Application.Modules.Integrations.LoadBoard.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Logistics.Application.Tests.LoadBoard;

public class LoadBoardProviderGatewayTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ILoadBoardProviderFactory _factory = Substitute.For<ILoadBoardProviderFactory>();
    private readonly ILoadBoardProviderService _provider = Substitute.For<ILoadBoardProviderService>();
    private readonly LoadBoardConfiguration _config;
    private readonly LoadBoardProviderGateway _sut;

    public LoadBoardProviderGatewayTests()
    {
        _config = new LoadBoardConfiguration
        {
            ProviderType = LoadBoardProviderType.Dat,
            ApiKey = "svc@carrier.example",
            ApiSecret = "secret",
            UserEmail = "dispatch@carrier.example"
        };

        _factory.IsProviderSupported(LoadBoardProviderType.Dat).Returns(true);
        _factory.GetProvider(LoadBoardProviderType.Dat).Returns(_provider);
        _provider.RequiresOAuthToken.Returns(true);

        _sut = new LoadBoardProviderGateway(_tenantUow, _factory, NullLogger<LoadBoardProviderGateway>.Instance);
    }

    private static OAuthTokenResultDto Token(string access, string? refresh = null) => new()
    {
        AccessToken = access,
        RefreshToken = refresh,
        ExpiresAt = DateTime.UtcNow.AddMinutes(30)
    };

    private static LoadBoardApiException Unauthorized() => new("DAT rejected the credentials (401)", HttpStatusCode.Unauthorized);

    private Task<Result<int>> CallAsync() => _sut.ExecuteAsync(_config, async p =>
    {
        await p.RemoveTruckPostAsync("P-1");
        return 7;
    });

    #region Token caching

    [Fact]
    public async Task Execute_ValidToken_CallsWithoutTokenWork()
    {
        _config.AccessToken = "cached";
        _config.TokenExpiresAt = DateTime.UtcNow.AddMinutes(15);

        var result = await CallAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(7, result.Value);
        await _provider.DidNotReceiveWithAnyArgs().AcquireTokenAsync(default!);
        await _provider.DidNotReceiveWithAnyArgs().RefreshTokenAsync(default!);
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_NoToken_AcquiresBothTokensAndPersistsThem()
    {
        _provider.AcquireTokenAsync(_config, Arg.Any<CancellationToken>()).Returns(Token("user", "org"));

        var result = await CallAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal("user", _config.AccessToken);
        Assert.Equal("org", _config.RefreshToken);
        await _tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Received.InOrder(() =>
        {
            _tenantUow.SaveChangesAsync(Arg.Any<CancellationToken>());
            _provider.Initialize(_config);
        });
    }

    [Fact]
    public async Task Execute_ExpiredUserTokenWithOrgToken_RefreshesAndKeepsOrgToken()
    {
        _config.AccessToken = "expired";
        _config.RefreshToken = "org";
        _config.TokenExpiresAt = DateTime.UtcNow.AddSeconds(30);
        _provider.RefreshTokenAsync(_config, Arg.Any<CancellationToken>()).Returns(Token("user-2"));

        var result = await CallAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal("user-2", _config.AccessToken);
        Assert.Equal("org", _config.RefreshToken);
        await _provider.DidNotReceiveWithAnyArgs().AcquireTokenAsync(default!);
    }

    [Fact]
    public async Task Execute_OrgTokenRejectedOnRefresh_FallsBackToFullAcquisition()
    {
        _config.RefreshToken = "org-expired";
        _provider.RefreshTokenAsync(_config, Arg.Any<CancellationToken>()).ThrowsAsync(Unauthorized());
        _provider.AcquireTokenAsync(_config, Arg.Any<CancellationToken>()).Returns(Token("user", "org-new"));

        var result = await CallAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal("org-new", _config.RefreshToken);
    }

    #endregion

    #region 401 retry

    [Fact]
    public async Task Execute_CallReturns401_ReauthenticatesOnceAndRetries()
    {
        _config.AccessToken = "revoked";
        _config.RefreshToken = "org";
        _config.TokenExpiresAt = DateTime.UtcNow.AddMinutes(20);
        _provider.RefreshTokenAsync(_config, Arg.Any<CancellationToken>()).Returns(Token("user-2"));
        _provider.RemoveTruckPostAsync("P-1", Arg.Any<CancellationToken>())
            .Returns(_ => throw Unauthorized(), _ => Task.CompletedTask);

        var result = await CallAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal("user-2", _config.AccessToken);
        await _provider.Received(2).RemoveTruckPostAsync("P-1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_401AfterReauthentication_FailsWithoutLooping()
    {
        _config.AccessToken = "revoked";
        _config.TokenExpiresAt = DateTime.UtcNow.AddMinutes(20);
        _provider.AcquireTokenAsync(_config, Arg.Any<CancellationToken>()).Returns(Token("user-2", "org"));
        _provider.RemoveTruckPostAsync("P-1", Arg.Any<CancellationToken>()).ThrowsAsync(Unauthorized());

        var result = await CallAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal("DAT: DAT rejected the credentials (401)", result.Error);
        await _provider.Received(2).RemoveTruckPostAsync("P-1", Arg.Any<CancellationToken>());
    }

    #endregion

    #region Failures

    [Fact]
    public async Task Execute_VendorError_ReturnsFailWithProviderPrefixedMessage()
    {
        _config.AccessToken = "cached";
        _config.TokenExpiresAt = DateTime.UtcNow.AddMinutes(15);
        _provider.RemoveTruckPostAsync("P-1", Arg.Any<CancellationToken>())
            .ThrowsAsync(new LoadBoardApiException("DAT returned 503 ServiceUnavailable", HttpStatusCode.ServiceUnavailable));

        var result = await CallAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal("DAT: DAT returned 503 ServiceUnavailable", result.Error);
    }

    [Fact]
    public async Task Execute_AcquisitionFails_ReturnsFailWithoutSaveOrCall()
    {
        _provider.AcquireTokenAsync(_config, Arg.Any<CancellationToken>()).ThrowsAsync(Unauthorized());

        var result = await CallAsync();

        Assert.False(result.IsSuccess);
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _provider.DidNotReceiveWithAnyArgs().RemoveTruckPostAsync(default!);
    }

    [Fact]
    public async Task Execute_UnsupportedProvider_SaysNotAvailable()
    {
        _config.ProviderType = LoadBoardProviderType.Truckstop;

        var result = await CallAsync();

        Assert.False(result.IsSuccess);
        Assert.Contains("not available", result.Error);
    }

    #endregion

    #region Connection test

    [Fact]
    public async Task TestConnection_TokenProvider_AcquiresFreshTokenEvenWhenOneIsCached()
    {
        _config.AccessToken = "cached";
        _config.TokenExpiresAt = DateTime.UtcNow.AddMinutes(15);
        _provider.AcquireTokenAsync(_config, Arg.Any<CancellationToken>()).Returns(Token("fresh", "org"));

        var result = await _sut.TestConnectionAsync(_config);

        Assert.True(result.IsSuccess);
        Assert.Equal("fresh", _config.AccessToken);
    }

    [Fact]
    public async Task TestConnection_ApiKeyProviderRejectsKey_Fails()
    {
        _provider.RequiresOAuthToken.Returns(false);
        _provider.ValidateCredentialsAsync(_config, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _sut.TestConnectionAsync(_config);

        Assert.False(result.IsSuccess);
    }

    #endregion
}
