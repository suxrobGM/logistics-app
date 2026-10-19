using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Operations.Loads.Queries;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Identity.Roles;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Loads;

public class GetLoadsHandlerTests
{
    [Fact]
    public async Task Handle_DriverWithoutUserClaim_ReturnsNoLoads()
    {
        var tenantUow = Substitute.For<ITenantUnitOfWork>();
        var loadRepo = Substitute.For<ITenantRepository<Load, Guid>>();
        var currentUser = Substitute.For<ICurrentUserService>();
        tenantUow.Repository<Load>().Returns(loadRepo);
        currentUser.IsInRole(TenantRoles.Driver).Returns(true);

        var sut = new GetLoadsHandler(tenantUow, currentUser);
        var result = await sut.Handle(new GetLoadsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
        Assert.Equal(0, result.TotalItems);
        _ = loadRepo.DidNotReceive().Query();
    }

    [Fact]
    public async Task Handle_SearchInADifferentCase_MatchesTheCustomerName()
    {
        var tenantUow = Substitute.For<ITenantUnitOfWork>();
        var loadRepo = Substitute.For<ITenantRepository<Load, Guid>>();
        tenantUow.Repository<Load>().Returns(loadRepo);
        var load = new Load
        {
            Name = "Load 1",
            Type = LoadType.GeneralFreight,
            OriginAddress = new Address { Line1 = "1 A St", City = "NYC", State = "NY", ZipCode = "10001", Country = "US" },
            OriginLocation = new GeoPoint(-74.0, 40.7),
            DestinationAddress = new Address { Line1 = "2 B St", City = "LA", State = "CA", ZipCode = "90001", Country = "US" },
            DestinationLocation = new GeoPoint(-118.2, 34.0),
            DeliveryCost = new Money { Amount = 1000m, Currency = "USD" },
            Customer = new Customer { Name = "ACME Freight" }
        };
        loadRepo.Query().Returns(new[] { load }.BuildMock());

        var sut = new GetLoadsHandler(tenantUow, Substitute.For<ICurrentUserService>());
        var result = await sut.Handle(new GetLoadsQuery { Search = "acme" }, CancellationToken.None);

        Assert.Single(result.Value!);
    }
}
