using Logistics.Application.Modules.IdentityAccess.Roles.Queries;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.IdentityAccess.Roles;

public class GetAppRolesHandlerTests
{
    private readonly IMasterUnitOfWork _masterUow = Substitute.For<IMasterUnitOfWork>();
    private readonly IMasterRepository<AppRole, Guid> _roleRepo =
        Substitute.For<IMasterRepository<AppRole, Guid>>();

    private readonly GetAppRolesHandler _sut;

    public GetAppRolesHandlerTests()
    {
        _masterUow.Repository<AppRole>().Returns(_roleRepo);
        _sut = new GetAppRolesHandler(_masterUow);
    }

    // Stored in reverse name order, so paging the unordered set would fail this.
    [Fact]
    public async Task Handle_RolesStoredOutOfNameOrder_ReturnsThemOrderedByName()
    {
        var zeta = new AppRole("zeta");
        var alpha = new AppRole("alpha");
        _roleRepo.Query().Returns(new[] { zeta, alpha }.AsQueryable());

        var result = await _sut.Handle(new GetAppRolesQuery(), CancellationToken.None);

        Assert.Equal(["app.alpha", "app.zeta"], result.Value!.Select(r => r.Name).ToArray());
    }

    [Fact]
    public async Task Handle_OrderByIsSupplied_HonoursIt()
    {
        var alpha = new AppRole("alpha");
        var zeta = new AppRole("zeta");
        _roleRepo.Query().Returns(new[] { alpha, zeta }.AsQueryable());

        var result = await _sut.Handle(new GetAppRolesQuery { OrderBy = "-Name" }, CancellationToken.None);

        Assert.Equal(["app.zeta", "app.alpha"], result.Value!.Select(r => r.Name).ToArray());
    }

    // Both roles sort equally by name, so only the Id tie-breaker decides page 1.
    // DisplayName carries the identity because RoleDto does not expose Id.
    [Fact]
    public async Task Handle_RolesShareAName_TheIdTieBreakerDecidesThePageBoundary()
    {
        var second = new AppRole("same")
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            DisplayName = "second"
        };
        var first = new AppRole("same")
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            DisplayName = "first"
        };
        _roleRepo.Query().Returns(new[] { second, first }.AsQueryable());

        var result = await _sut.Handle(new GetAppRolesQuery { PageSize = 1 }, CancellationToken.None);

        var page = result.Value!.ToArray();
        Assert.Single(page);
        Assert.Equal("first", page[0].DisplayName);
        Assert.Equal(2, result.TotalItems);
    }
}
