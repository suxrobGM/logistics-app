using Logistics.Application.Abstractions.LoadBoard;
using Logistics.Application.Modules.Integrations.LoadBoard.Commands;
using Logistics.Application.Modules.Integrations.LoadBoard.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.LoadBoard;

public class SearchLoadBoardHandlerTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ILoadBoardTokenService tokenService = Substitute.For<ILoadBoardTokenService>();
    private readonly ITenantRepository<LoadBoardConfiguration, Guid> configRepo =
        Substitute.For<ITenantRepository<LoadBoardConfiguration, Guid>>();
    private readonly ITenantRepository<LoadBoardListing, Guid> listingRepo =
        Substitute.For<ITenantRepository<LoadBoardListing, Guid>>();
    private readonly SearchLoadBoardHandler sut;

    private readonly LoadBoardConfiguration demoConfig =
        new() { ProviderType = LoadBoardProviderType.Demo, ApiKey = "demo" };
    private readonly LoadBoardConfiguration datConfig =
        new() { ProviderType = LoadBoardProviderType.Dat, ApiKey = "dat" };

    public SearchLoadBoardHandlerTests()
    {
        tenantUow.Repository<LoadBoardConfiguration>().Returns(configRepo);
        tenantUow.Repository<LoadBoardListing>().Returns(listingRepo);
        configRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<LoadBoardConfiguration, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns([demoConfig, datConfig]);
        listingRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<LoadBoardListing, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        sut = new SearchLoadBoardHandler(tenantUow, tokenService, NullLogger<SearchLoadBoardHandler>.Instance);
    }

    private static LoadBoardListingDto CreateListing(string externalId) => new()
    {
        ExternalListingId = externalId,
        ProviderType = LoadBoardProviderType.Demo,
        OriginAddress = new Address { Line1 = "1 St", City = "Dallas", State = "TX", ZipCode = "75001", Country = "US" },
        OriginLocation = new GeoPoint(-96.8, 32.8),
        DestinationAddress = new Address { Line1 = "2 St", City = "Chicago", State = "IL", ZipCode = "60601", Country = "US" },
        DestinationLocation = new GeoPoint(-87.6, 41.9),
        RatePerMile = 2.5m,
        ExpiresAt = DateTime.UtcNow.AddDays(1)
    };

    [Fact]
    public async Task Handle_OneProviderFailsAuth_OtherProviderStillReturnsListings()
    {
        var healthyProvider = Substitute.For<ILoadBoardProviderService>();
        healthyProvider.SearchLoadsAsync(Arg.Any<LoadBoardSearchCriteria>())
            .Returns([CreateListing("EXT-1"), CreateListing("EXT-2")]);
        tokenService.GetReadyProviderAsync(demoConfig, Arg.Any<CancellationToken>())
            .Returns(Result<ILoadBoardProviderService>.Ok(healthyProvider));
        tokenService.GetReadyProviderAsync(datConfig, Arg.Any<CancellationToken>())
            .Returns(Result<ILoadBoardProviderService>.Fail("Could not authenticate with Dat"));

        var result = await sut.Handle(new SearchLoadBoardCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.TotalCount);
        Assert.Equal(2, result.Value.CountByProvider[LoadBoardProviderType.Demo]);
        Assert.Equal(0, result.Value.CountByProvider[LoadBoardProviderType.Dat]);
        Assert.NotNull(result.Value.Errors);
        Assert.Contains("authenticate", result.Value.Errors[LoadBoardProviderType.Dat]);
    }

    [Fact]
    public async Task Handle_AllProvidersHealthy_NoErrors()
    {
        var provider = Substitute.For<ILoadBoardProviderService>();
        provider.SearchLoadsAsync(Arg.Any<LoadBoardSearchCriteria>()).Returns([CreateListing("EXT-1")]);
        tokenService.GetReadyProviderAsync(Arg.Any<LoadBoardConfiguration>(), Arg.Any<CancellationToken>())
            .Returns(Result<ILoadBoardProviderService>.Ok(provider));

        var result = await sut.Handle(new SearchLoadBoardCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.Errors);
    }

    [Fact]
    public async Task Handle_NoConfiguredProviders_Fails()
    {
        configRepo.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<LoadBoardConfiguration, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await sut.Handle(new SearchLoadBoardCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
