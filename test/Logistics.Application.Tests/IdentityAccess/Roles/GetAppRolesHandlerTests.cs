using Logistics.Application.Modules.IdentityAccess.Roles.Queries;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.IdentityAccess.Roles;

public class GetAppRolesHandlerTests
{
    private readonly IMasterUnitOfWork masterUow = Substitute.For<IMasterUnitOfWork>();
    private readonly IMasterRepository<AppRole, Guid> roleRepo =
        Substitute.For<IMasterRepository<AppRole, Guid>>();

    private readonly GetAppRolesHandler sut;

    public GetAppRolesHandlerTests()
    {
        masterUow.Repository<AppRole>().Returns(roleRepo);
        sut = new GetAppRolesHandler(masterUow);
    }

    /// <summary>
    /// The source is deliberately stored in reverse name order, so a handler that paged the
    /// unordered set would return it unchanged and fail this.
    /// </summary>
    [Fact]
    public async Task Handle_RolesStoredOutOfNameOrder_ReturnsThemOrderedByName()
    {
        var zeta = new AppRole("zeta");
        var alpha = new AppRole("alpha");
        roleRepo.Query().Returns(new[] { zeta, alpha }.AsQueryable());

        var result = await sut.Handle(new GetAppRolesQuery(), CancellationToken.None);

        Assert.Equal(["app.alpha", "app.zeta"], result.Value!.Select(r => r.Name).ToArray());
    }

    /// <summary>
    /// Paging is only safe over a total order, and a name is not unique. Both roles here sort
    /// equally by name, so only the Id tie-breaker decides which one page 1 contains - without it
    /// LINQ's stable sort returns the source order and this asserts the opposite.
    /// DisplayName carries the identity because RoleDto does not expose Id.
    /// </summary>
    [Fact]
    public async Task Handle_RolesShareAName_TheIdTieBreakerDecidesThePageBoundary()
    {
        var second = new AppRole("same")
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), DisplayName = "second"
        };
        var first = new AppRole("same")
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), DisplayName = "first"
        };
        roleRepo.Query().Returns(new[] { second, first }.AsQueryable());

        var result = await sut.Handle(new GetAppRolesQuery { PageSize = 1 }, CancellationToken.None);

        var page = result.Value!.ToArray();
        Assert.Single(page);
        Assert.Equal("first", page[0].DisplayName);
        Assert.Equal(2, result.TotalItems);
    }
}
