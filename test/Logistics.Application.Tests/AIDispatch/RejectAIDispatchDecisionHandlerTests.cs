using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Modules.Integrations.AIDispatch.Commands;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AIDispatch;

public class RejectAIDispatchDecisionHandlerTests
{
    private readonly AgentTestContext _ctx = new();
    private readonly IAIDispatchBroadcastService _broadcastService = Substitute.For<IAIDispatchBroadcastService>();
    private readonly RejectAIDispatchDecisionHandler _sut;

    public RejectAIDispatchDecisionHandlerTests()
    {
        _sut = new RejectAIDispatchDecisionHandler(
            _ctx.TenantUow, _ctx.DispatchGuard(), _ctx.Notes, _ctx.CurrentUser, _broadcastService);
    }

    [Fact]
    public async Task Handle_ReasonGiven_AppendsRejectionNoteAndBroadcastsTenantWide()
    {
        var conversation = _ctx.SetConversation(kind: AgentConversationKind.Dispatch);
        var decision = _ctx.SetDispatchSuggestedDecision(conversation);

        var result = await _sut.Handle(
            new RejectAIDispatchDecisionCommand { DecisionId = decision.Id, Reason = "wrong truck" },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var note = Assert.Single(conversation.Messages);
        Assert.Equal(AgentMessageRole.System, note.Role);
        Assert.Equal("Rejected: assign_load_to_truck - wrong truck", note.DisplayText);

        await _broadcastService.Received(1).BroadcastMessageAsync(
            _ctx.Tenant.Id, Arg.Is<AgentMessageDto>(m => m.ConversationId == conversation.Id));
    }

    /// <summary>Everyone on the board reads the note, so it has to say who rejected it.</summary>
    [Fact]
    public async Task Handle_ReasonGiven_AttributesTheNoteToTheRejecter()
    {
        var conversation = _ctx.SetConversation(kind: AgentConversationKind.Dispatch);
        var decision = _ctx.SetDispatchSuggestedDecision(conversation);
        _ctx.SetEmployees((_ctx.UserId, "Marcus", "Johnson"));

        await _sut.Handle(
            new RejectAIDispatchDecisionCommand { DecisionId = decision.Id, Reason = "wrong truck" },
            CancellationToken.None);

        Assert.Equal(_ctx.UserId, Assert.Single(conversation.Messages).SentByUserId);
        await _broadcastService.Received(1).BroadcastMessageAsync(
            _ctx.Tenant.Id, Arg.Is<AgentMessageDto>(m => m.SentByName == "Marcus Johnson"));
    }

    [Fact]
    public async Task Handle_NoReasonGiven_NoteOmitsTheDash()
    {
        var conversation = _ctx.SetConversation(kind: AgentConversationKind.Dispatch);
        var decision = _ctx.SetDispatchSuggestedDecision(conversation);

        await _sut.Handle(
            new RejectAIDispatchDecisionCommand { DecisionId = decision.Id }, CancellationToken.None);

        Assert.Equal("Rejected: assign_load_to_truck", Assert.Single(conversation.Messages).DisplayText);
    }
}
