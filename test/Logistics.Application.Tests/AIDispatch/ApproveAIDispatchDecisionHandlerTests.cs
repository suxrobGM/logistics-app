using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Abstractions.AI;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Integrations.AIDispatch.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using MockQueryable;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.AIDispatch;

public class ApproveAIDispatchDecisionHandlerTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ICurrentUserService currentUser = Substitute.For<ICurrentUserService>();
    private readonly IAgentToolExecutor toolExecutor = Substitute.For<IAgentToolExecutor>();
    private readonly IAIDispatchBroadcastService broadcastService = Substitute.For<IAIDispatchBroadcastService>();
    private readonly ITenantRepository<AgentDecision, Guid> decisionRepo =
        Substitute.For<ITenantRepository<AgentDecision, Guid>>();
    private readonly ITenantRepository<AgentConversation, Guid> conversationRepo =
        Substitute.For<ITenantRepository<AgentConversation, Guid>>();

    private readonly Guid userId = Guid.NewGuid();
    private readonly Tenant tenant;
    private readonly ApproveAIDispatchDecisionHandler sut;

    public ApproveAIDispatchDecisionHandlerTests()
    {
        tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Test Tenant",
            ConnectionString = "test-connection",
            BillingEmail = "test@test.com",
            CompanyAddress = new() { Line1 = "1 Main", City = "Dallas", State = "TX", ZipCode = "75201", Country = "US" }
        };

        tenantUow.Repository<AgentDecision>().Returns(decisionRepo);
        tenantUow.Repository<AgentConversation>().Returns(conversationRepo);
        tenantUow.Repository<AgentMessage>().Returns(Substitute.For<ITenantRepository<AgentMessage, Guid>>());
        tenantUow.GetCurrentTenant().Returns(tenant);
        currentUser.GetUserId().Returns(userId);

        sut = new ApproveAIDispatchDecisionHandler(
            tenantUow, toolExecutor, currentUser, broadcastService,
            Options.Create(new LlmOptions { BypassAIGate = true }));
    }

    private AgentDecision SetSuggestedDecision(Guid? conversationId = null)
    {
        var session = new AgentSession { Type = AgentSessionType.Dispatch, ConversationId = conversationId };
        var decision = new AgentDecision
        {
            SessionId = session.Id,
            Session = session,
            ToolName = "assign_load_to_truck",
            ToolInput = """{"load_id":"x"}""",
            Status = AgentDecisionStatus.Suggested
        };

        decisionRepo.Query().Returns(new List<AgentDecision> { decision }.BuildMock());
        return decision;
    }

    private AgentConversation SetConversation(Guid conversationId)
    {
        var conversation = new AgentConversation { Id = conversationId, Kind = AgentConversationKind.Dispatch };
        conversationRepo.GetByIdAsync(conversationId, Arg.Any<CancellationToken>()).Returns(conversation);
        return conversation;
    }

    /// <summary>Old sessions may predate conversations - approval must still execute without a note.</summary>
    [Fact]
    public async Task Handle_SessionHasNoConversation_ExecutesWithoutAppendingANote()
    {
        var decision = SetSuggestedDecision(conversationId: null);
        toolExecutor.ExecuteToolAsync("assign_load_to_truck", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("""{"success":true}""");

        var result = await sut.Handle(
            new ApproveAIDispatchDecisionCommand { DecisionId = decision.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(AgentDecisionStatus.Executed, decision.Status);
        await broadcastService.DidNotReceiveWithAnyArgs().BroadcastMessageAsync(default, default!);
    }

    [Fact]
    public async Task Handle_SessionHasConversation_AppendsApprovedNoteAndBroadcastsTenantWide()
    {
        var conversationId = Guid.NewGuid();
        var decision = SetSuggestedDecision(conversationId);
        var conversation = SetConversation(conversationId);
        toolExecutor.ExecuteToolAsync("assign_load_to_truck", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("""{"success":true}""");

        var result = await sut.Handle(
            new ApproveAIDispatchDecisionCommand { DecisionId = decision.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var note = Assert.Single(conversation.Messages);
        Assert.Equal(AgentMessageRole.System, note.Role);
        Assert.StartsWith("Approved and executed: assign_load_to_truck", note.DisplayText);

        await broadcastService.Received(1).BroadcastMessageAsync(
            tenant.Id, Arg.Is<AgentMessageDto>(m => m.ConversationId == conversationId));
    }

    [Fact]
    public async Task Handle_ToolThrows_AppendsFailureNoteWhenConversationPresent()
    {
        var conversationId = Guid.NewGuid();
        var decision = SetSuggestedDecision(conversationId);
        var conversation = SetConversation(conversationId);
        toolExecutor.ExecuteToolAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Task<string>>(_ => throw new InvalidOperationException("boom"));

        var result = await sut.Handle(
            new ApproveAIDispatchDecisionCommand { DecisionId = decision.Id }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(AgentDecisionStatus.Failed, decision.Status);
        Assert.Contains("failed to execute", Assert.Single(conversation.Messages).DisplayText);
    }
}
