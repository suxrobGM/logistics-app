using System.Linq.Expressions;
using Logistics.Application.Modules.Integrations.Negotiation.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Negotiation;

public class CreateLaneRateFloorHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<LaneRateFloor, Guid> _floorRepo =
        Substitute.For<ITenantRepository<LaneRateFloor, Guid>>();

    private readonly CreateLaneRateFloorHandler _sut;

    public CreateLaneRateFloorHandlerTests()
    {
        _tenantUow.Repository<LaneRateFloor>().Returns(_floorRepo);
        _sut = new CreateLaneRateFloorHandler(_tenantUow);
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
        _floorRepo.GetAsync(Arg.Any<Expression<Func<LaneRateFloor, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(existing);

    [Fact]
    public async Task Handle_NoDuplicate_AddsFloorNormalizedAndSaves()
    {
        SetupExisting(null);

        var result = await _sut.Handle(Command(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _floorRepo.Received(1).AddAsync(
            Arg.Is<LaneRateFloor>(f =>
                f.OriginCountry == "US" && f.OriginState == "TX" &&
                f.DestinationCountry == "US" && f.DestinationState == "IL" &&
                f.MinRatePerMile == 2.25m),
            Arg.Any<CancellationToken>());
        await _tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateLane_ReturnsFailWithoutAdding()
    {
        SetupExisting(new LaneRateFloor { OriginState = "TX", DestinationState = "IL", MinRatePerMile = 1.00m });

        var result = await _sut.Handle(Command(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("already exists", result.Error);
        await _floorRepo.DidNotReceive().AddAsync(Arg.Any<LaneRateFloor>(), Arg.Any<CancellationToken>());
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
