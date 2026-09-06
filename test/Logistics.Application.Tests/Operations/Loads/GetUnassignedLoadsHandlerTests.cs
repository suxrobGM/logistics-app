using System.Linq.Expressions;
using Logistics.Application.Modules.Operations.Loads.Queries;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Loads;

public class GetUnassignedLoadsHandlerTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<Load, Guid> loadRepo =
        Substitute.For<ITenantRepository<Load, Guid>>();

    private readonly GetUnassignedLoadsHandler sut;

    public GetUnassignedLoadsHandlerTests()
    {
        tenantUow.Repository<Load>().Returns(loadRepo);
        sut = new GetUnassignedLoadsHandler(tenantUow);
    }

    // No container or terminal ids, so LoadIntermodalResolver short-circuits both of its lookups
    // and this test needs no repository beyond Load.
    private static Load CreateDraftLoad(string name, DateTime createdAt) => new()
    {
        Name = name,
        Type = LoadType.GeneralFreight,
        OriginAddress = new Address { Line1 = "1 A St", City = "NYC", State = "NY", ZipCode = "10001", Country = "US" },
        OriginLocation = new GeoPoint(-74.0, 40.7),
        DestinationAddress = new Address { Line1 = "2 B St", City = "LA", State = "CA", ZipCode = "90001", Country = "US" },
        DestinationLocation = new GeoPoint(-118.2, 34.0),
        DeliveryCost = new Money { Amount = 1000m, Currency = "USD" },
        Customer = new Customer { Name = "ACME" },
        CreatedAt = createdAt
    };

    /// <summary>
    /// The source is stored oldest-first, so a handler that paged the unordered set would return
    /// it unchanged and fail this. Newest-first follows the portal handlers' own default arm.
    /// </summary>
    [Fact]
    public async Task Handle_DraftLoadsStoredOldestFirst_ReturnsThemNewestFirst()
    {
        var older = CreateDraftLoad("older", new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var newer = CreateDraftLoad("newer", new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc));
        loadRepo.Query().Returns(new[] { older, newer }.BuildMock());
        loadRepo.CountAsync(Arg.Any<Expression<Func<Load, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(2);

        var result = await sut.Handle(new GetUnassignedLoadsQuery(), CancellationToken.None);

        Assert.Equal(["newer", "older"], result.Value!.Select(l => l.Name).ToArray());
    }

    /// <summary>
    /// Two loads created at the same instant sort equally, so only the Id tie-breaker decides
    /// which one page 1 contains - without it LINQ's stable sort returns the source order, which
    /// is the opposite of what this asserts.
    /// </summary>
    [Fact]
    public async Task Handle_LoadsShareACreatedAt_TheIdTieBreakerDecidesThePageBoundary()
    {
        var sameInstant = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        var second = CreateDraftLoad("second", sameInstant);
        second.Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var first = CreateDraftLoad("first", sameInstant);
        first.Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        loadRepo.Query().Returns(new[] { second, first }.BuildMock());
        loadRepo.CountAsync(Arg.Any<Expression<Func<Load, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(2);

        var result = await sut.Handle(
            new GetUnassignedLoadsQuery { PageSize = 1 }, CancellationToken.None);

        var page = result.Value!.ToArray();
        Assert.Single(page);
        Assert.Equal("first", page[0].Name);
    }
}
