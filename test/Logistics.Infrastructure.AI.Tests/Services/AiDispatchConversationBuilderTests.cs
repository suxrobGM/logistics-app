using System.Text.Json.Nodes;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Options;
using Logistics.Infrastructure.AI.Providers;
using Logistics.Infrastructure.AI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable;
using NSubstitute;
using Xunit;
using Logistics.Application.Abstractions.Features;
using Logistics.Application.Abstractions.AiDispatch;
using MsOptions = Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.AI.Tests.Services;

public class AiDispatchConversationBuilderTests
{
    private readonly ILogger<AiDispatchConversationBuilder> logger = NullLogger<AiDispatchConversationBuilder>.Instance;

    private readonly AiDispatchConversationBuilder sut;
    private readonly IAiDispatchToolRegistry toolRegistry = Substitute.For<IAiDispatchToolRegistry>();
    private readonly IFeatureService featureService = Substitute.For<IFeatureService>();
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();

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

        var llmOptions = MsOptions.Options.Create(ValidConfig);
        var providerFactory = new LlmProviderFactory(llmOptions);

        sut = new AiDispatchConversationBuilder(toolRegistry, featureService, providerFactory, tenantUow, logger);
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
