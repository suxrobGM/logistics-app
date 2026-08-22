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
    private readonly AgentTestContext ctx = new();
    private readonly IAIDispatchBroadcastService broadcastService = Substitute.For<IAIDispatchBroadcastService>();
    private readonly RejectAIDispatchDecisionHandler sut;

    public RejectAIDispatchDecisionHandlerTests()
    {
        sut = new RejectAIDispatchDecisionHandler(
            ctx.TenantUow, ctx.DispatchGuard(), ctx.Notes, ctx.CurrentUser, broadcastService);
    }

    [Fact]
    public async Task Handle_ReasonGiven_AppendsRejectionNoteAndBroadcastsTenantWide()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Dispatch);
        var decision = ctx.SetDispatchSuggestedDecision(conversation);

        var result = await sut.Handle(
            new RejectAIDispatchDecisionCommand { DecisionId = decision.Id, Reason = "wrong truck" },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var note = Assert.Single(conversation.Messages);
        Assert.Equal(AgentMessageRole.System, note.Role);
        Assert.Equal("Rejected: assign_load_to_truck - wrong truck", note.DisplayText);

        await broadcastService.Received(1).BroadcastMessageAsync(
            ctx.Tenant.Id, Arg.Is<AgentMessageDto>(m => m.ConversationId == conversation.Id));
    }

    /// <summary>Everyone on the board reads the note, so it has to say who rejected it.</summary>
    [Fact]
    public async Task Handle_ReasonGiven_AttributesTheNoteToTheRejecter()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Dispatch);
        var decision = ctx.SetDispatchSuggestedDecision(conversation);
        ctx.SetEmployees((ctx.UserId, "Marcus", "Johnson"));

        await sut.Handle(
            new RejectAIDispatchDecisionCommand { DecisionId = decision.Id, Reason = "wrong truck" },
            CancellationToken.None);

        Assert.Equal(ctx.UserId, Assert.Single(conversation.Messages).SentByUserId);
        await broadcastService.Received(1).BroadcastMessageAsync(
            ctx.Tenant.Id, Arg.Is<AgentMessageDto>(m => m.SentByName == "Marcus Johnson"));
    }

    [Fact]
    public async Task Handle_NoReasonGiven_NoteOmitsTheDash()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Dispatch);
        var decision = ctx.SetDispatchSuggestedDecision(conversation);

        await sut.Handle(
            new RejectAIDispatchDecisionCommand { DecisionId = decision.Id }, CancellationToken.None);

        Assert.Equal("Rejected: assign_load_to_truck", Assert.Single(conversation.Messages).DisplayText);
    }
}
