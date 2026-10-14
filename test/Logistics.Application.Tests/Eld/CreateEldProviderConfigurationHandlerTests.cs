using System.Linq.Expressions;
using Logistics.Application.Abstractions.Eld;
using Logistics.Application.Modules.Compliance.Eld.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Eld;

public class CreateEldProviderConfigurationHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IEldProviderFactory _factory = Substitute.For<IEldProviderFactory>();
    private readonly IEldProviderService _provider = Substitute.For<IEldProviderService>();
    private readonly ITenantRepository<EldProviderConfiguration, Guid> _configs =
        Substitute.For<ITenantRepository<EldProviderConfiguration, Guid>>();
    private readonly CreateEldProviderConfigurationHandler _sut;

    public CreateEldProviderConfigurationHandlerTests()
    {
        _tenantUow.Repository<EldProviderConfiguration>().Returns(_configs);
        _configs.GetAsync(Arg.Any<Expression<Func<EldProviderConfiguration, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((EldProviderConfiguration?)null);
        _factory.IsProviderSupported(Arg.Any<EldProviderType>()).Returns(true);
        _factory.GetProvider(Arg.Any<EldProviderConfiguration>()).Returns(_provider);
        _provider.ValidateCredentialsAsync(Arg.Any<string>(), Arg.Any<string?>()).Returns(true);

        _sut = new CreateEldProviderConfigurationHandler(
            _tenantUow, _factory, NullLogger<CreateEldProviderConfigurationHandler>.Instance);
    }

    [Fact]
    public async Task Handle_TtEldWithoutUsdot_Fails()
    {
        var result = await _sut.Handle(new CreateEldProviderConfigurationCommand
        {
            ProviderType = EldProviderType.TtEld,
            ApiKey = "key",
            ApiSecret = "token"
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await _configs.DidNotReceive().AddAsync(Arg.Any<EldProviderConfiguration>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SamsaraEu_InitializesProviderWithRegionBeforeValidating()
    {
        var result = await _sut.Handle(new CreateEldProviderConfigurationCommand
        {
            ProviderType = EldProviderType.Samsara,
            ApiKey = "samsara_api_token",
            Region = "EU",
            WebhookSecret = "rGoy+beNph0qGBLj6Aqoydj6SGA="
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Received.InOrder(() =>
        {
            _factory.GetProvider(Arg.Is<EldProviderConfiguration>(c => c.Region == "eu"));
            _provider.ValidateCredentialsAsync("samsara_api_token", null);
        });
        await _configs.Received(1).AddAsync(
            Arg.Is<EldProviderConfiguration>(c => c.Region == "eu" && c.WebhookSecret == "rGoy+beNph0qGBLj6Aqoydj6SGA="),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownSamsaraRegion_Fails()
    {
        var result = await _sut.Handle(new CreateEldProviderConfigurationCommand
        {
            ProviderType = EldProviderType.Samsara,
            ApiKey = "samsara_api_token",
            Region = "mars"
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
