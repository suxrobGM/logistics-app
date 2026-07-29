using System.Text.Json.Nodes;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Application.Abstractions.AI;
using Logistics.Infrastructure.AI.Providers;
using Logistics.Infrastructure.AI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable;
using NSubstitute;
using Xunit;
using Logistics.Application.Abstractions.Features;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.SystemSettings;
using MsOptions = Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.AI.Tests.Services;

public class AIDispatchConversationBuilderTests
{
    private readonly ILogger<AIDispatchConversationBuilder> logger = NullLogger<AIDispatchConversationBuilder>.Instance;

    private readonly AIDispatchConversationBuilder sut;
    private readonly IAIDispatchToolRegistry toolRegistry = Substitute.For<IAIDispatchToolRegistry>();
    private readonly IFeatureService featureService = Substitute.For<IFeatureService>();
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ISystemSettingsService systemSettings = Substitute.For<ISystemSettingsService>();
    private readonly ITenantRepository<AIDispatchPolicy, Guid> policyRepo =
        Substitute.For<ITenantRepository<AIDispatchPolicy, Guid>>();

    public AIDispatchConversationBuilderTests()
    {
        toolRegistry.GetToolDefinitions(
                Arg.Any<IReadOnlySet<TenantFeature>>(),
                Arg.Any<IReadOnlySet<string>?>(),
                Arg.Any<bool>())
            .Returns([new AIDispatchToolDefinition("test_tool", "A test tool", new JsonObject { ["type"] = "object" })]);

        SetTenant();

        // Mock the AIDispatchSession repository for GetPreviousSessionContextAsync
        var sessionRepo = Substitute.For<ITenantRepository<AIDispatchSession, Guid>>();
        var emptySessionList = new List<AIDispatchSession>().BuildMock();
        sessionRepo.Query().Returns(emptySessionList);
        tenantUow.Repository<AIDispatchSession>().Returns(sessionRepo);

        tenantUow.Repository<AIDispatchPolicy>().Returns(policyRepo);
        SetPolicies();

        var llmOptions = MsOptions.Options.Create(ValidConfig);
        var providerFactory = new LlmProviderFactory(llmOptions);
        var modelResolver = new LlmModelResolver(systemSettings, NullLogger<LlmModelResolver>.Instance);

        var sessionSetup = new LlmSessionSetup(featureService, providerFactory, modelResolver, tenantUow);

        sut = new AIDispatchConversationBuilder(toolRegistry, sessionSetup, tenantUow, systemSettings, logger);
    }

    private static LlmOptions ValidConfig => new()
    {
        DefaultProvider = LlmProvider.Anthropic,
        MaxTokens = 4096,
        Providers = new Dictionary<LlmProvider, LlmProviderOptions>
        {
            [LlmProvider.Anthropic] = new() { ApiKey = "sk-ant-test-key", Model = "claude-sonnet-4-6" }
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

    private static AIDispatchRequest CreateRequest(AIDispatchMode mode = AIDispatchMode.Autonomous)
    {
        return new AIDispatchRequest(Guid.NewGuid(), mode, null);
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
        var session = new AIDispatchSession { Mode = AIDispatchMode.Autonomous, StartedAt = DateTime.UtcNow };

        var conversation = await sut.BuildAsync(session, CreateRequest(), ValidConfig);

        Assert.DoesNotContain("Dispatcher Preferences", conversation.SystemPrompt);
    }

    /// <summary>A disabled policy is filtered out in SQL, so it never reaches the prompt.</summary>
    [Fact]
    public async Task BuildAsync_DisabledPolicy_OmitsPolicySection()
    {
        SetPolicies(CreatePolicy(learned: "## Learned preferences\n- Prefer short hauls (5 rejections)", isEnabled: false));
        var session = new AIDispatchSession { Mode = AIDispatchMode.Autonomous, StartedAt = DateTime.UtcNow };

        var conversation = await sut.BuildAsync(session, CreateRequest(), ValidConfig);

        Assert.DoesNotContain("Dispatcher Preferences", conversation.SystemPrompt);
        Assert.DoesNotContain("Prefer short hauls", conversation.SystemPrompt);
    }

    [Fact]
    public async Task BuildAsync_EnabledPolicy_InjectsBothSections()
    {
        SetPolicies(CreatePolicy(
            learned: "## Learned preferences\n- Prefer short hauls (5 rejections)",
            directives: "- Never assign Truck 42 to hazmat"));
        var session = new AIDispatchSession { Mode = AIDispatchMode.Autonomous, StartedAt = DateTime.UtcNow };

        var conversation = await sut.BuildAsync(session, CreateRequest(), ValidConfig);

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
        var session = new AIDispatchSession { Mode = AIDispatchMode.Autonomous, StartedAt = DateTime.UtcNow };

        var conversation = await sut.BuildAsync(session, CreateRequest(), ValidConfig);

        Assert.Contains("Dispatcher directives", conversation.SystemPrompt);
        Assert.DoesNotContain("### Learned preferences", conversation.SystemPrompt);
    }

    #endregion

    #region Operating mode

    [Fact]
    public async Task BuildAsync_SoloOperatorTenant_BuildsTheSoloPrompt()
    {
        SetTenant(OperatingMode.SoloOperator);
        var session = new AIDispatchSession { Mode = AIDispatchMode.Autonomous, StartedAt = DateTime.UtcNow };

        var conversation = await sut.BuildAsync(session, CreateRequest(), ValidConfig);

        Assert.Contains("## Fleet Profile: SOLO OWNER-OPERATOR", conversation.SystemPrompt);
        Assert.DoesNotContain("Maximize fleet utilization", conversation.SystemPrompt);
    }

    [Fact]
    public async Task BuildAsync_FleetTenant_BuildsTheFleetPrompt()
    {
        var session = new AIDispatchSession { Mode = AIDispatchMode.Autonomous, StartedAt = DateTime.UtcNow };

        var conversation = await sut.BuildAsync(session, CreateRequest(), ValidConfig);

        Assert.DoesNotContain("SOLO OWNER-OPERATOR", conversation.SystemPrompt);
        Assert.Contains("Maximize fleet utilization", conversation.SystemPrompt);
    }

    #endregion

    [Fact]
    public async Task BuildAsync_ValidConfig_ReturnsConversation()
    {
        var session = new AIDispatchSession { Mode = AIDispatchMode.Autonomous, StartedAt = DateTime.UtcNow };

        var conversation = await sut.BuildAsync(session, CreateRequest(), ValidConfig);

        Assert.NotNull(conversation.Provider);
        Assert.Single(conversation.Messages);
        Assert.Equal(ValidConfig.MaxTokens, conversation.MaxTokens);
        Assert.Equal("claude-sonnet-4-6", conversation.Model);
    }

    [Fact]
    public async Task BuildAsync_IncludesToolsFromRegistry()
    {
        var session = new AIDispatchSession { Mode = AIDispatchMode.Autonomous, StartedAt = DateTime.UtcNow };

        var conversation = await sut.BuildAsync(session, CreateRequest(), ValidConfig);

        Assert.Single(conversation.Tools);
    }

    [Fact]
    public async Task BuildAsync_IncludesSystemPrompt()
    {
        var session = new AIDispatchSession { Mode = AIDispatchMode.Autonomous, StartedAt = DateTime.UtcNow };

        var conversation = await sut.BuildAsync(session, CreateRequest(), ValidConfig);

        Assert.NotNull(conversation.SystemPrompt);
        Assert.NotEmpty(conversation.SystemPrompt);
    }

    [Fact]
    public async Task BuildAsync_MissingApiKey_Throws()
    {
        var session = new AIDispatchSession { Mode = AIDispatchMode.Autonomous, StartedAt = DateTime.UtcNow };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.BuildAsync(session, CreateRequest(), EmptyApiKeyConfig));

        Assert.Contains("API key", ex.Message);
    }

    [Fact]
    public async Task BuildAsync_InitialMessage_ContainsFleetAnalysisInstruction()
    {
        var session = new AIDispatchSession { Mode = AIDispatchMode.Autonomous, StartedAt = DateTime.UtcNow };

        var conversation = await sut.BuildAsync(session, CreateRequest(), ValidConfig);

        Assert.Single(conversation.Messages);
        Assert.NotEmpty(conversation.Messages[0].Content);
    }
}
