using System.Linq.Expressions;
using Logistics.Application.Modules.Integrations.AIDispatch.Queries;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AIDispatch;

public class GetPendingDecisionsHandlerTests
{
    private readonly AgentTestContext _ctx = new();
    private readonly ITenantRepository<Load, Guid> _loadRepo = Substitute.For<ITenantRepository<Load, Guid>>();
    private readonly ITenantRepository<Truck, Guid> _truckRepo = Substitute.For<ITenantRepository<Truck, Guid>>();
    private readonly GetPendingDecisionsHandler _sut;

    public GetPendingDecisionsHandlerTests()
    {
        _ctx.TenantUow.Repository<Load>().Returns(_loadRepo);
        _ctx.TenantUow.Repository<Truck>().Returns(_truckRepo);
        _loadRepo.GetListAsync(Arg.Any<Expression<Func<Load, bool>>>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _truckRepo.GetListAsync(Arg.Any<Expression<Func<Truck, bool>>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        _sut = new GetPendingDecisionsHandler(_ctx.TenantUow);
    }

    private static AgentDecision SuggestedDecision(AgentSessionType sessionType) => new()
    {
        Session = new AgentSession { Type = sessionType },
        Status = AgentDecisionStatus.Suggested,
        ToolName = "assign_load_to_truck"
    };

    /// <summary>Copilot suggestions surface in the chat drawer, never on the dispatch board.</summary>
    [Fact]
    public async Task Handle_MixOfSurfacesAndStatuses_OnlyDispatchSuggestedReturned()
    {
        var dispatchSuggested = SuggestedDecision(AgentSessionType.Dispatch);
        var copilotSuggested = SuggestedDecision(AgentSessionType.Copilot);
        var dispatchApproved = SuggestedDecision(AgentSessionType.Dispatch);
        dispatchApproved.Approve(_ctx.UserId);
        _ctx.DecisionRepo.Query().Returns(
            new List<AgentDecision> { dispatchSuggested, copilotSuggested, dispatchApproved }.BuildMock());

        var result = await _sut.Handle(new GetPendingDecisionsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var returned = Assert.Single(result.Value!);
        Assert.Equal(dispatchSuggested.Id, returned.Id);
    }

    [Fact]
    public async Task Handle_DecisionReferencesLoadAndTruck_EnrichesNamesWithoutNPlusOne()
    {
        var load = new Load
        {
            Name = "Load 42",
            Type = LoadType.GeneralFreight,
            Customer = null!,
            OriginAddress = new Address { Line1 = "1 Origin", City = "City", State = "ST", ZipCode = "00000", Country = "US" },
            OriginLocation = new GeoPoint(0, 0),
            DestinationAddress = new Address { Line1 = "1 Dest", City = "City", State = "ST", ZipCode = "00000", Country = "US" },
            DestinationLocation = new GeoPoint(0, 0),
            DeliveryCost = Money.Zero("USD")
        };
        var truck = new Truck { Number = "T-100", Type = TruckType.FreightTruck };
        var decision = SuggestedDecision(AgentSessionType.Dispatch);
        decision.LoadId = load.Id;
        decision.TruckId = truck.Id;
        _ctx.DecisionRepo.Query().Returns(new List<AgentDecision> { decision }.BuildMock());
        _loadRepo.GetListAsync(Arg.Any<Expression<Func<Load, bool>>>(), Arg.Any<CancellationToken>())
            .Returns([load]);
        _truckRepo.GetListAsync(Arg.Any<Expression<Func<Truck, bool>>>(), Arg.Any<CancellationToken>())
            .Returns([truck]);

        var result = await _sut.Handle(new GetPendingDecisionsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = Assert.Single(result.Value!);
        Assert.Equal("Load 42", dto.LoadName);
        Assert.Equal("T-100", dto.TruckNumber);
    }

    [Fact]
    public async Task Handle_DecisionWithoutLoadOrTruck_LeavesNamesNull()
    {
        var decision = SuggestedDecision(AgentSessionType.Dispatch);
        _ctx.DecisionRepo.Query().Returns(new List<AgentDecision> { decision }.BuildMock());

        var result = await _sut.Handle(new GetPendingDecisionsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = Assert.Single(result.Value!);
        Assert.Null(dto.LoadName);
        Assert.Null(dto.TruckNumber);
    }
}
