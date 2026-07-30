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
    private readonly ITenantRepository<AICopilotConversation, Guid> conversationRepo =
        Substitute.For<ITenantRepository<AICopilotConversation, Guid>>();
    private readonly ITenantRepository<AICopilotMessage, Guid> messageRepo =
        Substitute.For<ITenantRepository<AICopilotMessage, Guid>>();

    private readonly Guid userId = Guid.NewGuid();
    private readonly SendAICopilotMessageHandler sut;

    public SendAICopilotMessageHandlerTests()
    {
        tenantUow.Repository<AICopilotConversation>().Returns(conversationRepo);
        tenantUow.Repository<AICopilotMessage>().Returns(messageRepo);
        tenantUow.GetCurrentTenant().Returns(new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Test Tenant",
            ConnectionString = "test-connection",
            BillingEmail = "test@test.com",
            CompanyAddress = new() { Line1 = "1 Main", City = "Dallas", State = "TX", ZipCode = "75201", Country = "US" }
        });
        currentUser.GetUserId().Returns(userId);
        SetQuota(isOverQuota: false);

        sut = new SendAICopilotMessageHandler(
            tenantUow, currentUser, quotaService, backgroundRunner,
            NullLogger<SendAICopilotMessageHandler>.Instance);
    }

    private void SetQuota(bool isOverQuota)
    {
        quotaService.GetQuotaStatusAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new AIQuotaStatus(500, isOverQuota ? 500 : 10, isOverQuota ? 0 : 490,
                isOverQuota, "Starter", DateTime.UtcNow.AddDays(3)));
    }

    private AICopilotConversation SetConversation(Guid? ownerId = null)
    {
        var conversation = new AICopilotConversation { CreatedById = ownerId ?? userId };
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

    [Fact]
    public async Task Handle_QuotaExhausted_FailsWithQuotaErrorCode()
    {
        var conversation = SetConversation();
        SetQuota(isOverQuota: true);

        var result = await sut.Handle(Command(conversation.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.AIQuotaExceeded, result.ErrorCode);
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
        Assert.Equal(AICopilotMessageRole.User, message.Role);
        Assert.Equal(1, message.Sequence);
        Assert.Equal(AICopilotConversationStatus.Running, conversation.Status);
        Assert.Equal(message.Id, result.Value!.UserMessageId);

        // Load-bearing: without the explicit Add, EF saves the pre-generated-id message as an UPDATE.
        await messageRepo.Received(1).AddAsync(message, Arg.Any<CancellationToken>());
        backgroundRunner.Received(1).Enqueue(Arg.Is<AICopilotTurnRequest>(r =>
            r.ConversationId == conversation.Id && r.UserId == userId));
        await tenantUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
