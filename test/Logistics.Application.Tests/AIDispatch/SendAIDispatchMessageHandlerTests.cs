using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Integrations.AIDispatch.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AIDispatch;

public class SendAIDispatchMessageHandlerTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ICurrentUserService currentUser = Substitute.For<ICurrentUserService>();
    private readonly IAIQuotaService quotaService = Substitute.For<IAIQuotaService>();
    private readonly IBackgroundJobRunner<AIDispatchTurnRequest> backgroundRunner =
        Substitute.For<IBackgroundJobRunner<AIDispatchTurnRequest>>();
    private readonly ITenantRepository<AgentConversation, Guid> conversationRepo =
        Substitute.For<ITenantRepository<AgentConversation, Guid>>();
    private readonly ITenantRepository<AgentMessage, Guid> messageRepo =
        Substitute.For<ITenantRepository<AgentMessage, Guid>>();

    private readonly Guid userId = Guid.NewGuid();

    private readonly Tenant tenant = new()
    {
        Id = Guid.NewGuid(),
        Name = "Test Tenant",
        ConnectionString = "test-connection",
        BillingEmail = "test@test.com",
        CompanyAddress = new() { Line1 = "1 Main", City = "Dallas", State = "TX", ZipCode = "75201", Country = "US" }
    };

    private readonly SendAIDispatchMessageHandler sut;

    public SendAIDispatchMessageHandlerTests()
    {
        tenantUow.Repository<AgentConversation>().Returns(conversationRepo);
        tenantUow.Repository<AgentMessage>().Returns(messageRepo);
        tenantUow.GetCurrentTenant().Returns(tenant);
        currentUser.GetUserId().Returns(userId);
        SetQuota(overageBlocked: false);

        sut = new SendAIDispatchMessageHandler(
            tenantUow, currentUser, quotaService, backgroundRunner,
            NullLogger<SendAIDispatchMessageHandler>.Instance);
    }

    private void SetQuota(bool overageBlocked, bool isOverQuota = false)
    {
        tenant.Settings.BlockAIOverage = overageBlocked;
        quotaService.GetQuotaStatusAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new AIQuotaStatus(5m, isOverQuota || overageBlocked ? 5m : 0m,
                isOverQuota || overageBlocked)
            {
                OverageBlocked = overageBlocked
            });
    }

    private AgentConversation SetConversation(
        Guid? createdById = null, AgentConversationKind kind = AgentConversationKind.Dispatch)
    {
        var conversation = new AgentConversation { CreatedById = createdById ?? userId, Kind = kind };
        conversationRepo.GetByIdAsync(conversation.Id, Arg.Any<CancellationToken>()).Returns(conversation);
        return conversation;
    }

    private SendAIDispatchMessageCommand Command(Guid conversationId, string text = "hello") =>
        new() { ConversationId = conversationId, Text = text };

    /// <summary>A copilot-kind conversation must never accept a dispatch send.</summary>
    [Fact]
    public async Task Handle_CopilotKindConversation_Fails()
    {
        var conversation = SetConversation(kind: AgentConversationKind.Copilot);

        var result = await sut.Handle(Command(conversation.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        backgroundRunner.DidNotReceiveWithAnyArgs().Enqueue(default!);
    }

    /// <summary>Dispatch conversations are tenant-shared: any user may send, not only the creator.</summary>
    [Fact]
    public async Task Handle_ConversationCreatedByAnotherUser_StillSucceeds()
    {
        var conversation = SetConversation(createdById: Guid.NewGuid());

        var result = await sut.Handle(Command(conversation.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        backgroundRunner.Received(1).Enqueue(Arg.Any<AIDispatchTurnRequest>());
    }

    [Fact]
    public async Task Handle_TurnAlreadyRunning_Fails()
    {
        var conversation = SetConversation();
        conversation.BeginTurn();

        var result = await sut.Handle(Command(conversation.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("in progress", result.Error);
    }

    [Fact]
    public async Task Handle_HappyPath_AppendsMessageBeginsTurnAndEnqueues()
    {
        var conversation = SetConversation();

        var result = await sut.Handle(Command(conversation.Id, "assign what you can"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var message = Assert.Single(conversation.Messages);
        Assert.Equal("assign what you can", message.DisplayText);
        Assert.Equal(AgentMessageRole.User, message.Role);
        Assert.Equal(1, message.Sequence);
        Assert.Equal(AgentConversationStatus.Running, conversation.Status);
        Assert.Equal(message.Id, result.Value!.UserMessageId);

        // Load-bearing: without the explicit Add, EF saves the pre-generated-id message as an UPDATE.
        await messageRepo.Received(1).AddAsync(message, Arg.Any<CancellationToken>());
        backgroundRunner.Received(1).Enqueue(Arg.Is<AIDispatchTurnRequest>(r =>
            r.ConversationId == conversation.Id && r.TriggeredByUserId == userId));
        await tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OverageBlocked_FailsWithBudgetErrorCode()
    {
        var conversation = SetConversation();
        SetQuota(overageBlocked: true);

        var result = await sut.Handle(Command(conversation.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.AIBudgetReached, result.ErrorCode);
        Assert.Empty(conversation.Messages);
        Assert.NotEqual(AgentConversationStatus.Running, conversation.Status);
        backgroundRunner.DidNotReceiveWithAnyArgs().Enqueue(default!);
    }

    [Fact]
    public async Task Handle_OverQuotaWithoutBlock_BillsThroughAndEnqueues()
    {
        var conversation = SetConversation();
        SetQuota(overageBlocked: false, isOverQuota: true);

        var result = await sut.Handle(Command(conversation.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        backgroundRunner.Received(1).Enqueue(Arg.Any<AIDispatchTurnRequest>());
    }
}
