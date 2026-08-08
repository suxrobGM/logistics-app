using Logistics.Application.Abstractions.AICopilot;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Integrations.AICopilot.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AICopilot;

public class SendAICopilotMessageHandlerTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ICurrentUserService currentUser = Substitute.For<ICurrentUserService>();
    private readonly IAIQuotaService quotaService = Substitute.For<IAIQuotaService>();
    private readonly IBackgroundJobRunner<AICopilotTurnRequest> backgroundRunner =
        Substitute.For<IBackgroundJobRunner<AICopilotTurnRequest>>();
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

    private readonly SendAICopilotMessageHandler sut;

    public SendAICopilotMessageHandlerTests()
    {
        tenantUow.Repository<AgentConversation>().Returns(conversationRepo);
        tenantUow.Repository<AgentMessage>().Returns(messageRepo);
        tenantUow.GetCurrentTenant().Returns(tenant);
        currentUser.GetUserId().Returns(userId);
        SetQuota(overageBlocked: false);

        sut = new SendAICopilotMessageHandler(
            tenantUow, currentUser, quotaService, backgroundRunner,
            NullLogger<SendAICopilotMessageHandler>.Instance);
    }

    /// <summary>
    /// Mirrors the real invariant: <c>AIQuotaService</c> only ever reports <c>OverageBlocked</c>
    /// for a tenant that opted in, and the handler skips the quota lookup when the flag is off.
    /// </summary>
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
        Guid? ownerId = null, AgentConversationKind kind = AgentConversationKind.Copilot)
    {
        var conversation = new AgentConversation { CreatedById = ownerId ?? userId, Kind = kind };
        conversationRepo.GetByIdAsync(conversation.Id, Arg.Any<CancellationToken>()).Returns(conversation);
        return conversation;
    }

    private SendAICopilotMessageCommand Command(Guid conversationId, string text = "hello") =>
        new() { ConversationId = conversationId, Text = text };

    [Fact]
    public async Task Handle_NotOwner_Fails()
    {
        var conversation = SetConversation(ownerId: Guid.NewGuid());

        var result = await sut.Handle(Command(conversation.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        backgroundRunner.DidNotReceiveWithAnyArgs().Enqueue(default!);
    }

    /// <summary>A dispatch-kind conversation must never accept a copilot send.</summary>
    [Fact]
    public async Task Handle_DispatchKindConversation_Fails()
    {
        var conversation = SetConversation(kind: AgentConversationKind.Dispatch);

        var result = await sut.Handle(Command(conversation.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        backgroundRunner.DidNotReceiveWithAnyArgs().Enqueue(default!);
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

        var result = await sut.Handle(Command(conversation.Id, "invoice load 42"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var message = Assert.Single(conversation.Messages);
        Assert.Equal("invoice load 42", message.DisplayText);
        Assert.Equal(AgentMessageRole.User, message.Role);
        Assert.Equal(1, message.Sequence);
        Assert.Equal(AICopilotConversationStatus.Running, conversation.Status);
        Assert.Equal(message.Id, result.Value!.UserMessageId);

        // Load-bearing: without the explicit Add, EF saves the pre-generated-id message as an UPDATE.
        await messageRepo.Received(1).AddAsync(message, Arg.Any<CancellationToken>());
        backgroundRunner.Received(1).Enqueue(Arg.Is<AICopilotTurnRequest>(r =>
            r.ConversationId == conversation.Id && r.UserId == userId));
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
        Assert.NotEqual(AICopilotConversationStatus.Running, conversation.Status);
        backgroundRunner.DidNotReceiveWithAnyArgs().Enqueue(default!);
    }

    [Fact]
    public async Task Handle_OverQuotaWithoutBlock_BillsThroughAndEnqueues()
    {
        var conversation = SetConversation();
        SetQuota(overageBlocked: false, isOverQuota: true);

        var result = await sut.Handle(Command(conversation.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        backgroundRunner.Received(1).Enqueue(Arg.Any<AICopilotTurnRequest>());
    }
}
