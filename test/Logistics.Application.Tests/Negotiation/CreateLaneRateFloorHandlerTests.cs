using System.Linq.Expressions;
using Logistics.Application.Modules.Integrations.Negotiation.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Negotiation;

public class CreateLaneRateFloorHandlerTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<LaneRateFloor, Guid> floorRepo =
        Substitute.For<ITenantRepository<LaneRateFloor, Guid>>();

    private readonly CreateLaneRateFloorHandler sut;

    public CreateLaneRateFloorHandlerTests()
    {
        tenantUow.Repository<LaneRateFloor>().Returns(floorRepo);
        sut = new CreateLaneRateFloorHandler(tenantUow);
    }

    private static CreateLaneRateFloorCommand Command() => new()
    {
        OriginCountry = "us",
        OriginState = " tx ",
        DestinationCountry = "us",
        DestinationState = " il ",
        MinRatePerMile = 2.25m
    };

    private void SetupExisting(LaneRateFloor? existing) =>
        floorRepo.GetAsync(Arg.Any<Expression<Func<LaneRateFloor, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(existing);

    [Fact]
    public async Task Handle_NoDuplicate_AddsFloorNormalizedAndSaves()
    {
        SetupExisting(null);

        var result = await sut.Handle(Command(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await floorRepo.Received(1).AddAsync(
            Arg.Is<LaneRateFloor>(f =>
                f.OriginCountry == "US" && f.OriginState == "TX" &&
                f.DestinationCountry == "US" && f.DestinationState == "IL" &&
                f.MinRatePerMile == 2.25m),
            Arg.Any<CancellationToken>());
        await tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateLane_ReturnsFailWithoutAdding()
    {
        SetupExisting(new LaneRateFloor { OriginState = "TX", DestinationState = "IL", MinRatePerMile = 1.00m });

        var result = await sut.Handle(Command(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("already exists", result.Error);
        await floorRepo.DidNotReceive().AddAsync(Arg.Any<LaneRateFloor>(), Arg.Any<CancellationToken>());
        await tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
