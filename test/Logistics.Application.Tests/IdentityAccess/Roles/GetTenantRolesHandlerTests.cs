using Logistics.Application.Modules.IdentityAccess.Roles.Queries;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.IdentityAccess.Roles;

public class GetTenantRolesHandlerTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<TenantRole, Guid> roleRepo =
        Substitute.For<ITenantRepository<TenantRole, Guid>>();

    private readonly GetTenantRolesHandler sut;

    public GetTenantRolesHandlerTests()
    {
        tenantUow.Repository<TenantRole>().Returns(roleRepo);
        sut = new GetTenantRolesHandler(tenantUow);
    }

    // Stored in reverse name order, so paging the unordered set would fail this.
    [Fact]
    public async Task Handle_RolesStoredOutOfNameOrder_ReturnsThemOrderedByName()
    {
        var zeta = new TenantRole("zeta");
        var alpha = new TenantRole("alpha");
        roleRepo.Query().Returns(new[] { zeta, alpha }.AsQueryable());

        var result = await sut.Handle(new GetTenantRolesQuery(), CancellationToken.None);

        Assert.Equal(["tenant.alpha", "tenant.zeta"], result.Value!.Select(r => r.Name).ToArray());
    }

    // Both roles sort equally by name, so only the Id tie-breaker decides page 1.
    [Fact]
    public async Task Handle_RolesShareAName_TheIdTieBreakerDecidesThePageBoundary()
    {
        var second = new TenantRole("same")
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), DisplayName = "second"
        };
        var first = new TenantRole("same")
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), DisplayName = "first"
        };
        roleRepo.Query().Returns(new[] { second, first }.AsQueryable());

        var result = await sut.Handle(new GetTenantRolesQuery { PageSize = 1 }, CancellationToken.None);

        var page = result.Value!.ToArray();
        Assert.Single(page);
        Assert.Equal("first", page[0].DisplayName);
        Assert.Equal(2, result.TotalItems);
    }
}
