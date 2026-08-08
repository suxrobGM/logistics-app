using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.IdentityAccess.Users.Queries;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using MediatR;
using MockQueryable;
using NSubstitute;

namespace Logistics.Application.Tests.TestKit;

/// <summary>
/// Shared substitute rig for AI dispatch/copilot handler tests: the tenant unit of work, current
/// user, and the Agent* repositories pre-wired to it, plus builders for the fixtures nearly every
/// handler needs (a conversation, a suggested decision, the caller's permissions).
/// </summary>
internal sealed class AgentTestContext
{
    public ITenantUnitOfWork TenantUow { get; } = Substitute.For<ITenantUnitOfWork>();
    public ICurrentUserService CurrentUser { get; } = Substitute.For<ICurrentUserService>();
    public IAgentToolExecutor ToolExecutor { get; } = Substitute.For<IAgentToolExecutor>();
    public IAgentToolRegistry ToolRegistry { get; } = Substitute.For<IAgentToolRegistry>();
    public IMediator Mediator { get; } = Substitute.For<IMediator>();

    public ITenantRepository<AgentDecision, Guid> DecisionRepo { get; } =
        Substitute.For<ITenantRepository<AgentDecision, Guid>>();
    public ITenantRepository<AgentConversation, Guid> ConversationRepo { get; } =
        Substitute.For<ITenantRepository<AgentConversation, Guid>>();
    public ITenantRepository<AgentMessage, Guid> MessageRepo { get; } =
        Substitute.For<ITenantRepository<AgentMessage, Guid>>();
    public ITenantRepository<AgentSession, Guid> SessionRepo { get; } =
        Substitute.For<ITenantRepository<AgentSession, Guid>>();

    public Guid UserId { get; } = Guid.NewGuid();

    public Tenant Tenant { get; } = new()
    {
        Id = Guid.NewGuid(),
        Name = "Test Tenant",
        ConnectionString = "test-connection",
        BillingEmail = "test@test.com",
        CompanyAddress = new() { Line1 = "1 Main", City = "Dallas", State = "TX", ZipCode = "75201", Country = "US" }
    };

    public AgentTestContext()
    {
        TenantUow.Repository<AgentDecision>().Returns(DecisionRepo);
        TenantUow.Repository<AgentConversation>().Returns(ConversationRepo);
        TenantUow.Repository<AgentMessage>().Returns(MessageRepo);
        TenantUow.Repository<AgentSession>().Returns(SessionRepo);
        TenantUow.GetCurrentTenant().Returns(Tenant);
        CurrentUser.GetUserId().Returns(UserId);
    }

    public void SetCallerPermissions(params string[] permissions) =>
        Mediator.Send(Arg.Any<GetCurrentUserPermissionsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result<string[]>.Ok(permissions));

    /// <summary>Registers a tool definition the way <c>AgentToolRegistry</c> would - name, required permission, decision type.</summary>
    public void SetToolDefinition(
        string toolName, string requiredPermission, AgentDecisionType decisionType = AgentDecisionType.AssignLoad) =>
        ToolRegistry.TryGetDefinition(toolName).Returns(new AgentToolDefinition(
            toolName, toolName, new System.Text.Json.Nodes.JsonObject())
        {
            RequiredPermission = requiredPermission,
            DecisionType = decisionType
        });

    public AgentConversation SetConversation(
        Guid? id = null, Guid? createdById = null, AgentConversationKind kind = AgentConversationKind.Copilot)
    {
        var conversation = new AgentConversation
        {
            Id = id ?? Guid.NewGuid(),
            CreatedById = createdById ?? UserId,
            Kind = kind
        };
        ConversationRepo.GetByIdAsync(conversation.Id, Arg.Any<CancellationToken>()).Returns(conversation);
        return conversation;
    }

    /// <summary>Wires a Suggested decision the way <c>AICopilotDecisionGuard</c> loads it: by id, with its conversation.</summary>
    public (AgentDecision Decision, AgentConversation Conversation) SetCopilotSuggestedDecision(
        string toolName = "send_invoice",
        string toolInput = """{"invoice_id":"x"}""",
        AgentSessionType sessionType = AgentSessionType.Copilot)
    {
        var conversation = new AgentConversation { CreatedById = UserId };
        var session = new AgentSession
        {
            Type = sessionType,
            ConversationId = sessionType == AgentSessionType.Copilot ? conversation.Id : null
        };
        var decision = new AgentDecision
        {
            SessionId = session.Id,
            Session = session,
            ToolName = toolName,
            ToolInput = toolInput,
            Status = AgentDecisionStatus.Suggested
        };

        DecisionRepo.GetByIdAsync(decision.Id, Arg.Any<CancellationToken>()).Returns(decision);
        ConversationRepo.GetByIdAsync(conversation.Id, Arg.Any<CancellationToken>()).Returns(conversation);
        return (decision, conversation);
    }

    /// <summary>Wires a Suggested decision the way <c>AIDispatchDecisionGuard</c> loads it: via <c>Query().DispatchOnly()</c>.</summary>
    public AgentDecision SetDispatchSuggestedDecision(
        Guid? conversationId = null,
        string toolName = "assign_load_to_truck",
        string toolInput = """{"load_id":"x"}""")
    {
        var session = new AgentSession { Type = AgentSessionType.Dispatch, ConversationId = conversationId };
        var decision = new AgentDecision
        {
            SessionId = session.Id,
            Session = session,
            ToolName = toolName,
            ToolInput = toolInput,
            Status = AgentDecisionStatus.Suggested
        };

        DecisionRepo.Query().Returns(new List<AgentDecision> { decision }.BuildMock());
        return decision;
    }
}
