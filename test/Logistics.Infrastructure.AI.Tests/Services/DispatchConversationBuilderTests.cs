using System.Text.Json.Nodes;
using Logistics.Application.Services;
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
using MsOptions = Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.AI.Tests.Services;

public class DispatchConversationBuilderTests
{
    private readonly ILogger<DispatchConversationBuilder> logger = NullLogger<DispatchConversationBuilder>.Instance;

    private readonly DispatchConversationBuilder sut;
    private readonly IDispatchToolRegistry toolRegistry = Substitute.For<IDispatchToolRegistry>();
    private readonly IFeatureService featureService = Substitute.For<IFeatureService>();
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();

    public DispatchConversationBuilderTests()
    {
        toolRegistry.GetToolDefinitions(Arg.Any<bool>()).Returns(
            [new DispatchToolDefinition("test_tool", "A test tool", new JsonObject { ["type"] = "object" })]);

        tenantUow.GetCurrentTenant().Returns(new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Test Fleet",
            ConnectionString = "test",
            BillingEmail = "test@test.com",
            CompanyAddress = new() { Line1 = "123 Test St", City = "Test", State = "TX", ZipCode = "12345", Country = "US" }
        });

        // Mock the DispatchSession repository for GetPreviousSessionContextAsync
        var sessionRepo = Substitute.For<ITenantRepository<DispatchSession, Guid>>();
        var emptySessionList = new List<DispatchSession>().BuildMock();
        sessionRepo.Query().Returns(emptySessionList);
        tenantUow.Repository<DispatchSession>().Returns(sessionRepo);

        var llmOptions = MsOptions.Options.Create(ValidConfig);
        var providerFactory = new LlmProviderFactory(llmOptions);

        sut = new DispatchConversationBuilder(toolRegistry, featureService, providerFactory, tenantUow, logger);
    }

    private static LlmOptions ValidConfig => new()
    {
        MaxTokens = 4096,
        Providers = new Dictionary<LlmProviderType, LlmProviderOptions>
        {
            [LlmProviderType.Anthropic] = new() { ApiKey = "sk-ant-test-key", Model = "claude-sonnet-4-6" }
        }
    };

    private static LlmOptions EmptyApiKeyConfig => new()
    {
        MaxTokens = 100,
        Providers = new Dictionary<LlmProviderType, LlmProviderOptions>
        {
            [LlmProviderType.Anthropic] = new() { ApiKey = "", Model = "test" }
        }
    };

    private static DispatchAgentRequest CreateRequest(DispatchAgentMode mode = DispatchAgentMode.Autonomous)
    {
        return new DispatchAgentRequest(Guid.NewGuid(), mode, null);
    }

    [Fact]
    public async Task BuildAsync_ValidConfig_ReturnsConversation()
    {
        var session = new DispatchSession { Mode = DispatchAgentMode.Autonomous, StartedAt = DateTime.UtcNow };

        var conversation = await sut.BuildAsync(session, CreateRequest(), ValidConfig);

        Assert.NotNull(conversation.Provider);
        Assert.Single(conversation.Messages);
        Assert.Equal(ValidConfig.MaxTokens, conversation.MaxTokens);
        Assert.Equal("claude-sonnet-4-6", conversation.Model);
    }

    [Fact]
    public async Task BuildAsync_IncludesToolsFromRegistry()
    {
        var session = new DispatchSession { Mode = DispatchAgentMode.Autonomous, StartedAt = DateTime.UtcNow };

        var conversation = await sut.BuildAsync(session, CreateRequest(), ValidConfig);

        Assert.Single(conversation.Tools);
    }

    [Fact]
    public async Task BuildAsync_IncludesSystemPrompt()
    {
        var session = new DispatchSession { Mode = DispatchAgentMode.Autonomous, StartedAt = DateTime.UtcNow };

        var conversation = await sut.BuildAsync(session, CreateRequest(), ValidConfig);

        Assert.NotNull(conversation.SystemPrompt);
        Assert.NotEmpty(conversation.SystemPrompt);
    }

    [Fact]
    public async Task BuildAsync_MissingApiKey_Throws()
    {
        var session = new DispatchSession { Mode = DispatchAgentMode.Autonomous, StartedAt = DateTime.UtcNow };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.BuildAsync(session, CreateRequest(), EmptyApiKeyConfig));

        Assert.Contains("API key", ex.Message);
    }

    [Fact]
    public async Task BuildAsync_InitialMessage_ContainsFleetAnalysisInstruction()
    {
        var session = new DispatchSession { Mode = DispatchAgentMode.Autonomous, StartedAt = DateTime.UtcNow };

        var conversation = await sut.BuildAsync(session, CreateRequest(), ValidConfig);

        Assert.Single(conversation.Messages);
        Assert.NotEmpty(conversation.Messages[0].Content);
    }
}
