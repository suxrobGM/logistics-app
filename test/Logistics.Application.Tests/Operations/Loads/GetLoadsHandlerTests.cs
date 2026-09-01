using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Operations.Loads.Queries;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Identity.Roles;
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
}
