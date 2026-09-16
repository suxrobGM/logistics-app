using Logistics.Application.Abstractions.AICopilot;
using Logistics.Application.Modules.Integrations.AICopilot.Commands;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AICopilot;

public class RejectAICopilotDecisionHandlerTests
{
    private readonly AgentTestContext _ctx = new();
    private readonly IAICopilotBroadcastService _broadcastService = Substitute.For<IAICopilotBroadcastService>();
    private readonly RejectAICopilotDecisionHandler _sut;

    public RejectAICopilotDecisionHandlerTests()
    {
        _sut = new RejectAICopilotDecisionHandler(
            _ctx.TenantUow, _ctx.CopilotGuard, _ctx.Notes, _ctx.CurrentUser, _broadcastService);
    }

    [Fact]
    public async Task Handle_DispatchDecision_IsNotRejectableViaCopilot()
    {
        var (decision, _) = _ctx.SetCopilotSuggestedDecision(sessionType: AgentSessionType.Dispatch);

        var result = await _sut.Handle(
            new RejectAICopilotDecisionCommand { DecisionId = decision.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await _broadcastService.DidNotReceiveWithAnyArgs().BroadcastMessageAsync(default, default, default!);
    }

    [Fact]
    public async Task Handle_DecisionNotSuggested_Fails()
    {
        var (decision, _) = _ctx.SetCopilotSuggestedDecision();
        decision.Approve(_ctx.UserId);

        var result = await _sut.Handle(
            new RejectAICopilotDecisionCommand { DecisionId = decision.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("suggested state", result.Error);
    }

    /// <summary>Copilot rejection notes go to the owner's private group, not the tenant board.</summary>
    [Fact]
    public async Task Handle_HappyPath_AppendsRejectionNoteInlineAndBroadcasts()
    {
        var (decision, conversation) = _ctx.SetCopilotSuggestedDecision();

        var result = await _sut.Handle(
            new RejectAICopilotDecisionCommand { DecisionId = decision.Id, Reason = "wrong invoice" },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(AgentDecisionStatus.Rejected, decision.Status);
        Assert.Equal(_ctx.UserId, decision.ApprovedByUserId);

        var note = Assert.Single(conversation.Messages);
        Assert.Equal(AgentMessageRole.System, note.Role);
        Assert.Equal("Rejected: send_invoice - wrong invoice", note.DisplayText);

        // Adding to the navigation alone saves as an UPDATE affecting 0 rows - see ef-persistence.md.
        await _ctx.MessageRepo.Received(1).AddAsync(note, Arg.Any<CancellationToken>());

        await _broadcastService.Received(1).BroadcastMessageAsync(
            _ctx.Tenant.Id, conversation.CreatedById, Arg.Any<AgentMessageDto>());
        await _broadcastService.Received(1).BroadcastDecisionAsync(
            _ctx.Tenant.Id, conversation.CreatedById, Arg.Any<AgentDecisionDto>());
    }

    [Fact]
    public async Task Handle_NoReasonGiven_NoteOmitsTheDash()
    {
        var (decision, conversation) = _ctx.SetCopilotSuggestedDecision();

        await _sut.Handle(
            new RejectAICopilotDecisionCommand { DecisionId = decision.Id }, CancellationToken.None);

        Assert.Equal("Rejected: send_invoice", Assert.Single(conversation.Messages).DisplayText);
    }
}
