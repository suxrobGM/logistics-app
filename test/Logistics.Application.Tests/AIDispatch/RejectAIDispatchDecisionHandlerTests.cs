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

public class RejectAIDispatchDecisionHandlerTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ICurrentUserService currentUser = Substitute.For<ICurrentUserService>();
    private readonly IAIDispatchBroadcastService broadcastService = Substitute.For<IAIDispatchBroadcastService>();
    private readonly ITenantRepository<AgentDecision, Guid> decisionRepo =
        Substitute.For<ITenantRepository<AgentDecision, Guid>>();
    private readonly ITenantRepository<AgentConversation, Guid> conversationRepo =
        Substitute.For<ITenantRepository<AgentConversation, Guid>>();

    private readonly Guid userId = Guid.NewGuid();
    private readonly Tenant tenant;
    private readonly RejectAIDispatchDecisionHandler sut;

    public RejectAIDispatchDecisionHandlerTests()
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

        sut = new RejectAIDispatchDecisionHandler(
            tenantUow, currentUser, broadcastService,
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

    [Fact]
    public async Task Handle_SessionHasNoConversation_RejectsWithoutAppendingANote()
    {
        var decision = SetSuggestedDecision(conversationId: null);

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
        var decision = SetSuggestedDecision(conversationId);
        var conversation = SetConversation(conversationId);

        var result = await sut.Handle(
            new RejectAIDispatchDecisionCommand { DecisionId = decision.Id, Reason = "wrong truck" },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var note = Assert.Single(conversation.Messages);
        Assert.Equal(AgentMessageRole.System, note.Role);
        Assert.Equal("Rejected: assign_load_to_truck - wrong truck", note.DisplayText);

        await broadcastService.Received(1).BroadcastMessageAsync(
            tenant.Id, Arg.Is<AgentMessageDto>(m => m.ConversationId == conversationId));
    }

    [Fact]
    public async Task Handle_NoReasonGiven_NoteOmitsTheDash()
    {
        var conversationId = Guid.NewGuid();
        var decision = SetSuggestedDecision(conversationId);
        var conversation = SetConversation(conversationId);

        await sut.Handle(
            new RejectAIDispatchDecisionCommand { DecisionId = decision.Id }, CancellationToken.None);

        Assert.Equal("Rejected: assign_load_to_truck", Assert.Single(conversation.Messages).DisplayText);
    }
}
