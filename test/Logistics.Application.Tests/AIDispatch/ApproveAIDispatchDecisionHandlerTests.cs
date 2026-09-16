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
    private readonly AgentTestContext _ctx = new();
    private readonly IAIDispatchBroadcastService _broadcastService = Substitute.For<IAIDispatchBroadcastService>();
    private readonly ApproveAIDispatchDecisionHandler _sut;

    public ApproveAIDispatchDecisionHandlerTests()
    {
        _ctx.SetCallerPermissions("Permission.Dispatch.Manage");
        _ctx.SetToolDefinition("assign_load_to_truck", "Permission.Dispatch.Manage", AgentDecisionType.AssignLoad);

        _sut = new ApproveAIDispatchDecisionHandler(
            _ctx.TenantUow, _ctx.DispatchGuard(), _ctx.Authorization, _ctx.Execution, _ctx.Notes,
            _ctx.CurrentUser, _ctx.RunContext, _broadcastService);
    }

    [Fact]
    public async Task Handle_ApproverLacksToolPermission_FailsWithoutExecuting()
    {
        var decision = _ctx.SetDispatchSuggestedDecision();
        _ctx.SetCallerPermissions("Permission.Load.View");

        var result = await _sut.Handle(
            new ApproveAIDispatchDecisionCommand { DecisionId = decision.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Permission.Dispatch.Manage", result.Error);
        Assert.Equal(AgentDecisionStatus.Suggested, decision.Status);
        await _ctx.ToolExecutor.DidNotReceiveWithAnyArgs().ExecuteToolAsync(default!, default!, default);
    }

    [Fact]
    public async Task Handle_ToolSucceeds_AppendsApprovedNoteAndBroadcastsTenantWide()
    {
        var conversation = _ctx.SetConversation(kind: AgentConversationKind.Dispatch);
        var decision = _ctx.SetDispatchSuggestedDecision(conversation);
        _ctx.ToolExecutor.ExecuteToolAsync("assign_load_to_truck", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("""{"success":true}""");

        var result = await _sut.Handle(
            new ApproveAIDispatchDecisionCommand { DecisionId = decision.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var note = Assert.Single(conversation.Messages);
        Assert.Equal(AgentMessageRole.System, note.Role);
        Assert.StartsWith("Approved and executed: assign_load_to_truck", note.DisplayText);

        // Adding to the navigation alone saves as an UPDATE affecting 0 rows - see ef-persistence.md.
        await _ctx.MessageRepo.Received(1).AddAsync(note, Arg.Any<CancellationToken>());

        await _broadcastService.Received(1).BroadcastMessageAsync(
            _ctx.Tenant.Id, Arg.Is<AgentMessageDto>(m => m.ConversationId == conversation.Id));
    }

    [Fact]
    public async Task Handle_ToolSucceeds_AttributesTheNoteToTheApprover()
    {
        var conversation = _ctx.SetConversation(kind: AgentConversationKind.Dispatch);
        var decision = _ctx.SetDispatchSuggestedDecision(conversation);
        _ctx.SetEmployees((_ctx.UserId, "Sarah", "Thompson"));
        _ctx.ToolExecutor.ExecuteToolAsync("assign_load_to_truck", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("""{"success":true}""");

        await _sut.Handle(
            new ApproveAIDispatchDecisionCommand { DecisionId = decision.Id }, CancellationToken.None);

        Assert.Equal(_ctx.UserId, Assert.Single(conversation.Messages).SentByUserId);
        await _broadcastService.Received(1).BroadcastMessageAsync(
            _ctx.Tenant.Id, Arg.Is<AgentMessageDto>(m => m.SentByName == "Sarah Thompson"));
    }

    [Fact]
    public async Task Handle_ToolThrows_AppendsFailureNote()
    {
        var conversation = _ctx.SetConversation(kind: AgentConversationKind.Dispatch);
        var decision = _ctx.SetDispatchSuggestedDecision(conversation);
        _ctx.ToolExecutor.ExecuteToolAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Task<string>>(_ => throw new InvalidOperationException("boom"));

        var result = await _sut.Handle(
            new ApproveAIDispatchDecisionCommand { DecisionId = decision.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(AgentDecisionStatus.Failed, decision.Status);
        Assert.Contains("failed to execute", Assert.Single(conversation.Messages).DisplayText);
    }
}
