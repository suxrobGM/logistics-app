using System.Text.Json.Nodes;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Providers;
using Logistics.Infrastructure.AI.Services;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Services;

public class DispatchDecisionProcessorTests
{
    private readonly ITenantRepository<DispatchDecision, Guid> decisionRepo =
        Substitute.For<ITenantRepository<DispatchDecision, Guid>>();

    private readonly ILogger<DispatchDecisionProcessor> logger = NullLogger<DispatchDecisionProcessor>.Instance;

    private readonly DispatchDecisionProcessor sut;
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IDispatchToolExecutor toolExecutor = Substitute.For<IDispatchToolExecutor>();
    private readonly IDispatchAgentBroadcastService broadcastService = Substitute.For<IDispatchAgentBroadcastService>();

    public DispatchDecisionProcessorTests()
    {
        tenantUow.Repository<DispatchDecision>().Returns(decisionRepo);
        tenantUow.GetCurrentTenant().Returns(new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Test Tenant",
            ConnectionString = "test-connection",
            BillingEmail = "test@test.com",
            CompanyAddress = new() { Line1 = "123 Test St", City = "Test", State = "TX", ZipCode = "12345", Country = "US" }
        });
        sut = new DispatchDecisionProcessor(toolExecutor, tenantUow, broadcastService, logger);
    }

    private static DispatchSession CreateSession(DispatchAgentMode mode = DispatchAgentMode.Autonomous)
    {
        return new DispatchSession { Mode = mode, StartedAt = DateTime.UtcNow };
    }

    private static LlmToolUseBlock CreateToolUse(string name, JsonObject? input = null)
    {
        return new LlmToolUseBlock(Guid.NewGuid().ToString(), name, input ?? new JsonObject());
    }

    #region Tool execution failure

    [Fact]
    public async Task ProcessToolCalls_ToolThrows_MarksDecisionFailed()
    {
        var session = CreateSession();
        var toolUse = CreateToolUse("dispatch_trip", new JsonObject
        {
            ["trip_id"] = Guid.NewGuid().ToString()
        });

        toolExecutor.ExecuteToolAsync("dispatch_trip", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Trip is not in Draft status"));

        var results = await sut.ProcessToolCallsAsync(
            session, DispatchAgentMode.Autonomous, [toolUse], null, CancellationToken.None);

        Assert.Single(results);

        await decisionRepo.Received(1).AddAsync(
            Arg.Is<DispatchDecision>(d =>
                d.Status == DispatchDecisionStatus.Failed &&
                d.ToolOutput!.Contains("Trip is not in Draft status")),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region Decision type mapping

    [Theory]
    [InlineData("assign_load_to_truck", DispatchDecisionType.AssignLoad)]
    [InlineData("create_trip", DispatchDecisionType.CreateTrip)]
    [InlineData("dispatch_trip", DispatchDecisionType.DispatchTrip)]
    [InlineData("book_loadboard_load", DispatchDecisionType.BookLoadBoardLoad)]
    [InlineData("get_available_trucks", DispatchDecisionType.Query)]
    [InlineData("check_hos_feasibility", DispatchDecisionType.Query)]
    [InlineData("unknown_tool", DispatchDecisionType.Query)]
    public async Task ProcessToolCalls_MapsCorrectDecisionType(string toolName, DispatchDecisionType expectedType)
    {
        var session = CreateSession(DispatchAgentMode.HumanInTheLoop);
        var toolUse = CreateToolUse(toolName);

        toolExecutor.ExecuteToolAsync(toolName, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("{}");

        await sut.ProcessToolCallsAsync(
            session, DispatchAgentMode.HumanInTheLoop, [toolUse], null, CancellationToken.None);

        await decisionRepo.Received(1).AddAsync(
            Arg.Is<DispatchDecision>(d => d.Type == expectedType),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region IsWriteTool

    [Theory]
    [InlineData("assign_load_to_truck", true)]
    [InlineData("create_trip", true)]
    [InlineData("dispatch_trip", true)]
    [InlineData("book_loadboard_load", true)]
    [InlineData("get_available_trucks", false)]
    [InlineData("calculate_distance", false)]
    public void IsWriteTool_ClassifiesCorrectly(string toolName, bool expected)
    {
        Assert.Equal(expected, DispatchDecisionProcessor.IsWriteTool(toolName));
    }

    #endregion

    #region Mode-aware execution

    [Fact]
    public async Task ProcessToolCalls_WriteTool_HumanInTheLoop_CreatesSuggestion()
    {
        var session = CreateSession(DispatchAgentMode.HumanInTheLoop);
        var toolUse = CreateToolUse("assign_load_to_truck", new JsonObject
        {
            ["load_id"] = Guid.NewGuid().ToString(),
            ["truck_id"] = Guid.NewGuid().ToString()
        });

        var results = await sut.ProcessToolCallsAsync(
            session, DispatchAgentMode.HumanInTheLoop, [toolUse], "reasoning", CancellationToken.None);

        Assert.Single(results);

        // Tool executor should NOT be called - suggestion only
        await toolExecutor.DidNotReceive().ExecuteToolAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());

        // Decision should be persisted
        await decisionRepo.Received(1).AddAsync(
            Arg.Is<DispatchDecision>(d =>
                d.Status == DispatchDecisionStatus.Suggested &&
                d.ToolName == "assign_load_to_truck"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessToolCalls_WriteTool_Autonomous_ExecutesImmediately()
    {
        var session = CreateSession();
        var toolUse = CreateToolUse("create_trip", new JsonObject
        {
            ["truck_id"] = Guid.NewGuid().ToString(),
            ["load_ids"] = new JsonArray(Guid.NewGuid().ToString())
        });

        toolExecutor.ExecuteToolAsync("create_trip", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("{\"success\": true}");

        var results = await sut.ProcessToolCallsAsync(
            session, DispatchAgentMode.Autonomous, [toolUse], null, CancellationToken.None);

        Assert.Single(results);

        await toolExecutor.Received(1).ExecuteToolAsync(
            "create_trip", Arg.Any<string>(), Arg.Any<CancellationToken>());

        await decisionRepo.Received(1).AddAsync(
            Arg.Is<DispatchDecision>(d =>
                d.Status == DispatchDecisionStatus.Executed &&
                d.ExecutedAt != null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessToolCalls_ReadTool_ExecutesRegardlessOfMode()
    {
        var session = CreateSession(DispatchAgentMode.HumanInTheLoop);
        var toolUse = CreateToolUse("get_available_trucks");

        toolExecutor.ExecuteToolAsync("get_available_trucks", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("{\"total_trucks\": 5}");

        var results = await sut.ProcessToolCallsAsync(
            session, DispatchAgentMode.HumanInTheLoop, [toolUse], null, CancellationToken.None);

        Assert.Single(results);

        // Read tools always execute, even in HumanInTheLoop mode
        await toolExecutor.Received(1).ExecuteToolAsync(
            "get_available_trucks", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region Entity ID extraction

    [Fact]
    public async Task ProcessToolCalls_ExtractsEntityIds_FromToolInput()
    {
        var session = CreateSession();
        var loadId = Guid.NewGuid();
        var truckId = Guid.NewGuid();

        var toolUse = CreateToolUse("assign_load_to_truck", new JsonObject
        {
            ["load_id"] = loadId.ToString(),
            ["truck_id"] = truckId.ToString()
        });

        toolExecutor.ExecuteToolAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("{\"success\": true}");

        await sut.ProcessToolCallsAsync(
            session, DispatchAgentMode.Autonomous, [toolUse], null, CancellationToken.None);

        await decisionRepo.Received(1).AddAsync(
            Arg.Is<DispatchDecision>(d =>
                d.LoadId == loadId && d.TruckId == truckId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessToolCalls_InvalidGuidInInput_DoesNotSetEntityId()
    {
        var session = CreateSession();
        var toolUse = CreateToolUse("assign_load_to_truck", new JsonObject
        {
            ["load_id"] = "not-a-guid",
            ["truck_id"] = "also-not-a-guid"
        });

        toolExecutor.ExecuteToolAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("{\"success\": true}");

        await sut.ProcessToolCallsAsync(
            session, DispatchAgentMode.Autonomous, [toolUse], null, CancellationToken.None);

        await decisionRepo.Received(1).AddAsync(
            Arg.Is<DispatchDecision>(d =>
                d.LoadId == null && d.TruckId == null),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region Decision count and reasoning

    [Fact]
    public async Task ProcessToolCalls_IncrementsDecisionCount()
    {
        var session = CreateSession();

        toolExecutor.ExecuteToolAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("{}");

        var tools = new List<LlmToolUseBlock>
        {
            CreateToolUse("get_available_trucks"),
            CreateToolUse("get_unassigned_loads"),
            CreateToolUse("get_available_trucks")
        };

        await sut.ProcessToolCallsAsync(
            session, DispatchAgentMode.Autonomous, tools, "analyzing fleet", CancellationToken.None);

        Assert.Equal(3, session.DecisionCount);
    }

    [Fact]
    public async Task ProcessToolCalls_StoresReasoning()
    {
        var session = CreateSession();
        var toolUse = CreateToolUse("get_available_trucks");

        toolExecutor.ExecuteToolAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("{}");

        await sut.ProcessToolCallsAsync(
            session, DispatchAgentMode.Autonomous, [toolUse], "Let me check the fleet status", CancellationToken.None);

        await decisionRepo.Received(1).AddAsync(
            Arg.Is<DispatchDecision>(d => d.Reasoning == "Let me check the fleet status"),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region Broadcasting

    [Fact]
    public async Task ProcessToolCalls_WriteTool_Autonomous_BroadcastsDecision()
    {
        var session = CreateSession();
        var toolUse = CreateToolUse("assign_load_to_truck", new JsonObject
        {
            ["load_id"] = Guid.NewGuid().ToString(),
            ["truck_id"] = Guid.NewGuid().ToString()
        });

        toolExecutor.ExecuteToolAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("{\"success\": true}");

        await sut.ProcessToolCallsAsync(
            session, DispatchAgentMode.Autonomous, [toolUse], null, CancellationToken.None);

        await broadcastService.Received(1).BroadcastDecisionAsync(
            Arg.Any<Guid>(), Arg.Any<DispatchDecisionDto>());
    }

    [Fact]
    public async Task ProcessToolCalls_ReadTool_BroadcastsDecision()
    {
        var session = CreateSession();
        var toolUse = CreateToolUse("get_available_trucks");

        toolExecutor.ExecuteToolAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("{}");

        await sut.ProcessToolCallsAsync(
            session, DispatchAgentMode.Autonomous, [toolUse], null, CancellationToken.None);

        await broadcastService.Received(1).BroadcastDecisionAsync(
            Arg.Any<Guid>(), Arg.Any<DispatchDecisionDto>());
    }

    #endregion
}
