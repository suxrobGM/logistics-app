using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Modules.Integrations.Agents.Services;
using Logistics.Infrastructure.AI.Agents;
using Logistics.Infrastructure.AI.Agents.Dispatch;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Application.Abstractions.AI;
using Logistics.Infrastructure.AI.Llm;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using Logistics.Application.Abstractions.Features;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.Payments.Stripe;
using Logistics.Application.Abstractions.SystemSettings;
using MsOptions = Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.AI.Tests.Agents.Dispatch;

/// <summary>
/// Covers <see cref="AIDispatchService"/> and <see cref="DispatchAgentSurface"/> together - the
/// adapter only has meaning wired to its surface. Prompt content (snapshot, replay, catalogue)
/// belongs to <see cref="AIDispatchConversationBuilderTests"/>; this file is the turn lifecycle.
/// </summary>
public class AIDispatchServiceTests
{
    private readonly ITenantRepository<AgentSession, Guid> _sessionRepo =
        Substitute.For<ITenantRepository<AgentSession, Guid>>();
    private readonly ITenantRepository<AgentConversation, Guid> _conversationRepo =
        Substitute.For<ITenantRepository<AgentConversation, Guid>>();
    private readonly ITenantRepository<AgentMessage, Guid> _messageRepo =
        Substitute.For<ITenantRepository<AgentMessage, Guid>>();

    private readonly IStripeUsageService _stripeUsageService = Substitute.For<IStripeUsageService>();
    private readonly IAIQuotaService _quotaService = Substitute.For<IAIQuotaService>();
    private readonly IAIDispatchBroadcastService _broadcastService = Substitute.For<IAIDispatchBroadcastService>();

    private readonly AIDispatchService _sut;
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly Tenant _tenant;

    public AIDispatchServiceTests()
    {
        _tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            DatabaseName = "test",
            BillingEmail = "test@test.com",
            CompanyAddress = new() { Line1 = "123 Test St", City = "Test", State = "TX", ZipCode = "12345", Country = "US" }
        };

        _tenantUow.Repository<AgentSession>().Returns(_sessionRepo);
        _tenantUow.Repository<AgentConversation>().Returns(_conversationRepo);
        _tenantUow.Repository<AgentMessage>().Returns(_messageRepo);
        _messageRepo.Query().Returns(_ => new List<AgentMessage>().BuildMock());
        _tenantUow.GetCurrentTenant().Returns(_tenant);
        _tenantUow.SqlQueryAsync<int>(Arg.Any<FormattableString>(), Arg.Any<CancellationToken>()).Returns([1]);

        var toolRegistry = Substitute.For<IAgentToolRegistry>();
        toolRegistry.GetDispatchAgentTools(Arg.Any<IReadOnlySet<TenantFeature>>()).Returns([]);

        var featureService = Substitute.For<IFeatureService>();

        var llmOptions = MsOptions.Options.Create(new LlmOptions
        {
            MaxTokens = 100,
            Providers = new Dictionary<LlmProvider, LlmProviderOptions>
            {
                [LlmProvider.Anthropic] = new() { ApiKey = "sk-ant-test-key", Model = "claude-haiku-5-5" }
            }
        });

        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient(Arg.Any<string>()).Returns(_ => new HttpClient());
        var providerFactory = new LlmProviderFactory(llmOptions, httpClientFactory);

        var systemSettings = Substitute.For<ISystemSettingsService>();
        var modelResolver = new LlmModelResolver(systemSettings, NullLogger<LlmModelResolver>.Instance);
        var sessionSetup = new LlmSessionSetup(
            featureService, providerFactory, modelResolver, systemSettings, _tenantUow);

        var policyRepo = Substitute.For<ITenantRepository<AIDispatchPolicy, Guid>>();
        policyRepo.Query().Returns(new List<AIDispatchPolicy>().BuildMock());
        _tenantUow.Repository<AIDispatchPolicy>().Returns(policyRepo);

        var conversationBuilder = new AIDispatchConversationBuilder(
            toolRegistry, sessionSetup, _tenantUow, new AgentTranscriptLoader(_tenantUow),
            NullLogger<AIDispatchConversationBuilder>.Instance);

        var toolExecutor = Substitute.For<IAgentToolExecutor>();
        var decisionProcessor = new AgentDecisionProcessor(
            toolExecutor, toolRegistry, _tenantUow, _broadcastService, new AgentDecisionDtoMapper(_tenantUow),
            NullLogger<AgentDecisionProcessor>.Instance);
        var loopRunner = new AgentLoopRunner(decisionProcessor, _tenantUow, NullLogger<AgentLoopRunner>.Instance);

        var cancellationRegistry = new AgentSessionCancellationRegistry();

        SetQuotaStatus(isOverQuota: false);

        var overageReporter = new AgentOverageReporter(
            _stripeUsageService, NullLogger<AgentOverageReporter>.Instance);

        var surface = new DispatchAgentSurface(conversationBuilder, _broadcastService);
        var turnService = new AgentTurnService(
            llmOptions, loopRunner, cancellationRegistry, _tenantUow, _quotaService, overageReporter,
            new AgentRunContext(), NullLogger<AgentTurnService>.Instance);

        _sut = new AIDispatchService(turnService, surface, cancellationRegistry, _tenantUow);
    }

    private void SetQuotaStatus(bool isOverQuota, bool overageBlocked = false, bool overageBillable = true)
    {
        _quotaService.GetQuotaStatusAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new AIQuotaStatus(5m, isOverQuota ? 5m : 0m, isOverQuota)
            {
                OverageBlocked = overageBlocked,
                OverageBillable = overageBillable
            });
    }

    private AgentConversation SetConversation()
    {
        var conversation = new AgentConversation { Kind = AgentConversationKind.Dispatch };
        _conversationRepo.GetByIdAsync(conversation.Id, Arg.Any<CancellationToken>()).Returns(conversation);
        return conversation;
    }

    private AIDispatchTurnRequest CreateRequest(AgentConversation conversation) =>
        new(_tenant.Id, conversation.Id, null);

    private List<AgentMessage> AddedMessages() =>
    [
        .. _messageRepo.ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == nameof(_messageRepo.AddAsync))
            .Select(c => (AgentMessage)c.GetArguments()[0]!)
    ];

    #region Turn adapter wiring (no network - the LLM-disabled early exit)

    /// <summary>
    /// Proves the adapter threads TenantId/ConversationId through to AgentTurnService and that
    /// DispatchAgentSurface broadcasts tenant-wide, without needing a real LLM call: the
    /// AI-disabled path short-circuits before the agent loop runs.
    /// </summary>
    [Fact]
    public async Task RunTurnAsync_AIDisabled_AppendsNoticeToTheRightConversationAndBroadcastsTenantWide()
    {
        _tenant.Settings.AIEnabled = false;
        var conversation = SetConversation();

        await _sut.RunTurnAsync(CreateRequest(conversation));

        var note = Assert.Single(AddedMessages());
        Assert.Equal(AgentMessageRole.System, note.Role);
        Assert.Contains("AI is disabled", note.DisplayText);

        await _broadcastService.Received(1).BroadcastMessageAsync(
            _tenant.Id, Arg.Is<AgentMessageDto>(m => m.ConversationId == conversation.Id));
        await _sessionRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task RunTurnAsync_OverageBlocked_AppendsBudgetNoticeWithoutCreatingASession()
    {
        SetQuotaStatus(isOverQuota: true, overageBlocked: true);
        var conversation = SetConversation();

        await _sut.RunTurnAsync(CreateRequest(conversation));

        var note = Assert.Single(AddedMessages());
        Assert.Contains("budget", note.DisplayText, StringComparison.OrdinalIgnoreCase);
        await _sessionRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task RunTurnAsync_ConversationNotFound_DoesNothing()
    {
        var request = new AIDispatchTurnRequest(_tenant.Id, Guid.NewGuid(), null);

        await _sut.RunTurnAsync(request);

        await _sessionRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await _broadcastService.DidNotReceiveWithAnyArgs().BroadcastMessageAsync(default, default!);
    }

    #endregion

    #region IsOverage flag on session

    [Fact]
    public async Task RunTurnAsync_SetsIsOverageTrue_WhenTenantIsOverQuota()
    {
        SetQuotaStatus(isOverQuota: true);
        var conversation = SetConversation();
        AgentSession? capturedSession = null;
        _sessionRepo.AddAsync(Arg.Do<AgentSession>(s => capturedSession = s), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // The agent loop will fail because we don't have a real LLM API, but the session should
        // still be created with IsOverage set - AgentTurnService never rethrows, so no try/catch
        // is strictly required, kept only for defense against an unexpected synchronous throw.
        try
        {
            await _sut.RunTurnAsync(CreateRequest(conversation));
        }
        catch
        {
            // Expected - no real API
        }

        Assert.NotNull(capturedSession);
        Assert.Equal(AgentSessionType.Dispatch, capturedSession!.Type);
        Assert.Equal(conversation.Id, capturedSession.ConversationId);
        Assert.True(capturedSession.IsOverage);
    }

    [Fact]
    public async Task RunTurnAsync_SetsIsOverageFalse_WhenTenantIsUnderQuota()
    {
        var conversation = SetConversation();
        AgentSession? capturedSession = null;
        _sessionRepo.AddAsync(Arg.Do<AgentSession>(s => capturedSession = s), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        try
        {
            await _sut.RunTurnAsync(CreateRequest(conversation));
        }
        catch
        {
            // Expected
        }

        Assert.NotNull(capturedSession);
        Assert.False(capturedSession!.IsOverage);
    }

    #endregion

    #region Overage reporting

    [Fact]
    public async Task RunTurnAsync_DoesNotReportOverage_WhenNotOverage()
    {
        var conversation = SetConversation();

        try
        {
            await _sut.RunTurnAsync(CreateRequest(conversation));
        }
        catch
        {
            // Expected
        }

        await _stripeUsageService.DidNotReceive()
            .ReportAISessionOverageAsync(Arg.Any<Guid>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReportOverageIfNeeded_DoesNotThrow_WhenStripeServiceFails()
    {
        _stripeUsageService.ReportAISessionOverageAsync(Arg.Any<Guid>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Stripe API error"));

        SetQuotaStatus(isOverQuota: true);
        var conversation = SetConversation();

        // Should not throw even if Stripe fails
        try
        {
            await _sut.RunTurnAsync(CreateRequest(conversation));
        }
        catch (Exception ex) when (ex.Message != "Stripe API error")
        {
            // Expected - API error from LLM, not from Stripe
        }
    }

    #endregion

    #region Session lifecycle

    [Fact]
    public async Task RunTurnAsync_SavesSessionWithCancellationTokenNone_OnFailure()
    {
        var conversation = SetConversation();

        try
        {
            await _sut.RunTurnAsync(CreateRequest(conversation));
        }
        catch
        {
            // Expected
        }

        await _tenantUow.Received().SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task RunTurnAsync_AIDisabled_NoticeTakesTheSequenceReservedInTheDatabase()
    {
        _tenant.Settings.AIEnabled = false;
        _tenantUow.SqlQueryAsync<int>(Arg.Any<FormattableString>(), Arg.Any<CancellationToken>()).Returns([42]);
        var conversation = SetConversation();

        await _sut.RunTurnAsync(CreateRequest(conversation));

        Assert.Equal(42, Assert.Single(AddedMessages()).Sequence);
    }

    /// <summary>
    /// A broker reply saved mid-turn is the trigger of the turn its wake-up starts. Stamping at the
    /// end of the turn used to claim it for the turn that never saw it.
    /// </summary>
    [Fact]
    public async Task RunTurnAsync_StampsOnlyTheUserRowsWaitingAtTurnStart()
    {
        var conversation = SetConversation();
        var trigger = AgentMessage.TextMessage(conversation.Id, 1, AgentMessageRole.User, "Assign load 42");
        var brokerReply = AgentMessage.TextMessage(conversation.Id, 2, AgentMessageRole.User, "Broker reply");
        List<AgentMessage> rows = [trigger];
        _messageRepo.Query().Returns(_ => rows.BuildMock());

        AgentSession? session = null;
        await _sessionRepo.AddAsync(Arg.Do<AgentSession>(s => session = s), Arg.Any<CancellationToken>());
        _broadcastService
            .When(b => b.BroadcastTurnUpdateAsync(Arg.Any<Guid>(), Arg.Any<AgentTurnUpdateDto>()))
            .Do(_ => rows.Add(brokerReply));

        await _sut.RunTurnAsync(CreateRequest(conversation));

        Assert.NotNull(session);
        Assert.Equal(session.Id, trigger.SessionId);
        Assert.Null(brokerReply.SessionId);
    }

    #endregion

    #region Cancellation (shared across session types)

    [Fact]
    public async Task CancelAsync_SessionNotFound_ReturnsFalse()
    {
        _sessionRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((AgentSession?)null);

        var result = await _sut.CancelAsync(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task CancelAsync_SessionNotRunning_ReturnsFalse()
    {
        var session = new AgentSession { CreatedAt = DateTime.UtcNow };
        session.Complete();

        _sessionRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(session);

        var result = await _sut.CancelAsync(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task CancelAsync_RunningSession_ReturnsTrue()
    {
        var session = new AgentSession { CreatedAt = DateTime.UtcNow };

        _sessionRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(session);

        var result = await _sut.CancelAsync(Guid.NewGuid());

        Assert.True(result);
        Assert.Equal(AgentSessionStatus.Cancelled, session.Status);
    }

    #endregion
}
