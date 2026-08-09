using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Modules.Integrations.AIDispatch.Commands;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AIDispatch;

public class ApproveAIDispatchDecisionHandlerTests
{
    private readonly AgentTestContext ctx = new();
    private readonly IAIDispatchBroadcastService broadcastService = Substitute.For<IAIDispatchBroadcastService>();
    private readonly ApproveAIDispatchDecisionHandler sut;

    public ApproveAIDispatchDecisionHandlerTests()
    {
        ctx.SetCallerPermissions("Permission.Dispatch.Manage");
        ctx.SetToolDefinition("assign_load_to_truck", "Permission.Dispatch.Manage", AgentDecisionType.AssignLoad);

        sut = new ApproveAIDispatchDecisionHandler(
            ctx.TenantUow, ctx.DispatchGuard(), ctx.Authorization, ctx.Execution, ctx.Notes,
            ctx.CurrentUser, broadcastService);
    }

    [Fact]
    public async Task Handle_ApproverLacksToolPermission_FailsWithoutExecuting()
    {
        var decision = ctx.SetDispatchSuggestedDecision();
        ctx.SetCallerPermissions("Permission.Load.View");

        var result = await sut.Handle(
            new ApproveAIDispatchDecisionCommand { DecisionId = decision.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Permission.Dispatch.Manage", result.Error);
        Assert.Equal(AgentDecisionStatus.Suggested, decision.Status);
        await ctx.ToolExecutor.DidNotReceiveWithAnyArgs().ExecuteToolAsync(default!, default!, default);
    }

    [Fact]
    public async Task Handle_ToolSucceeds_AppendsApprovedNoteAndBroadcastsTenantWide()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Dispatch);
        var decision = ctx.SetDispatchSuggestedDecision(conversation);
        ctx.ToolExecutor.ExecuteToolAsync("assign_load_to_truck", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("""{"success":true}""");

        var result = await sut.Handle(
            new ApproveAIDispatchDecisionCommand { DecisionId = decision.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var note = Assert.Single(conversation.Messages);
        Assert.Equal(AgentMessageRole.System, note.Role);
        Assert.StartsWith("Approved and executed: assign_load_to_truck", note.DisplayText);

        // Adding to the navigation alone saves as an UPDATE affecting 0 rows - see ef-persistence.md.
        await ctx.MessageRepo.Received(1).AddAsync(note, Arg.Any<CancellationToken>());

        await broadcastService.Received(1).BroadcastMessageAsync(
            ctx.Tenant.Id, Arg.Is<AgentMessageDto>(m => m.ConversationId == conversation.Id));
    }

    [Fact]
    public async Task Handle_ToolThrows_AppendsFailureNote()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Dispatch);
        var decision = ctx.SetDispatchSuggestedDecision(conversation);
        ctx.ToolExecutor.ExecuteToolAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Task<string>>(_ => throw new InvalidOperationException("boom"));

        var result = await sut.Handle(
            new ApproveAIDispatchDecisionCommand { DecisionId = decision.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(AgentDecisionStatus.Failed, decision.Status);
        Assert.Contains("failed to execute", Assert.Single(conversation.Messages).DisplayText);
    }
}
