using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Modules.Integrations.AIDispatch.Commands;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AIDispatch;

public class SendAIDispatchMessageHandlerTests
{
    private readonly AgentTestContext ctx = new();
    private readonly IBackgroundJobRunner<AIDispatchTurnRequest> backgroundRunner =
        Substitute.For<IBackgroundJobRunner<AIDispatchTurnRequest>>();

    private readonly IAIDispatchBroadcastService broadcastService =
        Substitute.For<IAIDispatchBroadcastService>();

    private readonly SendAIDispatchMessageHandler sut;

    public SendAIDispatchMessageHandlerTests()
    {
        SetQuota(overageBlocked: false);

        sut = new SendAIDispatchMessageHandler(
            ctx.Commands, ctx.CurrentUser, backgroundRunner, broadcastService);
    }

    private void SetQuota(bool overageBlocked, bool isOverQuota = false)
    {
        ctx.Tenant.Settings.BlockAIOverage = overageBlocked;
        ctx.QuotaService.GetQuotaStatusAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new AIQuotaStatus(5m, isOverQuota || overageBlocked ? 5m : 0m,
                isOverQuota || overageBlocked)
            {
                OverageBlocked = overageBlocked
            });
    }

    private SendAIDispatchMessageCommand Command(Guid conversationId, string text = "hello") =>
        new() { ConversationId = conversationId, Text = text };

    /// <summary>A copilot-kind conversation must never accept a dispatch send.</summary>
    [Fact]
    public async Task Handle_CopilotKindConversation_Fails()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Copilot);

        var result = await sut.Handle(Command(conversation.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        backgroundRunner.DidNotReceiveWithAnyArgs().Enqueue(default!);
    }

    /// <summary>Dispatch conversations are tenant-shared: any user may send, not only the creator.</summary>
    [Fact]
    public async Task Handle_ConversationCreatedByAnotherUser_StillSucceeds()
    {
        var conversation = ctx.SetConversation(createdById: Guid.NewGuid(), kind: AgentConversationKind.Dispatch);

        var result = await sut.Handle(Command(conversation.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        backgroundRunner.Received(1).Enqueue(Arg.Any<AIDispatchTurnRequest>());
    }

    [Fact]
    public async Task Handle_TurnAlreadyRunning_Fails()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Dispatch);
        conversation.BeginTurn();

        var result = await sut.Handle(Command(conversation.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("in progress", result.Error);
    }

    [Fact]
    public async Task Handle_HappyPath_AppendsMessageBeginsTurnAndEnqueues()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Dispatch);

        var result = await sut.Handle(Command(conversation.Id, "assign what you can"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var message = Assert.Single(conversation.Messages);
        Assert.Equal("assign what you can", message.DisplayText);
        Assert.Equal(AgentMessageRole.User, message.Role);
        Assert.Equal(1, message.Sequence);
        Assert.Equal(AgentConversationStatus.Running, conversation.Status);
        Assert.Equal(message.Id, result.Value!.UserMessageId);

        // Load-bearing: without the explicit Add, EF saves the pre-generated-id message as an UPDATE.
        await ctx.MessageRepo.Received(1).AddAsync(message, Arg.Any<CancellationToken>());
        backgroundRunner.Received(1).Enqueue(Arg.Is<AIDispatchTurnRequest>(r =>
            r.ConversationId == conversation.Id && r.TriggeredByUserId == ctx.UserId));
        await ctx.TenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>A shared board needs the author: the transcript is read by people who did not type it.</summary>
    [Fact]
    public async Task Handle_HappyPath_StampsSenderOnTheMessage()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Dispatch);

        await sut.Handle(Command(conversation.Id), CancellationToken.None);

        var message = Assert.Single(conversation.Messages);
        Assert.Equal(ctx.UserId, message.SentByUserId);
    }

    /// <summary>Without this the other dispatchers see the agent's answer but never the question.</summary>
    [Fact]
    public async Task Handle_HappyPath_BroadcastsTheMessageWithTheSendersName()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Dispatch);
        ctx.SetEmployees((ctx.UserId, "Sarah", "Thompson"));

        await sut.Handle(Command(conversation.Id), CancellationToken.None);

        await broadcastService.Received(1).BroadcastMessageAsync(
            ctx.Tenant.Id,
            Arg.Is<AgentMessageDto>(m =>
                m.SentByUserId == ctx.UserId && m.SentByName == "Sarah Thompson"));
    }

    [Fact]
    public async Task Handle_SenderHasNoEmployeeRow_BroadcastsWithoutAName()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Dispatch);

        await sut.Handle(Command(conversation.Id), CancellationToken.None);

        await broadcastService.Received(1).BroadcastMessageAsync(
            ctx.Tenant.Id, Arg.Is<AgentMessageDto>(m => m.SentByName == null));
    }

    [Fact]
    public async Task Handle_OverageBlocked_FailsWithBudgetErrorCode()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Dispatch);
        SetQuota(overageBlocked: true);

        var result = await sut.Handle(Command(conversation.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.AIBudgetReached, result.ErrorCode);
        Assert.Empty(conversation.Messages);
        Assert.NotEqual(AgentConversationStatus.Running, conversation.Status);
        backgroundRunner.DidNotReceiveWithAnyArgs().Enqueue(default!);
        await broadcastService.DidNotReceiveWithAnyArgs().BroadcastMessageAsync(default, default!);
    }

    [Fact]
    public async Task Handle_OverQuotaWithoutBlock_BillsThroughAndEnqueues()
    {
        var conversation = ctx.SetConversation(kind: AgentConversationKind.Dispatch);
        SetQuota(overageBlocked: false, isOverQuota: true);

        var result = await sut.Handle(Command(conversation.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        backgroundRunner.Received(1).Enqueue(Arg.Any<AIDispatchTurnRequest>());
    }
}
