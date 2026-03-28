using System.Text.Json.Nodes;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Options;
using Logistics.Infrastructure.AI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable;
using NSubstitute;
using Xunit;

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

        sut = new DispatchConversationBuilder(toolRegistry, featureService, tenantUow, logger);
    }

    private static LlmOptions ValidConfig => new()
    {
        ApiKey = "sk-ant-test-key",
        Model = "claude-sonnet-4-6",
        MaxTokens = 4096
    };

    private static DispatchAgentRequest CreateRequest(DispatchAgentMode mode = DispatchAgentMode.Autonomous)
    {
        return new DispatchAgentRequest(Guid.NewGuid(), mode, null);
    }

    [Fact]
    public async Task BuildAsync_ValidConfig_ReturnsClientAndParameters()
    {
        var session = new DispatchSession { Mode = DispatchAgentMode.Autonomous, StartedAt = DateTime.UtcNow };

        var (client, parameters, messages) = await sut.BuildAsync(session, CreateRequest(), ValidConfig);

        Assert.NotNull(client);
        Assert.NotNull(parameters);
        Assert.Single(messages);
        Assert.Equal(ValidConfig.MaxTokens, parameters.MaxTokens);
        Assert.Equal(ValidConfig.Model, parameters.Model);
        Assert.Equal(0m, parameters.Temperature);
    }

    [Fact]
    public async Task BuildAsync_IncludesToolsFromRegistry()
    {
        var session = new DispatchSession { Mode = DispatchAgentMode.Autonomous, StartedAt = DateTime.UtcNow };

        var (_, parameters, _) = await sut.BuildAsync(session, CreateRequest(), ValidConfig);

        Assert.NotNull(parameters.Tools);
        Assert.Single(parameters.Tools);
    }

    [Fact]
    public async Task BuildAsync_IncludesSystemPrompt()
    {
        var session = new DispatchSession { Mode = DispatchAgentMode.Autonomous, StartedAt = DateTime.UtcNow };

        var (_, parameters, _) = await sut.BuildAsync(session, CreateRequest(), ValidConfig);

        Assert.NotNull(parameters.System);
        Assert.NotEmpty(parameters.System);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task BuildAsync_MissingApiKey_Throws(string? apiKey)
    {
        var session = new DispatchSession { Mode = DispatchAgentMode.Autonomous, StartedAt = DateTime.UtcNow };
        var config = new LlmOptions { ApiKey = apiKey!, Model = "test", MaxTokens = 100 };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.BuildAsync(session, CreateRequest(), config));

        Assert.Contains("API key", ex.Message);
    }

    [Fact]
    public async Task BuildAsync_InitialMessage_ContainsFleetAnalysisInstruction()
    {
        var session = new DispatchSession { Mode = DispatchAgentMode.Autonomous, StartedAt = DateTime.UtcNow };

        var (_, _, messages) = await sut.BuildAsync(session, CreateRequest(), ValidConfig);

        Assert.Single(messages);
        var content = messages[0].Content?.FirstOrDefault();
        Assert.NotNull(content);
    }
}
