using Logistics.Application.Abstractions.AICopilot;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Modules.Integrations.AICopilot.Commands;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AICopilot;

public class SendAICopilotMessageHandlerTests
{
    private readonly AgentTestContext _ctx = new();
    private readonly IBackgroundJobRunner<AICopilotTurnRequest> _backgroundRunner =
        Substitute.For<IBackgroundJobRunner<AICopilotTurnRequest>>();

    private readonly SendAICopilotMessageHandler _sut;

    public SendAICopilotMessageHandlerTests()
    {
        SetQuota(overageBlocked: false);

        _sut = new SendAICopilotMessageHandler(_ctx.Commands, _ctx.CurrentUser, _backgroundRunner);
    }

    /// <summary>
    /// Mirrors the real invariant: <c>AIQuotaService</c> only ever reports <c>OverageBlocked</c>
    /// for a tenant that opted in, and the handler skips the quota lookup when the flag is off.
    /// </summary>
    private void SetQuota(bool overageBlocked, bool isOverQuota = false)
    {
        _ctx.Tenant.Settings.BlockAIOverage = overageBlocked;
        _ctx.QuotaService.GetQuotaStatusAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new AIQuotaStatus(5m, isOverQuota || overageBlocked ? 5m : 0m,
                isOverQuota || overageBlocked)
            {
                OverageBlocked = overageBlocked
            });
    }

    private SendAICopilotMessageCommand Command(Guid conversationId, string text = "hello") =>
        new() { ConversationId = conversationId, Text = text };

    [Fact]
    public async Task Handle_NotOwner_Fails()
    {
        var conversation = _ctx.SetConversation(createdById: Guid.NewGuid());

        var result = await _sut.Handle(Command(conversation.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        _backgroundRunner.DidNotReceiveWithAnyArgs().Enqueue(default!);
    }

    /// <summary>A dispatch-kind conversation must never accept a copilot send.</summary>
    [Fact]
    public async Task Handle_DispatchKindConversation_Fails()
    {
        var conversation = _ctx.SetConversation(kind: AgentConversationKind.Dispatch);

        var result = await _sut.Handle(Command(conversation.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        _backgroundRunner.DidNotReceiveWithAnyArgs().Enqueue(default!);
    }

    [Fact]
    public async Task Handle_TurnAlreadyRunning_Fails()
    {
        var conversation = _ctx.SetConversation();
        conversation.BeginTurn();

        var result = await _sut.Handle(Command(conversation.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("in progress", result.Error);
    }

    [Fact]
    public async Task Handle_HappyPath_AppendsMessageBeginsTurnAndEnqueues()
    {
        var conversation = _ctx.SetConversation();

        var result = await _sut.Handle(Command(conversation.Id, "invoice load 42"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var message = Assert.Single(conversation.Messages);
        Assert.Equal("invoice load 42", message.DisplayText);
        Assert.Equal(AgentMessageRole.User, message.Role);
        Assert.Equal(1, message.Sequence);
        Assert.Equal(AgentConversationStatus.Running, conversation.Status);
        Assert.Equal(message.Id, result.Value!.UserMessageId);

        // Load-bearing: without the explicit Add, EF saves the pre-generated-id message as an UPDATE.
        await _ctx.MessageRepo.Received(1).AddAsync(message, Arg.Any<CancellationToken>());
        _backgroundRunner.Received(1).Enqueue(Arg.Is<AICopilotTurnRequest>(r =>
            r.ConversationId == conversation.Id && r.UserId == _ctx.UserId));
        await _ctx.TenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OverageBlocked_FailsWithBudgetErrorCode()
    {
        var conversation = _ctx.SetConversation();
        SetQuota(overageBlocked: true);

        var result = await _sut.Handle(Command(conversation.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.AIBudgetReached, result.ErrorCode);
        Assert.Empty(conversation.Messages);
        Assert.NotEqual(AgentConversationStatus.Running, conversation.Status);
        _backgroundRunner.DidNotReceiveWithAnyArgs().Enqueue(default!);
    }

    [Fact]
    public async Task Handle_OverQuotaWithoutBlock_BillsThroughAndEnqueues()
    {
        var conversation = _ctx.SetConversation();
        SetQuota(overageBlocked: false, isOverQuota: true);

        var result = await _sut.Handle(Command(conversation.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _backgroundRunner.Received(1).Enqueue(Arg.Any<AICopilotTurnRequest>());
    }
}
