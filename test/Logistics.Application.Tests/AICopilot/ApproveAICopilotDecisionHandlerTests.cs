using Logistics.Application.Abstractions.AI;
using Logistics.Application.Abstractions.AICopilot;
using Logistics.Application.Modules.Integrations.AICopilot.Commands;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AICopilot;

public class ApproveAICopilotDecisionHandlerTests
{
    private readonly AgentTestContext ctx = new();
    private readonly IAICopilotBroadcastService broadcastService = Substitute.For<IAICopilotBroadcastService>();
    private readonly ApproveAICopilotDecisionHandler sut;

    public ApproveAICopilotDecisionHandlerTests()
    {
        ctx.SetCallerPermissions("Permission.Invoice.Manage");
        ctx.SetToolDefinition("send_invoice", "Permission.Invoice.Manage", AgentDecisionType.SendInvoice);

        sut = new ApproveAICopilotDecisionHandler(
            ctx.TenantUow, ctx.CurrentUser, ctx.ToolExecutor, ctx.ToolRegistry, broadcastService, ctx.Mediator,
            Options.Create(new LlmOptions { BypassAIGate = true }));
    }

    [Fact]
    public async Task Handle_DispatchDecision_IsNotApprovableViaCopilot()
    {
        var (decision, _) = ctx.SetCopilotSuggestedDecision(sessionType: AgentSessionType.Dispatch);

        var result = await sut.Handle(
            new ApproveAICopilotDecisionCommand { DecisionId = decision.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await ctx.ToolExecutor.DidNotReceiveWithAnyArgs().ExecuteToolAsync(default!, default!, default);
    }

    [Fact]
    public async Task Handle_ApproverLacksToolPermission_FailsWithoutExecuting()
    {
        var (decision, _) = ctx.SetCopilotSuggestedDecision();
        ctx.SetCallerPermissions("Permission.Copilot.Manage");

        var result = await sut.Handle(
            new ApproveAICopilotDecisionCommand { DecisionId = decision.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Permission.Invoice.Manage", result.Error);
        Assert.Equal(AgentDecisionStatus.Suggested, decision.Status);
        await ctx.ToolExecutor.DidNotReceiveWithAnyArgs().ExecuteToolAsync(default!, default!, default);
    }

    [Fact]
    public async Task Handle_HappyPath_ExecutesAppendsOutcomeNoteAndBroadcasts()
    {
        var (decision, conversation) = ctx.SetCopilotSuggestedDecision();
        ctx.ToolExecutor.ExecuteToolAsync("send_invoice", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("""{"success":true}""");

        var result = await sut.Handle(
            new ApproveAICopilotDecisionCommand { DecisionId = decision.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(AgentDecisionStatus.Executed, decision.Status);
        Assert.Equal(ctx.UserId, decision.ApprovedByUserId);

        var note = Assert.Single(conversation.Messages);
        Assert.Equal(AgentMessageRole.System, note.Role);
        Assert.StartsWith("Approved and executed: send_invoice", note.DisplayText);

        // Adding to the navigation alone saves as an UPDATE affecting 0 rows - see ef-persistence.md.
        await ctx.MessageRepo.Received(1).AddAsync(note, Arg.Any<CancellationToken>());

        await broadcastService.Received(1).BroadcastMessageAsync(
            Arg.Any<Guid>(), conversation.CreatedById, Arg.Any<AgentMessageDto>());
        await broadcastService.Received(1).BroadcastDecisionAsync(
            Arg.Any<Guid>(), conversation.CreatedById, Arg.Any<AgentDecisionDto>());
    }

    [Fact]
    public async Task Handle_ToolThrows_MarksFailedAndStillNotesOutcome()
    {
        var (decision, conversation) = ctx.SetCopilotSuggestedDecision();
        ctx.ToolExecutor.ExecuteToolAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Task<string>>(_ => throw new InvalidOperationException("boom"));

        var result = await sut.Handle(
            new ApproveAICopilotDecisionCommand { DecisionId = decision.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(AgentDecisionStatus.Failed, decision.Status);
        Assert.Contains("failed to execute", Assert.Single(conversation.Messages).DisplayText);
    }
}
