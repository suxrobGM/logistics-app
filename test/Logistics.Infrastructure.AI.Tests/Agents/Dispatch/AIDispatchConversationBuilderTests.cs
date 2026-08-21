using Logistics.Application.Abstractions.Agents;
using Logistics.Infrastructure.AI.Agents;
using Logistics.Infrastructure.AI.Agents.Dispatch;
using System.Text.Json.Nodes;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Application.Abstractions.AI;
using Logistics.Infrastructure.AI.Llm;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable;
using NSubstitute;
using Xunit;
using Logistics.Application.Abstractions.Features;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.SystemSettings;
using MsOptions = Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.AI.Tests.Agents.Dispatch;

public class AIDispatchConversationBuilderTests
{
    private readonly ILogger<AIDispatchConversationBuilder> logger = NullLogger<AIDispatchConversationBuilder>.Instance;

    private readonly AIDispatchConversationBuilder sut;
    private readonly IAgentToolRegistry toolRegistry = Substitute.For<IAgentToolRegistry>();
    private readonly IFeatureService featureService = Substitute.For<IFeatureService>();
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ISystemSettingsService systemSettings = Substitute.For<ISystemSettingsService>();
    private readonly ITenantRepository<AIDispatchPolicy, Guid> policyRepo =
        Substitute.For<ITenantRepository<AIDispatchPolicy, Guid>>();

    public AIDispatchConversationBuilderTests()
    {
        toolRegistry.GetDispatchAgentTools(Arg.Any<IReadOnlySet<TenantFeature>>())
            .Returns([new AgentToolDefinition("test_tool", "A test tool", new JsonObject { ["type"] = "object" })]);

        SetTenant();

        tenantUow.Repository<AIDispatchPolicy>().Returns(policyRepo);
        SetPolicies();

        var llmOptions = MsOptions.Options.Create(ValidConfig);
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient(Arg.Any<string>()).Returns(_ => new HttpClient());
        var providerFactory = new LlmProviderFactory(llmOptions, httpClientFactory);
        var modelResolver = new LlmModelResolver(systemSettings, NullLogger<LlmModelResolver>.Instance);

        var sessionSetup = new LlmSessionSetup(
            featureService, providerFactory, modelResolver, systemSettings, tenantUow);

        sut = new AIDispatchConversationBuilder(toolRegistry, sessionSetup, tenantUow, logger);
    }

    private static LlmOptions ValidConfig => new()
    {
        DefaultProvider = LlmProvider.Anthropic,
        MaxTokens = 4096,
        Providers = new Dictionary<LlmProvider, LlmProviderOptions>
        {
            [LlmProvider.Anthropic] = new() { ApiKey = "sk-ant-test-key", Model = "claude-sonnet-5" }
        }
    };

    private static LlmOptions EmptyApiKeyConfig => new()
    {
        DefaultProvider = LlmProvider.Anthropic,
        MaxTokens = 100,
        Providers = new Dictionary<LlmProvider, LlmProviderOptions>
        {
            [LlmProvider.Anthropic] = new() { ApiKey = "", Model = "test" }
        }
    };

    private static AgentConversation CreateConversation(string text = "Assign what you can")
    {
        var conversation = new AgentConversation { Kind = AgentConversationKind.Dispatch };
        conversation.AddTextMessage(AgentMessageRole.User, text);
        return conversation;
    }

    private void SetTenant(OperatingMode operatingMode = OperatingMode.Fleet)
    {
        tenantUow.GetCurrentTenant().Returns(new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Test Fleet",
            ConnectionString = "test",
            BillingEmail = "test@test.com",
            CompanyAddress = new() { Line1 = "123 Test St", City = "Test", State = "TX", ZipCode = "12345", Country = "US" },
            Settings = new() { OperatingMode = operatingMode }
        });
    }

    private void SetPolicies(params AIDispatchPolicy[] policies)
    {
        policyRepo.Query().Returns(policies.ToList().BuildMock());
    }

    private static AIDispatchPolicy CreatePolicy(
        string? learned = null,
        string? directives = null,
        bool isEnabled = true)
    {
        var policy = new AIDispatchPolicy();
        policy.ApplyLearnedPolicy(learned, 20, DateTime.UtcNow, "deepseek-v4-flash", 0.001m);
        policy.EditManual(directives, isEnabled, Guid.NewGuid());
        return policy;
    }

    #region Learned policy injection

    [Fact]
    public async Task BuildAsync_NoPolicyRow_OmitsPolicySection()
    {
        SetPolicies();
        var session = new AgentSession { StartedAt = DateTime.UtcNow };

        var conversation = await sut.BuildAsync(session, CreateConversation(), ValidConfig, CancellationToken.None);

        Assert.DoesNotContain("Dispatcher Preferences", conversation.SystemPrompt);
    }

    /// <summary>A disabled policy is filtered out in SQL, so it never reaches the prompt.</summary>
    [Fact]
    public async Task BuildAsync_DisabledPolicy_OmitsPolicySection()
    {
        SetPolicies(CreatePolicy(learned: "## Learned preferences\n- Prefer short hauls (5 rejections)", isEnabled: false));
        var session = new AgentSession { StartedAt = DateTime.UtcNow };

        var conversation = await sut.BuildAsync(session, CreateConversation(), ValidConfig, CancellationToken.None);

        Assert.DoesNotContain("Dispatcher Preferences", conversation.SystemPrompt);
        Assert.DoesNotContain("Prefer short hauls", conversation.SystemPrompt);
    }

    [Fact]
    public async Task BuildAsync_EnabledPolicy_InjectsBothSections()
    {
        SetPolicies(CreatePolicy(
            learned: "## Learned preferences\n- Prefer short hauls (5 rejections)",
            directives: "- Never assign Truck 42 to hazmat"));
        var session = new AgentSession { StartedAt = DateTime.UtcNow };

        var conversation = await sut.BuildAsync(session, CreateConversation(), ValidConfig, CancellationToken.None);

        Assert.Contains("Dispatcher Preferences", conversation.SystemPrompt);
        Assert.Contains("Prefer short hauls", conversation.SystemPrompt);
        Assert.Contains("Never assign Truck 42 to hazmat", conversation.SystemPrompt);

        // Directives outrank learned preferences, so they come first.
        Assert.True(
            conversation.SystemPrompt.IndexOf("Dispatcher directives", StringComparison.Ordinal) <
            conversation.SystemPrompt.IndexOf("Learned preferences", StringComparison.Ordinal));
    }

    [Fact]
    public async Task BuildAsync_PolicyWithOnlyDirectives_OmitsLearnedHeading()
    {
        SetPolicies(CreatePolicy(directives: "- Prefer flatbeds out of Dallas"));
        var session = new AgentSession { StartedAt = DateTime.UtcNow };

        var conversation = await sut.BuildAsync(session, CreateConversation(), ValidConfig, CancellationToken.None);

        Assert.Contains("Dispatcher directives", conversation.SystemPrompt);
        Assert.DoesNotContain("### Learned preferences", conversation.SystemPrompt);
    }

    #endregion

    #region Operating mode

    [Fact]
    public async Task BuildAsync_SoloOperatorTenant_BuildsTheSoloPrompt()
    {
        SetTenant(OperatingMode.SoloOperator);
        var session = new AgentSession { StartedAt = DateTime.UtcNow };

        var conversation = await sut.BuildAsync(session, CreateConversation(), ValidConfig, CancellationToken.None);

        Assert.Contains("## Fleet Profile: SOLO OWNER-OPERATOR", conversation.SystemPrompt);
        Assert.DoesNotContain("Maximize fleet utilization", conversation.SystemPrompt);
    }

    [Fact]
    public async Task BuildAsync_FleetTenant_BuildsTheFleetPrompt()
    {
        var session = new AgentSession { StartedAt = DateTime.UtcNow };

        var conversation = await sut.BuildAsync(session, CreateConversation(), ValidConfig, CancellationToken.None);

        Assert.DoesNotContain("SOLO OWNER-OPERATOR", conversation.SystemPrompt);
        Assert.Contains("Maximize fleet utilization", conversation.SystemPrompt);
    }

    #endregion

    [Fact]
    public async Task BuildAsync_ValidConfig_ReturnsConversation()
    {
        var session = new AgentSession { StartedAt = DateTime.UtcNow };

        var conversation = await sut.BuildAsync(session, CreateConversation(), ValidConfig, CancellationToken.None);

        Assert.NotNull(conversation.Provider);
        Assert.Single(conversation.Messages);
        Assert.Equal(ValidConfig.MaxTokens, conversation.MaxTokens);
        Assert.Equal("claude-sonnet-5", conversation.Model);
    }

    [Fact]
    public async Task BuildAsync_IncludesToolsFromRegistry()
    {
        var session = new AgentSession { StartedAt = DateTime.UtcNow };

        var conversation = await sut.BuildAsync(session, CreateConversation(), ValidConfig, CancellationToken.None);

        Assert.Single(conversation.Tools);
    }

    [Fact]
    public async Task BuildAsync_IncludesSystemPrompt()
    {
        var session = new AgentSession { StartedAt = DateTime.UtcNow };

        var conversation = await sut.BuildAsync(session, CreateConversation(), ValidConfig, CancellationToken.None);

        Assert.NotNull(conversation.SystemPrompt);
        Assert.NotEmpty(conversation.SystemPrompt);
    }

    [Fact]
    public async Task BuildAsync_MissingApiKey_Throws()
    {
        var session = new AgentSession { StartedAt = DateTime.UtcNow };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.BuildAsync(session, CreateConversation(), EmptyApiKeyConfig, CancellationToken.None));

        Assert.Contains("API key", ex.Message);
    }

    #region Turn context injection

    [Fact]
    public async Task BuildAsync_RebuildsMessagesFromTranscript()
    {
        var conversation = new AgentConversation { Kind = AgentConversationKind.Dispatch };
        conversation.AddTextMessage(AgentMessageRole.User, "Assign truck 5 to load 42");
        conversation.AddTextMessage(AgentMessageRole.Assistant, "Done - see summary.");
        conversation.AddTextMessage(AgentMessageRole.User, "Now do the rest");
        var session = new AgentSession { StartedAt = DateTime.UtcNow };

        var llmConversation = await sut.BuildAsync(session, conversation, ValidConfig, CancellationToken.None);

        Assert.Equal(3, llmConversation.Messages.Count);
    }

    /// <summary>The turn-context text lands only on the final user message, appended in-memory.</summary>
    [Fact]
    public async Task BuildAsync_AppendsTurnContextToFinalUserMessage()
    {
        var conversation = CreateConversation("Now do the rest");
        var session = new AgentSession { StartedAt = DateTime.UtcNow };

        var llmConversation = await sut.BuildAsync(session, conversation, ValidConfig, CancellationToken.None);

        var lastMessage = llmConversation.Messages[^1];
        Assert.True(lastMessage.Content.Count >= 2);
        Assert.Contains(lastMessage.Content, block =>
            block is Logistics.Infrastructure.AI.Llm.Contracts.LlmTextBlock text &&
            text.Text.Contains("Current time:", StringComparison.Ordinal));
    }

    /// <summary>Chit-chat must not be ordered to call fleet tools - the directive is conditional.</summary>
    [Fact]
    public async Task BuildAsync_TurnContext_MakesFleetChecksConditional()
    {
        var conversation = CreateConversation("hi");
        var session = new AgentSession { StartedAt = DateTime.UtcNow };

        var llmConversation = await sut.BuildAsync(session, conversation, ValidConfig, CancellationToken.None);

        var lastMessage = llmConversation.Messages[^1];
        var injected = Assert.IsType<Logistics.Infrastructure.AI.Llm.Contracts.LlmTextBlock>(
            lastMessage.Content[^1]);
        Assert.Contains("about to propose or take any", injected.Text);
        Assert.Contains("no tool calls", injected.Text);
    }

    /// <summary>Persisted rows keep only the user's typed text - the turn context must never leak in.</summary>
    [Fact]
    public async Task BuildAsync_DoesNotMutateThePersistedTranscript()
    {
        var conversation = CreateConversation("Assign what you can");
        var originalContentJson = conversation.Messages[0].ContentJson;
        var session = new AgentSession { StartedAt = DateTime.UtcNow };

        await sut.BuildAsync(session, conversation, ValidConfig, CancellationToken.None);

        Assert.Equal(originalContentJson, conversation.Messages[0].ContentJson);
        Assert.DoesNotContain("Current time:", conversation.Messages[0].ContentJson);
    }

    #endregion
}
