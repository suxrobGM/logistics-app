using Logistics.Application.Abstractions.AI;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.IdentityAccess.Users.Services;
using Logistics.Application.Modules.Integrations.AICopilot.Services;
using Logistics.Application.Modules.Integrations.AIDispatch.Services;
using Logistics.Application.Modules.Integrations.Agents.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MockQueryable;
using NSubstitute;

namespace Logistics.Application.Tests.TestKit;

/// <summary>
/// Shared substitute rig for the AI dispatch/copilot handler tests: the unit of work, current user
/// and Agent* repositories, the real shared services built over them, and fixture builders.
/// </summary>
internal sealed class AgentTestContext
{
    public ITenantUnitOfWork TenantUow { get; } = Substitute.For<ITenantUnitOfWork>();
    public ICurrentUserService CurrentUser { get; } = Substitute.For<ICurrentUserService>();
    public IAgentToolExecutor ToolExecutor { get; } = Substitute.For<IAgentToolExecutor>();
    public IAgentToolRegistry ToolRegistry { get; } = Substitute.For<IAgentToolRegistry>();
    public IAgentRunContext RunContext { get; } = Substitute.For<IAgentRunContext>();
    public IMediator Mediator { get; } = Substitute.For<IMediator>();
    public IUserPermissionService UserPermissions { get; } = Substitute.For<IUserPermissionService>();
    public IAIQuotaService QuotaService { get; } = Substitute.For<IAIQuotaService>();
    public IAIDispatchService DispatchService { get; } = Substitute.For<IAIDispatchService>();

    public ITenantRepository<AgentDecision, Guid> DecisionRepo { get; } =
        Substitute.For<ITenantRepository<AgentDecision, Guid>>();
    public ITenantRepository<AgentConversation, Guid> ConversationRepo { get; } =
        Substitute.For<ITenantRepository<AgentConversation, Guid>>();
    public ITenantRepository<AgentMessage, Guid> MessageRepo { get; } =
        Substitute.For<ITenantRepository<AgentMessage, Guid>>();
    public ITenantRepository<AgentSession, Guid> SessionRepo { get; } =
        Substitute.For<ITenantRepository<AgentSession, Guid>>();

    public IAgentConversationAccess Access { get; }
    public IAgentConversationCommands Commands { get; }
    public IAgentConversationQueries Queries { get; }
    public IAgentDecisionNotes Notes { get; }
    public IAgentDecisionExecution Execution { get; }
    public IAgentDecisionAuthorization Authorization { get; }
    public IAICopilotDecisionGuard CopilotGuard { get; }

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
        SessionRepo.Query().Returns(_ => new List<AgentSession>().BuildMock());

        Access = new AgentConversationAccess(TenantUow);
        Commands = new AgentConversationCommands(
            TenantUow, Access, QuotaService, DispatchService,
            NullLogger<AgentConversationCommands>.Instance);
        Queries = new AgentConversationQueries(TenantUow, Access);
        Notes = new AgentDecisionNotes(TenantUow);
        Execution = new AgentDecisionExecution(ToolExecutor);
        Authorization = new AgentDecisionAuthorization(ToolRegistry, UserPermissions);
        CopilotGuard = new AICopilotDecisionGuard(TenantUow, Access);
    }

    /// <summary><paramref name="bypassAIGate"/> is the knob the AI-disabled tests turn.</summary>
    public IAIDispatchDecisionGuard DispatchGuard(bool bypassAIGate = true) =>
        new AIDispatchDecisionGuard(TenantUow, Options.Create(new LlmOptions { BypassAIGate = bypassAIGate }));

    public void SetCallerPermissions(params string[] permissions)
    {
        var granted = permissions.ToHashSet();
        UserPermissions.GetPermissionsAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(granted);
        UserPermissions.HasPermissionAsync(
                Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(call => granted.Contains(call.ArgAt<string>(2)));
    }

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
        var decision = SuggestedDecision(conversation, sessionType, toolName, toolInput);

        DecisionRepo.GetByIdAsync(decision.Id, Arg.Any<CancellationToken>()).Returns(decision);
        ConversationRepo.GetByIdAsync(conversation.Id, Arg.Any<CancellationToken>()).Returns(conversation);
        return (decision, conversation);
    }

    /// <summary>Wires a Suggested decision the way <c>AIDispatchDecisionGuard</c> loads it: via <c>Query().DispatchOnly()</c>.</summary>
    public AgentDecision SetDispatchSuggestedDecision(
        AgentConversation? conversation = null,
        string toolName = "assign_load_to_truck",
        string toolInput = """{"load_id":"x"}""")
    {
        conversation ??= new AgentConversation { CreatedById = UserId, Kind = AgentConversationKind.Dispatch };
        var decision = SuggestedDecision(conversation, AgentSessionType.Dispatch, toolName, toolInput);

        DecisionRepo.Query().Returns(_ => new List<AgentDecision> { decision }.BuildMock());
        return decision;
    }

    /// <summary>
    /// Linked both ways: the notes service reaches the conversation by projecting off the session
    /// query, not through the decision's navigation.
    /// </summary>
    private AgentDecision SuggestedDecision(
        AgentConversation conversation, AgentSessionType sessionType, string toolName, string toolInput)
    {
        var session = new AgentSession
        {
            Type = sessionType,
            ConversationId = conversation.Id,
            Conversation = conversation
        };
        SessionRepo.Query().Returns(_ => new List<AgentSession> { session }.BuildMock());

        return new AgentDecision
        {
            SessionId = session.Id,
            Session = session,
            ToolName = toolName,
            ToolInput = toolInput,
            Status = AgentDecisionStatus.Suggested
        };
    }
}
