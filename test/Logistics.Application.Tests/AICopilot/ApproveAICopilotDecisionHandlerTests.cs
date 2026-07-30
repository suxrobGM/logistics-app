using System.Text.Json.Nodes;
using Logistics.Application.Abstractions.AI;
using Logistics.Application.Abstractions.AICopilot;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.IdentityAccess.Users.Queries;
using Logistics.Application.Modules.Integrations.AICopilot.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AICopilot;

public class ApproveAICopilotDecisionHandlerTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ICurrentUserService currentUser = Substitute.For<ICurrentUserService>();
    private readonly IAIDispatchToolExecutor toolExecutor = Substitute.For<IAIDispatchToolExecutor>();
    private readonly IAgentToolRegistry toolRegistry = Substitute.For<IAgentToolRegistry>();
    private readonly IAICopilotBroadcastService broadcastService = Substitute.For<IAICopilotBroadcastService>();
    private readonly IMediator mediator = Substitute.For<IMediator>();
    private readonly ITenantRepository<AIDispatchDecision, Guid> decisionRepo =
        Substitute.For<ITenantRepository<AIDispatchDecision, Guid>>();
    private readonly ITenantRepository<AICopilotConversation, Guid> conversationRepo =
        Substitute.For<ITenantRepository<AICopilotConversation, Guid>>();

    private readonly Guid userId = Guid.NewGuid();
    private readonly ApproveAICopilotDecisionHandler sut;

    public ApproveAICopilotDecisionHandlerTests()
    {
        tenantUow.Repository<AIDispatchDecision>().Returns(decisionRepo);
        tenantUow.Repository<AICopilotConversation>().Returns(conversationRepo);
        tenantUow.GetCurrentTenant().Returns(new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Test Tenant",
            ConnectionString = "test-connection",
            BillingEmail = "test@test.com",
            CompanyAddress = new() { Line1 = "1 Main", City = "Dallas", State = "TX", ZipCode = "75201", Country = "US" }
        });
        currentUser.GetUserId().Returns(userId);
        SetCallerPermissions("Permission.Invoice.Manage");
        toolRegistry.TryGetDefinition("send_invoice").Returns(new AIDispatchToolDefinition(
            "send_invoice", "Send an invoice", new JsonObject(),
            IsWrite: true, RequiredPermission: "Permission.Invoice.Manage"));

        sut = new ApproveAICopilotDecisionHandler(
            tenantUow, currentUser, toolExecutor, toolRegistry, broadcastService, mediator,
            Options.Create(new LlmOptions { BypassLlmGate = true }));
    }

    private void SetCallerPermissions(params string[] permissions)
    {
        mediator.Send(Arg.Any<GetCurrentUserPermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<string[]>.Ok(permissions));
    }

    private (AIDispatchDecision Decision, AICopilotConversation Conversation) SetSuggestedDecision(
        AIDispatchSessionType sessionType = AIDispatchSessionType.Copilot)
    {
        var conversation = new AICopilotConversation { CreatedById = userId };
        var session = new AIDispatchSession
        {
            Type = sessionType,
            ConversationId = sessionType == AIDispatchSessionType.Copilot ? conversation.Id : null,
            Mode = AIDispatchMode.HumanInTheLoop
        };
        var decision = new AIDispatchDecision
        {
            SessionId = session.Id,
            Session = session,
            ToolName = "send_invoice",
            ToolInput = """{"invoice_id":"x"}""",
            Status = AIDispatchDecisionStatus.Suggested
        };

        decisionRepo.GetByIdAsync(decision.Id, Arg.Any<CancellationToken>()).Returns(decision);
        conversationRepo.GetByIdAsync(conversation.Id, Arg.Any<CancellationToken>()).Returns(conversation);
        return (decision, conversation);
    }

    [Fact]
    public async Task Handle_DispatchDecision_IsNotApprovableViaCopilot()
    {
        var (decision, _) = SetSuggestedDecision(AIDispatchSessionType.Dispatch);

        var result = await sut.Handle(
            new ApproveAICopilotDecisionCommand { DecisionId = decision.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await toolExecutor.DidNotReceiveWithAnyArgs().ExecuteToolAsync(default!, default!, default);
    }

    [Fact]
    public async Task Handle_ApproverLacksToolPermission_FailsWithoutExecuting()
    {
        var (decision, _) = SetSuggestedDecision();
        SetCallerPermissions("Permission.Copilot.Manage");

        var result = await sut.Handle(
            new ApproveAICopilotDecisionCommand { DecisionId = decision.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Permission.Invoice.Manage", result.Error);
        Assert.Equal(AIDispatchDecisionStatus.Suggested, decision.Status);
        await toolExecutor.DidNotReceiveWithAnyArgs().ExecuteToolAsync(default!, default!, default);
    }

    [Fact]
    public async Task Handle_HappyPath_ExecutesAppendsOutcomeNoteAndBroadcasts()
    {
        var (decision, conversation) = SetSuggestedDecision();
        toolExecutor.ExecuteToolAsync("send_invoice", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("""{"success":true}""");

        var result = await sut.Handle(
            new ApproveAICopilotDecisionCommand { DecisionId = decision.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(AIDispatchDecisionStatus.Executed, decision.Status);
        Assert.Equal(userId, decision.ApprovedByUserId);

        var note = Assert.Single(conversation.Messages);
        Assert.Equal(AICopilotMessageRole.System, note.Role);
        Assert.StartsWith("Approved and executed: send_invoice", note.DisplayText);

        await broadcastService.Received(1).BroadcastMessageAsync(
            Arg.Any<Guid>(), conversation.CreatedById, Arg.Any<AICopilotMessageDto>());
        await broadcastService.Received(1).BroadcastDecisionAsync(
            Arg.Any<Guid>(), conversation.CreatedById, Arg.Any<AIDispatchDecisionDto>());
    }

    [Fact]
    public async Task Handle_ToolThrows_MarksFailedAndStillNotesOutcome()
    {
        var (decision, conversation) = SetSuggestedDecision();
        toolExecutor.ExecuteToolAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Task<string>>(_ => throw new InvalidOperationException("boom"));

        var result = await sut.Handle(
            new ApproveAICopilotDecisionCommand { DecisionId = decision.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(AIDispatchDecisionStatus.Failed, decision.Status);
        Assert.Contains("failed to execute", Assert.Single(conversation.Messages).DisplayText);
    }
}
