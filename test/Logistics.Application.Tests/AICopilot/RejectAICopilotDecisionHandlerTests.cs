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
    private readonly AgentTestContext ctx = new();
    private readonly IAICopilotBroadcastService broadcastService = Substitute.For<IAICopilotBroadcastService>();
    private readonly RejectAICopilotDecisionHandler sut;

    public RejectAICopilotDecisionHandlerTests()
    {
        sut = new RejectAICopilotDecisionHandler(ctx.TenantUow, ctx.CurrentUser, broadcastService);
    }

    [Fact]
    public async Task Handle_DispatchDecision_IsNotRejectableViaCopilot()
    {
        var (decision, _) = ctx.SetCopilotSuggestedDecision(sessionType: AgentSessionType.Dispatch);

        var result = await sut.Handle(
            new RejectAICopilotDecisionCommand { DecisionId = decision.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await broadcastService.DidNotReceiveWithAnyArgs().BroadcastMessageAsync(default, default, default!);
    }

    [Fact]
    public async Task Handle_DecisionNotSuggested_Fails()
    {
        var (decision, _) = ctx.SetCopilotSuggestedDecision();
        decision.Approve(ctx.UserId);

        var result = await sut.Handle(
            new RejectAICopilotDecisionCommand { DecisionId = decision.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("suggested state", result.Error);
    }

    /// <summary>Copilot rejection notes go to the owner's private group, not the tenant board.</summary>
    [Fact]
    public async Task Handle_HappyPath_AppendsRejectionNoteInlineAndBroadcasts()
    {
        var (decision, conversation) = ctx.SetCopilotSuggestedDecision();

        var result = await sut.Handle(
            new RejectAICopilotDecisionCommand { DecisionId = decision.Id, Reason = "wrong invoice" },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(AgentDecisionStatus.Rejected, decision.Status);
        Assert.Equal(ctx.UserId, decision.ApprovedByUserId);

        var note = Assert.Single(conversation.Messages);
        Assert.Equal(AgentMessageRole.System, note.Role);
        Assert.Equal("Rejected: send_invoice - wrong invoice", note.DisplayText);

        // Adding to the navigation alone saves as an UPDATE affecting 0 rows - see ef-persistence.md.
        await ctx.MessageRepo.Received(1).AddAsync(note, Arg.Any<CancellationToken>());

        await broadcastService.Received(1).BroadcastMessageAsync(
            ctx.Tenant.Id, conversation.CreatedById, Arg.Any<AgentMessageDto>());
        await broadcastService.Received(1).BroadcastDecisionAsync(
            ctx.Tenant.Id, conversation.CreatedById, Arg.Any<AgentDecisionDto>());
    }

    [Fact]
    public async Task Handle_NoReasonGiven_NoteOmitsTheDash()
    {
        var (decision, conversation) = ctx.SetCopilotSuggestedDecision();

        await sut.Handle(
            new RejectAICopilotDecisionCommand { DecisionId = decision.Id }, CancellationToken.None);

        Assert.Equal("Rejected: send_invoice", Assert.Single(conversation.Messages).DisplayText);
    }
}
