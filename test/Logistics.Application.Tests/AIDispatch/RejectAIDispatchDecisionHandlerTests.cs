using Logistics.Application.Abstractions.AI;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Modules.Integrations.AIDispatch.Commands;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Options;
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
            ctx.TenantUow, ctx.CurrentUser, broadcastService,
            Options.Create(new LlmOptions { BypassAIGate = true }));
    }

    [Fact]
    public async Task Handle_SessionHasNoConversation_RejectsWithoutAppendingANote()
    {
        var decision = ctx.SetDispatchSuggestedDecision(conversationId: null);

        var result = await sut.Handle(
            new RejectAIDispatchDecisionCommand { DecisionId = decision.Id, Reason = "wrong truck" },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(AgentDecisionStatus.Rejected, decision.Status);
        await broadcastService.DidNotReceiveWithAnyArgs().BroadcastMessageAsync(default, default!);
    }

    [Fact]
    public async Task Handle_SessionHasConversation_AppendsRejectionNoteAndBroadcastsTenantWide()
    {
        var conversationId = Guid.NewGuid();
        var decision = ctx.SetDispatchSuggestedDecision(conversationId);
        var conversation = ctx.SetConversation(id: conversationId, kind: AgentConversationKind.Dispatch);

        var result = await sut.Handle(
            new RejectAIDispatchDecisionCommand { DecisionId = decision.Id, Reason = "wrong truck" },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var note = Assert.Single(conversation.Messages);
        Assert.Equal(AgentMessageRole.System, note.Role);
        Assert.Equal("Rejected: assign_load_to_truck - wrong truck", note.DisplayText);

        await broadcastService.Received(1).BroadcastMessageAsync(
            ctx.Tenant.Id, Arg.Is<AgentMessageDto>(m => m.ConversationId == conversationId));
    }

    [Fact]
    public async Task Handle_NoReasonGiven_NoteOmitsTheDash()
    {
        var conversationId = Guid.NewGuid();
        var decision = ctx.SetDispatchSuggestedDecision(conversationId);
        var conversation = ctx.SetConversation(id: conversationId, kind: AgentConversationKind.Dispatch);

        await sut.Handle(
            new RejectAIDispatchDecisionCommand { DecisionId = decision.Id }, CancellationToken.None);

        Assert.Equal("Rejected: assign_load_to_truck", Assert.Single(conversation.Messages).DisplayText);
    }
}
