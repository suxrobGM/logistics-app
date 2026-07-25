using System.Text.Json.Nodes;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Application.Abstractions.Ai;
using Logistics.Infrastructure.AI.Providers;
using Logistics.Infrastructure.AI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable;
using NSubstitute;
using Xunit;
using Logistics.Application.Abstractions.Features;
using Logistics.Application.Abstractions.AiDispatch;
using Logistics.Application.Abstractions.SystemSettings;
using MsOptions = Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.AI.Tests.Services;

public class AiDispatchConversationBuilderTests
{
    private readonly ILogger<AiDispatchConversationBuilder> logger = NullLogger<AiDispatchConversationBuilder>.Instance;

    private readonly AiDispatchConversationBuilder sut;
    private readonly IAiDispatchToolRegistry toolRegistry = Substitute.For<IAiDispatchToolRegistry>();
    private readonly IFeatureService featureService = Substitute.For<IFeatureService>();
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ISystemSettingsService systemSettings = Substitute.For<ISystemSettingsService>();
    private readonly ITenantRepository<AiDispatchPolicy, Guid> policyRepo =
        Substitute.For<ITenantRepository<AiDispatchPolicy, Guid>>();

    public AiDispatchConversationBuilderTests()
    {
        toolRegistry.GetToolDefinitions(Arg.Any<bool>()).Returns(
            [new AiDispatchToolDefinition("test_tool", "A test tool", new JsonObject { ["type"] = "object" })]);

        tenantUow.GetCurrentTenant().Returns(new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Test Fleet",
            ConnectionString = "test",
            BillingEmail = "test@test.com",
            CompanyAddress = new() { Line1 = "123 Test St", City = "Test", State = "TX", ZipCode = "12345", Country = "US" }
        });

        // Mock the AiDispatchSession repository for GetPreviousSessionContextAsync
        var sessionRepo = Substitute.For<ITenantRepository<AiDispatchSession, Guid>>();
        var emptySessionList = new List<AiDispatchSession>().BuildMock();
        sessionRepo.Query().Returns(emptySessionList);
        tenantUow.Repository<AiDispatchSession>().Returns(sessionRepo);

        tenantUow.Repository<AiDispatchPolicy>().Returns(policyRepo);
        SetPolicies();

        var llmOptions = MsOptions.Options.Create(ValidConfig);
        var providerFactory = new LlmProviderFactory(llmOptions);
        var modelResolver = new LlmModelResolver(systemSettings, NullLogger<LlmModelResolver>.Instance);

        sut = new AiDispatchConversationBuilder(toolRegistry, featureService, providerFactory, modelResolver, tenantUow, systemSettings, logger);
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

    private static AiDispatchRequest CreateRequest(AiDispatchMode mode = AiDispatchMode.Autonomous)
    {
        return new AiDispatchRequest(Guid.NewGuid(), mode, null);
    }

    private void SetPolicies(params AiDispatchPolicy[] policies)
    {
        policyRepo.Query().Returns(policies.ToList().BuildMock());
    }

    private static AiDispatchPolicy CreatePolicy(
        string? learned = null,
        string? directives = null,
        bool isEnabled = true)
    {
        var policy = new AiDispatchPolicy();
        policy.ApplyLearnedPolicy(learned, 20, DateTime.UtcNow, "deepseek-v4-flash", 0.001m);
        policy.EditManual(directives, isEnabled, Guid.NewGuid());
        return policy;
    }

    #region Learned policy injection

    [Fact]
    public async Task BuildAsync_NoPolicyRow_OmitsPolicySection()
    {
        SetPolicies();
        var session = new AiDispatchSession { Mode = AiDispatchMode.Autonomous, StartedAt = DateTime.UtcNow };

        var conversation = await sut.BuildAsync(session, CreateRequest(), ValidConfig);

        Assert.DoesNotContain("Dispatcher Preferences", conversation.SystemPrompt);
    }

    /// <summary>A disabled policy is filtered out in SQL, so it never reaches the prompt.</summary>
    [Fact]
    public async Task BuildAsync_DisabledPolicy_OmitsPolicySection()
    {
        SetPolicies(CreatePolicy(learned: "## Learned preferences\n- Prefer short hauls (5 rejections)", isEnabled: false));
        var session = new AiDispatchSession { Mode = AiDispatchMode.Autonomous, StartedAt = DateTime.UtcNow };

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
        var session = new AiDispatchSession { Mode = AiDispatchMode.Autonomous, StartedAt = DateTime.UtcNow };

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
        var session = new AiDispatchSession { Mode = AiDispatchMode.Autonomous, StartedAt = DateTime.UtcNow };

        var conversation = await sut.BuildAsync(session, CreateRequest(), ValidConfig);

        Assert.Contains("Dispatcher directives", conversation.SystemPrompt);
        Assert.DoesNotContain("### Learned preferences", conversation.SystemPrompt);
    }

    #endregion

    [Fact]
    public async Task BuildAsync_ValidConfig_ReturnsConversation()
    {
        var session = new AiDispatchSession { Mode = AiDispatchMode.Autonomous, StartedAt = DateTime.UtcNow };

        var conversation = await sut.BuildAsync(session, CreateRequest(), ValidConfig);

        Assert.NotNull(conversation.Provider);
        Assert.Single(conversation.Messages);
        Assert.Equal(ValidConfig.MaxTokens, conversation.MaxTokens);
        Assert.Equal("claude-sonnet-4-6", conversation.Model);
    }

    [Fact]
    public async Task BuildAsync_IncludesToolsFromRegistry()
    {
        var session = new AiDispatchSession { Mode = AiDispatchMode.Autonomous, StartedAt = DateTime.UtcNow };

        var conversation = await sut.BuildAsync(session, CreateRequest(), ValidConfig);

        Assert.Single(conversation.Tools);
    }

    [Fact]
    public async Task BuildAsync_IncludesSystemPrompt()
    {
        var session = new AiDispatchSession { Mode = AiDispatchMode.Autonomous, StartedAt = DateTime.UtcNow };

        var conversation = await sut.BuildAsync(session, CreateRequest(), ValidConfig);

        Assert.NotNull(conversation.SystemPrompt);
        Assert.NotEmpty(conversation.SystemPrompt);
    }

    [Fact]
    public async Task BuildAsync_MissingApiKey_Throws()
    {
        var session = new AiDispatchSession { Mode = AiDispatchMode.Autonomous, StartedAt = DateTime.UtcNow };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.BuildAsync(session, CreateRequest(), EmptyApiKeyConfig));

        Assert.Contains("API key", ex.Message);
    }

    [Fact]
    public async Task BuildAsync_InitialMessage_ContainsFleetAnalysisInstruction()
    {
        var session = new AiDispatchSession { Mode = AiDispatchMode.Autonomous, StartedAt = DateTime.UtcNow };

        var conversation = await sut.BuildAsync(session, CreateRequest(), ValidConfig);

        Assert.Single(conversation.Messages);
        Assert.NotEmpty(conversation.Messages[0].Content);
    }
}
