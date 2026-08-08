using Logistics.Application.Abstractions.Agents;
using System.Text.Json.Nodes;
using Logistics.Application.Abstractions.AI;
using Logistics.Application.Abstractions.AICopilot;
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
    private readonly IAgentToolExecutor toolExecutor = Substitute.For<IAgentToolExecutor>();
    private readonly IAgentToolRegistry toolRegistry = Substitute.For<IAgentToolRegistry>();
    private readonly IAICopilotBroadcastService broadcastService = Substitute.For<IAICopilotBroadcastService>();
    private readonly IMediator mediator = Substitute.For<IMediator>();
    private readonly ITenantRepository<AgentDecision, Guid> decisionRepo =
        Substitute.For<ITenantRepository<AgentDecision, Guid>>();
    private readonly ITenantRepository<AgentConversation, Guid> conversationRepo =
        Substitute.For<ITenantRepository<AgentConversation, Guid>>();
    private readonly ITenantRepository<AgentMessage, Guid> messageRepo =
        Substitute.For<ITenantRepository<AgentMessage, Guid>>();

    private readonly Guid userId = Guid.NewGuid();
    private readonly ApproveAICopilotDecisionHandler sut;

    public ApproveAICopilotDecisionHandlerTests()
    {
        tenantUow.Repository<AgentDecision>().Returns(decisionRepo);
        tenantUow.Repository<AgentConversation>().Returns(conversationRepo);
        tenantUow.Repository<AgentMessage>().Returns(messageRepo);
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
        toolRegistry.TryGetDefinition("send_invoice").Returns(new AgentToolDefinition(
            "send_invoice", "Send an invoice", new JsonObject())
        {
            RequiredPermission = "Permission.Invoice.Manage",
            DecisionType = AgentDecisionType.SendInvoice
        });

        sut = new ApproveAICopilotDecisionHandler(
            tenantUow, currentUser, toolExecutor, toolRegistry, broadcastService, mediator,
            Options.Create(new LlmOptions { BypassAIGate = true }));
    }

    private void SetCallerPermissions(params string[] permissions)
    {
        mediator.Send(Arg.Any<GetCurrentUserPermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<string[]>.Ok(permissions));
    }

    private (AgentDecision Decision, AgentConversation Conversation) SetSuggestedDecision(
        AgentSessionType sessionType = AgentSessionType.Copilot)
    {
        var conversation = new AgentConversation { CreatedById = userId };
        var session = new AgentSession
        {
            Type = sessionType,
            ConversationId = sessionType == AgentSessionType.Copilot ? conversation.Id : null
        };
        var decision = new AgentDecision
        {
            SessionId = session.Id,
            Session = session,
            ToolName = "send_invoice",
            ToolInput = """{"invoice_id":"x"}""",
            Status = AgentDecisionStatus.Suggested
        };

        decisionRepo.GetByIdAsync(decision.Id, Arg.Any<CancellationToken>()).Returns(decision);
        conversationRepo.GetByIdAsync(conversation.Id, Arg.Any<CancellationToken>()).Returns(conversation);
        return (decision, conversation);
    }

    [Fact]
    public async Task Handle_DispatchDecision_IsNotApprovableViaCopilot()
    {
        var (decision, _) = SetSuggestedDecision(AgentSessionType.Dispatch);

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
        Assert.Equal(AgentDecisionStatus.Suggested, decision.Status);
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
        Assert.Equal(AgentDecisionStatus.Executed, decision.Status);
        Assert.Equal(userId, decision.ApprovedByUserId);

        var note = Assert.Single(conversation.Messages);
        Assert.Equal(AgentMessageRole.System, note.Role);
        Assert.StartsWith("Approved and executed: send_invoice", note.DisplayText);

        // Adding to the navigation alone saves as an UPDATE affecting 0 rows - see ef-persistence.md.
        await messageRepo.Received(1).AddAsync(note, Arg.Any<CancellationToken>());

        await broadcastService.Received(1).BroadcastMessageAsync(
            Arg.Any<Guid>(), conversation.CreatedById, Arg.Any<AgentMessageDto>());
        await broadcastService.Received(1).BroadcastDecisionAsync(
            Arg.Any<Guid>(), conversation.CreatedById, Arg.Any<AgentDecisionDto>());
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
        Assert.Equal(AgentDecisionStatus.Failed, decision.Status);
        Assert.Contains("failed to execute", Assert.Single(conversation.Messages).DisplayText);
    }
}
