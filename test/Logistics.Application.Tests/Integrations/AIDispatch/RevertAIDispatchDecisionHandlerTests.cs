using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Modules.Integrations.AIDispatch.Commands;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Integrations.AIDispatch;

public class RevertAIDispatchDecisionHandlerTests
{
    private readonly AgentTestContext _ctx = new();
    private readonly RevertAIDispatchDecisionHandler _sut;

    public RevertAIDispatchDecisionHandlerTests()
    {
        _ctx.TenantUow.Repository<Trip>().Returns(Substitute.For<ITenantRepository<Trip, Guid>>());
        _ctx.TenantUow.Repository<Invoice>().Returns(Substitute.For<ITenantRepository<Invoice, Guid>>());
        _ctx.Mediator.Send(Arg.Any<IRequest<Result>>(), Arg.Any<CancellationToken>()).Returns(Result.Ok());

        _sut = new RevertAIDispatchDecisionHandler(
            _ctx.TenantUow, _ctx.Mediator, _ctx.Notes, _ctx.CurrentUser,
            Substitute.For<IAIDispatchBroadcastService>());
    }

    public static TheoryData<AgentDecisionType> EligibleTypes => [.. AIDispatchPolicy.AutoApproveEligibleTypes];

    /// <summary>A type that can auto-execute but has no undo case could never be reverted.</summary>
    [Theory]
    [MemberData(nameof(EligibleTypes))]
    public async Task Handle_EveryAutoApproveEligibleType_CanBeReverted(AgentDecisionType type)
    {
        var decision = _ctx.SetDispatchSuggestedDecision();
        decision.Type = type;
        decision.LoadId = Guid.NewGuid();
        decision.TripId = Guid.NewGuid();
        decision.InvoiceId = Guid.NewGuid();
        decision.MarkAutoExecuted();

        var result = await _sut.Handle(
            new RevertAIDispatchDecisionCommand { DecisionId = decision.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        Assert.NotNull(decision.RevertedAt);
    }
}
