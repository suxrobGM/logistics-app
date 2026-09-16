using Logistics.Application.Abstractions.LoadBoard;
using Logistics.Application.Modules.Integrations.LoadBoard.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.LoadBoard;

public class CreateLoadBoardConfigurationHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ILoadBoardProviderFactory _providerFactory = Substitute.For<ILoadBoardProviderFactory>();
    private readonly ILoadBoardProviderService _provider = Substitute.For<ILoadBoardProviderService>();
    private readonly ITenantRepository<LoadBoardConfiguration, Guid> _configRepo =
        Substitute.For<ITenantRepository<LoadBoardConfiguration, Guid>>();
    private readonly CreateLoadBoardConfigurationCommand _command;
    private readonly CreateLoadBoardConfigurationHandler _sut;

    public CreateLoadBoardConfigurationHandlerTests()
    {
        _command = new CreateLoadBoardConfigurationCommand
        {
            ProviderType = LoadBoardProviderType.Truckstop,
            ApiKey = "key",
            ApiSecret = "secret"
        };

        _tenantUow.Repository<LoadBoardConfiguration>().Returns(_configRepo);
        _configRepo.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<LoadBoardConfiguration, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns((LoadBoardConfiguration?)null);

        _providerFactory.IsProviderSupported(_command.ProviderType).Returns(true);
        _providerFactory.GetProvider(_command.ProviderType).Returns(_provider);

        _sut = new CreateLoadBoardConfigurationHandler(
            _tenantUow, _providerFactory, NullLogger<CreateLoadBoardConfigurationHandler>.Instance);
    }

    [Fact]
    public async Task Handle_OAuthProvider_StoresAcquiredTokensOnConfiguration()
    {
        _provider.RequiresOAuthToken.Returns(true);
        var expiresAt = DateTime.UtcNow.AddMinutes(20);
        _provider.AcquireTokenAsync("key", "secret").Returns(new OAuthTokenResultDto
        {
            AccessToken = "access-1",
            RefreshToken = "refresh-1",
            ExpiresAt = expiresAt
        });
        LoadBoardConfiguration? added = null;
        await _configRepo.AddAsync(Arg.Do<LoadBoardConfiguration>(c => added = c), Arg.Any<CancellationToken>());

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(added);
        Assert.Equal("access-1", added.AccessToken);
        Assert.Equal("refresh-1", added.RefreshToken);
        Assert.Equal(expiresAt, added.TokenExpiresAt);
        await _tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OAuthAcquisitionFails_FailsWithoutAdding()
    {
        _provider.RequiresOAuthToken.Returns(true);
        _provider.AcquireTokenAsync("key", "secret").Returns((OAuthTokenResultDto?)null);

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await _configRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_KeyBasedProvider_ValidatesWithoutTokenAcquisition()
    {
        _provider.RequiresOAuthToken.Returns(false);
        _provider.ValidateCredentialsAsync("key", "secret").Returns(true);

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _provider.DidNotReceiveWithAnyArgs().AcquireTokenAsync(default!, default);
        await _configRepo.Received(1).AddAsync(
            Arg.Is<LoadBoardConfiguration>(c => c.AccessToken == null), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_KeyBasedProviderInvalidCredentials_Fails()
    {
        _provider.RequiresOAuthToken.Returns(false);
        _provider.ValidateCredentialsAsync("key", "secret").Returns(false);

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await _configRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Handle_DuplicateConfiguration_Fails()
    {
        _configRepo.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<LoadBoardConfiguration, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns(new LoadBoardConfiguration { ProviderType = _command.ProviderType, ApiKey = "existing" });

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await _configRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }
}
