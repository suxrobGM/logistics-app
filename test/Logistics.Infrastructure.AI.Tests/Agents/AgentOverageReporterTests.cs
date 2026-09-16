using Logistics.Application.Abstractions.Payments.Stripe;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Agents;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Agents;

/// <summary>
/// Both agent surfaces meter through this, so the rules here are what keeps dispatch and the
/// copilot billing the same way.
/// </summary>
public class AgentOverageReporterTests
{
    private readonly IStripeUsageService _stripeUsageService = Substitute.For<IStripeUsageService>();
    private readonly AgentOverageReporter _sut;
    private readonly Guid _tenantId = Guid.NewGuid();

    public AgentOverageReporterTests()
    {
        _sut = new AgentOverageReporter(_stripeUsageService, NullLogger<AgentOverageReporter>.Instance);
    }

    private static AgentSession Session(AgentSessionType type, bool isOverage, decimal cost = 0.25m)
    {
        var session = new AgentSession
        {
            Type = type,
            StartedAt = DateTime.UtcNow,
            IsOverage = isOverage,
            EstimatedCostUsd = cost
        };
        return session;
    }

    [Theory]
    [InlineData(AgentSessionType.Dispatch)]
    [InlineData(AgentSessionType.Copilot)]
    public async Task ReportIfOverBudget_CompletedOverBudgetSession_ReportsRawCost(AgentSessionType type)
    {
        // Both surfaces bill - the copilot used to be blocked at the boundary instead.
        var session = Session(type, isOverage: true);
        session.Complete("done");

        await _sut.ReportIfOverBudgetAsync(session, _tenantId);

        await _stripeUsageService.Received(1)
            .ReportAISessionOverageAsync(_tenantId, 0.25m, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReportIfOverBudget_WithinBudget_ReportsNothing()
    {
        var session = Session(AgentSessionType.Copilot, isOverage: false);
        session.Complete("done");

        await _sut.ReportIfOverBudgetAsync(session, _tenantId);

        await _stripeUsageService.DidNotReceiveWithAnyArgs()
            .ReportAISessionOverageAsync(default, default, default);
    }

    [Fact]
    public async Task ReportIfOverBudget_FailedSession_ReportsNothing()
    {
        // The budget still absorbed the cost; charging for an answer nobody got would not be.
        var session = Session(AgentSessionType.Copilot, isOverage: true);
        session.Fail("boom");

        await _sut.ReportIfOverBudgetAsync(session, _tenantId);

        await _stripeUsageService.DidNotReceiveWithAnyArgs()
            .ReportAISessionOverageAsync(default, default, default);
    }

    [Fact]
    public async Task ReportIfOverBudget_CancelledSession_ReportsNothing()
    {
        var session = Session(AgentSessionType.Copilot, isOverage: true);
        session.Cancel();

        await _sut.ReportIfOverBudgetAsync(session, _tenantId);

        await _stripeUsageService.DidNotReceiveWithAnyArgs()
            .ReportAISessionOverageAsync(default, default, default);
    }

    [Fact]
    public async Task ReportIfOverBudget_StripeThrows_DoesNotPropagate()
    {
        // The turn already ran and its output is the user's - losing the meter event costs us the
        // charge, throwing here would cost them the answer.
        _stripeUsageService
            .ReportAISessionOverageAsync(Arg.Any<Guid>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("stripe down"));

        var session = Session(AgentSessionType.Copilot, isOverage: true);
        session.Complete("done");

        await _sut.ReportIfOverBudgetAsync(session, _tenantId);
    }
}
