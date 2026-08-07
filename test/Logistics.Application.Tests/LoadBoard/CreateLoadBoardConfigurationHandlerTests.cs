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
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ILoadBoardProviderFactory providerFactory = Substitute.For<ILoadBoardProviderFactory>();
    private readonly ILoadBoardProviderService provider = Substitute.For<ILoadBoardProviderService>();
    private readonly ITenantRepository<LoadBoardConfiguration, Guid> configRepo =
        Substitute.For<ITenantRepository<LoadBoardConfiguration, Guid>>();
    private readonly CreateLoadBoardConfigurationCommand command;
    private readonly CreateLoadBoardConfigurationHandler sut;

    public CreateLoadBoardConfigurationHandlerTests()
    {
        command = new CreateLoadBoardConfigurationCommand
        {
            ProviderType = LoadBoardProviderType.Truckstop,
            ApiKey = "key",
            ApiSecret = "secret"
        };

        tenantUow.Repository<LoadBoardConfiguration>().Returns(configRepo);
        configRepo.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<LoadBoardConfiguration, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns((LoadBoardConfiguration?)null);

        providerFactory.IsProviderSupported(command.ProviderType).Returns(true);
        providerFactory.GetProvider(command.ProviderType).Returns(provider);

        sut = new CreateLoadBoardConfigurationHandler(
            tenantUow, providerFactory, NullLogger<CreateLoadBoardConfigurationHandler>.Instance);
    }

    [Fact]
    public async Task Handle_OAuthProvider_StoresAcquiredTokensOnConfiguration()
    {
        provider.RequiresOAuthToken.Returns(true);
        var expiresAt = DateTime.UtcNow.AddMinutes(20);
        provider.AcquireTokenAsync("key", "secret").Returns(new OAuthTokenResultDto
        {
            AccessToken = "access-1", RefreshToken = "refresh-1", ExpiresAt = expiresAt
        });
        LoadBoardConfiguration? added = null;
        await configRepo.AddAsync(Arg.Do<LoadBoardConfiguration>(c => added = c), Arg.Any<CancellationToken>());

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(added);
        Assert.Equal("access-1", added.AccessToken);
        Assert.Equal("refresh-1", added.RefreshToken);
        Assert.Equal(expiresAt, added.TokenExpiresAt);
        await tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OAuthAcquisitionFails_FailsWithoutAdding()
    {
        provider.RequiresOAuthToken.Returns(true);
        provider.AcquireTokenAsync("key", "secret").Returns((OAuthTokenResultDto?)null);

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await configRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_KeyBasedProvider_ValidatesWithoutTokenAcquisition()
    {
        provider.RequiresOAuthToken.Returns(false);
        provider.ValidateCredentialsAsync("key", "secret").Returns(true);

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await provider.DidNotReceiveWithAnyArgs().AcquireTokenAsync(default!, default);
        await configRepo.Received(1).AddAsync(
            Arg.Is<LoadBoardConfiguration>(c => c.AccessToken == null), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_KeyBasedProviderInvalidCredentials_Fails()
    {
        provider.RequiresOAuthToken.Returns(false);
        provider.ValidateCredentialsAsync("key", "secret").Returns(false);

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await configRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Handle_DuplicateConfiguration_Fails()
    {
        configRepo.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<LoadBoardConfiguration, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns(new LoadBoardConfiguration { ProviderType = command.ProviderType, ApiKey = "existing" });

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await configRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }
}
