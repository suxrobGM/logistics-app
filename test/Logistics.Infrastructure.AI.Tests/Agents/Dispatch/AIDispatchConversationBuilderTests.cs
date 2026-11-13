using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Tests.TestKit;
using Logistics.Infrastructure.AI.Agents;
using Logistics.Infrastructure.AI.Agents.Dispatch;
using System.Text.Json.Nodes;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Application.Abstractions.AI;
using Logistics.Infrastructure.AI.Llm;
using Logistics.Infrastructure.AI.Llm.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using Logistics.Application.Abstractions.Features;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.SystemSettings;
using MsOptions = Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.AI.Tests.Agents.Dispatch;

public class AIDispatchConversationBuilderTests
{
    private readonly ILogger<AIDispatchConversationBuilder> _logger = NullLogger<AIDispatchConversationBuilder>.Instance;

    private readonly AIDispatchConversationBuilder _sut;
    private readonly IAgentToolRegistry _toolRegistry = Substitute.For<IAgentToolRegistry>();
    private readonly IFeatureService _featureService = Substitute.For<IFeatureService>();
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ISystemSettingsService _systemSettings = Substitute.For<ISystemSettingsService>();
    private readonly List<AgentMessage> _rows = [];

    public AIDispatchConversationBuilderTests()
    {
        _toolRegistry.GetDispatchAgentTools(Arg.Any<IReadOnlySet<TenantFeature>>())
            .Returns([new AgentToolDefinition("test_tool", "A test tool")]);

        SetTenant();

        _tenantUow.Stub(_rows);
        _tenantUow.Stub<AIDispatchPolicy>();

        var llmOptions = MsOptions.Options.Create(ValidConfig);
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient(Arg.Any<string>()).Returns(_ => new HttpClient());
        var providerFactory = new LlmProviderFactory(llmOptions, httpClientFactory);
        var modelResolver = new LlmModelResolver(_systemSettings, NullLogger<LlmModelResolver>.Instance);

        var sessionSetup = new LlmSessionSetup(
            _featureService, providerFactory, modelResolver, _systemSettings, _tenantUow);

        _sut = new AIDispatchConversationBuilder(_toolRegistry, sessionSetup, _tenantUow, new AgentTranscriptLoader(_tenantUow), _logger);
    }

    private static LlmOptions ValidConfig => new()
    {
        DefaultProvider = LlmProvider.Anthropic,
        MaxTokens = 4096,
        Providers = new Dictionary<LlmProvider, LlmProviderOptions>
        {
            [LlmProvider.Anthropic] = new() { ApiKey = "sk-ant-test-key", Model = "claude-sonnet-5-5" }
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

    /// <summary>A conversation whose single user message is "in the database".</summary>
    private AgentConversation CreateConversation(string text = "Assign what you can")
    {
        var conversation = new AgentConversation { Kind = AgentConversationKind.Dispatch };
        _rows.Add(conversation.AddTextMessage(_rows.Count + 1, AgentMessageRole.User, text));
        return conversation;
    }

    private void SetTenant(params TenantPreset[] presets)
    {
        var tenant = TestTenant.Create();
        tenant.Presets = [.. presets];
        _tenantUow.GetCurrentTenant().Returns(tenant);
    }

    private static AIDispatchPolicy CreatePolicy(
        string? learned = null,
        string? directives = null,
        bool isEnabled = true)
    {
        var policy = new AIDispatchPolicy();
        policy.ApplyLearnedPolicy(learned, 20, DateTime.UtcNow, "deepseek-flash");
        policy.EditManual(directives, isEnabled, Guid.NewGuid());
        return policy;
    }

    #region Learned policy injection

    [Fact]
    public async Task BuildAsync_NoPolicy_OmitsPolicySection()
    {
        var session = new AgentSession { CreatedAt = DateTime.UtcNow };

        var conversation = await _sut.BuildAsync(session, CreateConversation(), ValidConfig, CancellationToken.None);

        Assert.DoesNotContain("Dispatcher Preferences", conversation.SystemPrompt);
        Assert.DoesNotContain("Prefer short hauls", conversation.SystemPrompt);
    }

    /// <summary>A disabled policy is filtered out in SQL, so it never reaches the prompt.</summary>
    [Fact]
    public async Task BuildAsync_DisabledPolicy_OmitsPolicySection()
    {
        _tenantUow.Stub(CreatePolicy(learned: "## Learned preferences\n- Prefer short hauls (5 rejections)", isEnabled: false));
        var session = new AgentSession { CreatedAt = DateTime.UtcNow };

        var conversation = await _sut.BuildAsync(session, CreateConversation(), ValidConfig, CancellationToken.None);

        Assert.DoesNotContain("Dispatcher Preferences", conversation.SystemPrompt);
        Assert.DoesNotContain("Prefer short hauls", conversation.SystemPrompt);
    }

    [Fact]
    public async Task BuildAsync_EnabledPolicy_InjectsBothSections()
    {
        _tenantUow.Stub(CreatePolicy(
            learned: "## Learned preferences\n- Prefer short hauls (5 rejections)",
            directives: "- Never assign Truck 42 to hazmat"));
        var session = new AgentSession { CreatedAt = DateTime.UtcNow };

        var conversation = await _sut.BuildAsync(session, CreateConversation(), ValidConfig, CancellationToken.None);

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
        _tenantUow.Stub(CreatePolicy(directives: "- Prefer flatbeds out of Dallas"));
        var session = new AgentSession { CreatedAt = DateTime.UtcNow };

        var conversation = await _sut.BuildAsync(session, CreateConversation(), ValidConfig, CancellationToken.None);

        Assert.Contains("Dispatcher directives", conversation.SystemPrompt);
        Assert.DoesNotContain("### Learned preferences", conversation.SystemPrompt);
    }

    #endregion

    #region Fleet vs solo owner-operator

    [Fact]
    public async Task BuildAsync_SoloOperatorPreset_UsesTheSoloPrompt()
    {
        SetTenant(TenantPreset.GeneralFreight, TenantPreset.SoloOperator);
        var session = new AgentSession { CreatedAt = DateTime.UtcNow };

        var conversation = await _sut.BuildAsync(session, CreateConversation(), ValidConfig, CancellationToken.None);

        Assert.Contains("## Fleet Profile: SOLO OWNER-OPERATOR", conversation.SystemPrompt);
        Assert.DoesNotContain("Maximize fleet utilization", conversation.SystemPrompt);
    }

    [Fact]
    public async Task BuildAsync_NoSoloPreset_UsesTheFleetPrompt()
    {
        var session = new AgentSession { CreatedAt = DateTime.UtcNow };

        var conversation = await _sut.BuildAsync(session, CreateConversation(), ValidConfig, CancellationToken.None);

        Assert.DoesNotContain("## Fleet Profile: SOLO OWNER-OPERATOR", conversation.SystemPrompt);
        Assert.Contains("Maximize fleet utilization", conversation.SystemPrompt);
    }

    #endregion

    [Fact]
    public async Task BuildAsync_ValidConfig_ReturnsConversation()
    {
        var session = new AgentSession { CreatedAt = DateTime.UtcNow };

        var conversation = await _sut.BuildAsync(session, CreateConversation(), ValidConfig, CancellationToken.None);

        Assert.NotNull(conversation.Provider);
        Assert.Single(conversation.Messages);
        Assert.Equal(ValidConfig.MaxTokens, conversation.MaxTokens);
        Assert.Equal("claude-sonnet-5-5", conversation.Model);
        Assert.Single(conversation.Tools);
        Assert.NotEmpty(conversation.SystemPrompt);
    }

    [Fact]
    public async Task BuildAsync_MissingApiKey_Throws()
    {
        var session = new AgentSession { CreatedAt = DateTime.UtcNow };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.BuildAsync(session, CreateConversation(), EmptyApiKeyConfig, CancellationToken.None));

        Assert.Contains("API key", ex.Message);
    }

    #region Transcript replay

    [Fact]
    public async Task BuildAsync_RebuildsMessagesFromTranscript()
    {
        var conversation = new AgentConversation { Kind = AgentConversationKind.Dispatch };
        _rows.Add(conversation.AddTextMessage(_rows.Count + 1, AgentMessageRole.User, "Assign truck 5 to load 42"));
        _rows.Add(conversation.AddTextMessage(_rows.Count + 1, AgentMessageRole.Assistant, "Done - see summary."));
        _rows.Add(conversation.AddTextMessage(_rows.Count + 1, AgentMessageRole.User, "Now do the rest"));
        var session = new AgentSession { CreatedAt = DateTime.UtcNow };

        var llmConversation = await _sut.BuildAsync(session, conversation, ValidConfig, CancellationToken.None);

        Assert.Equal(3, llmConversation.Messages.Count);
    }

    /// <summary>The time comes from the stored row, so the system prompt carries no date.</summary>
    [Fact]
    public async Task BuildAsync_StampsUserMessagesWithWhenTheyWereSent()
    {
        var conversation = CreateConversation("Now do the rest");
        var sentAt = _rows[0].CreatedAt;
        var session = new AgentSession { CreatedAt = DateTime.UtcNow };

        var llmConversation = await _sut.BuildAsync(session, conversation, ValidConfig, CancellationToken.None);

        var stamp = Assert.IsType<LlmTextBlock>(llmConversation.Messages[^1].Content[0]);
        Assert.Equal(AgentTranscriptLoader.SentStamp(sentAt), stamp.Text);
        Assert.DoesNotContain(DateTime.UtcNow.ToString("yyyy-MM-dd"), llmConversation.SystemPrompt);
    }

    /// <summary>
    /// The prompt cache only hits an unchanged prefix, so building the same turn twice must give the
    /// same bytes - nothing may be stamped per request.
    /// </summary>
    [Fact]
    public async Task BuildAsync_SameTranscript_ReplaysIdentically()
    {
        var conversation = CreateConversation("Assign what you can");
        var session = new AgentSession { CreatedAt = DateTime.UtcNow };

        var first = await _sut.BuildAsync(session, conversation, ValidConfig, CancellationToken.None);
        var second = await _sut.BuildAsync(session, conversation, ValidConfig, CancellationToken.None);

        Assert.Equal(first.SystemPrompt, second.SystemPrompt);
        Assert.Equal(
            first.Messages.SelectMany(m => m.Content).Cast<LlmTextBlock>().Select(t => t.Text),
            second.Messages.SelectMany(m => m.Content).Cast<LlmTextBlock>().Select(t => t.Text));
    }

    #endregion
}
