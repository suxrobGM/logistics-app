using System.Text.Json.Nodes;
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
using Logistics.Application.Abstractions.AiDispatch;

namespace Logistics.Infrastructure.AI.Tests.Services;

public class AiDispatchDecisionProcessorTests
{
    private readonly ITenantRepository<AiDispatchDecision, Guid> decisionRepo =
        Substitute.For<ITenantRepository<AiDispatchDecision, Guid>>();

    private readonly ILogger<AiDispatchDecisionProcessor> logger = NullLogger<AiDispatchDecisionProcessor>.Instance;

    private readonly AiDispatchDecisionProcessor sut;
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IAiDispatchToolExecutor toolExecutor = Substitute.For<IAiDispatchToolExecutor>();
    private readonly IAiDispatchBroadcastService broadcastService = Substitute.For<IAiDispatchBroadcastService>();

    public AiDispatchDecisionProcessorTests()
    {
        tenantUow.Repository<AiDispatchDecision>().Returns(decisionRepo);
        tenantUow.GetCurrentTenant().Returns(new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Test Tenant",
            ConnectionString = "test-connection",
            BillingEmail = "test@test.com",
            CompanyAddress = new() { Line1 = "123 Test St", City = "Test", State = "TX", ZipCode = "12345", Country = "US" }
        });
        sut = new AiDispatchDecisionProcessor(toolExecutor, tenantUow, broadcastService, logger);
    }

    private static AiDispatchSession CreateSession(AiDispatchMode mode = AiDispatchMode.Autonomous)
    {
        return new AiDispatchSession { Mode = mode, StartedAt = DateTime.UtcNow };
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
            session, AiDispatchMode.Autonomous, [toolUse], null, CancellationToken.None);

        Assert.Single(results);

        await decisionRepo.Received(1).AddAsync(
            Arg.Is<AiDispatchDecision>(d =>
                d.Status == AiDispatchDecisionStatus.Failed &&
                d.ToolOutput!.Contains("Trip is not in Draft status")),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region Decision type mapping

    [Theory]
    [InlineData("assign_load_to_truck", AiDispatchDecisionType.AssignLoad)]
    [InlineData("create_trip", AiDispatchDecisionType.CreateTrip)]
    [InlineData("dispatch_trip", AiDispatchDecisionType.DispatchTrip)]
    [InlineData("book_loadboard_load", AiDispatchDecisionType.BookLoadBoardLoad)]
    [InlineData("get_available_trucks", AiDispatchDecisionType.Query)]
    [InlineData("check_hos_feasibility", AiDispatchDecisionType.Query)]
    [InlineData("unknown_tool", AiDispatchDecisionType.Query)]
    public async Task ProcessToolCalls_MapsCorrectDecisionType(string toolName, AiDispatchDecisionType expectedType)
    {
        var session = CreateSession(AiDispatchMode.HumanInTheLoop);
        var toolUse = CreateToolUse(toolName);

        toolExecutor.ExecuteToolAsync(toolName, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("{}");

        await sut.ProcessToolCallsAsync(
            session, AiDispatchMode.HumanInTheLoop, [toolUse], null, CancellationToken.None);

        await decisionRepo.Received(1).AddAsync(
            Arg.Is<AiDispatchDecision>(d => d.Type == expectedType),
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
        Assert.Equal(expected, AiDispatchDecisionProcessor.IsWriteTool(toolName));
    }

    #endregion

    #region Mode-aware execution

    [Fact]
    public async Task ProcessToolCalls_WriteTool_HumanInTheLoop_CreatesSuggestion()
    {
        var session = CreateSession(AiDispatchMode.HumanInTheLoop);
        var toolUse = CreateToolUse("assign_load_to_truck", new JsonObject
        {
            ["load_id"] = Guid.NewGuid().ToString(),
            ["truck_id"] = Guid.NewGuid().ToString()
        });

        var results = await sut.ProcessToolCallsAsync(
            session, AiDispatchMode.HumanInTheLoop, [toolUse], "reasoning", CancellationToken.None);

        Assert.Single(results);

        // Tool executor should NOT be called - suggestion only
        await toolExecutor.DidNotReceive().ExecuteToolAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());

        // Decision should be persisted
        await decisionRepo.Received(1).AddAsync(
            Arg.Is<AiDispatchDecision>(d =>
                d.Status == AiDispatchDecisionStatus.Suggested &&
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
            session, AiDispatchMode.Autonomous, [toolUse], null, CancellationToken.None);

        Assert.Single(results);

        await toolExecutor.Received(1).ExecuteToolAsync(
            "create_trip", Arg.Any<string>(), Arg.Any<CancellationToken>());

        await decisionRepo.Received(1).AddAsync(
            Arg.Is<AiDispatchDecision>(d =>
                d.Status == AiDispatchDecisionStatus.Executed &&
                d.ExecutedAt != null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessToolCalls_ReadTool_ExecutesRegardlessOfMode()
    {
        var session = CreateSession(AiDispatchMode.HumanInTheLoop);
        var toolUse = CreateToolUse("get_available_trucks");

        toolExecutor.ExecuteToolAsync("get_available_trucks", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("{\"total_trucks\": 5}");

        var results = await sut.ProcessToolCallsAsync(
            session, AiDispatchMode.HumanInTheLoop, [toolUse], null, CancellationToken.None);

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
            session, AiDispatchMode.Autonomous, [toolUse], null, CancellationToken.None);

        await decisionRepo.Received(1).AddAsync(
            Arg.Is<AiDispatchDecision>(d =>
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
            session, AiDispatchMode.Autonomous, [toolUse], null, CancellationToken.None);

        await decisionRepo.Received(1).AddAsync(
            Arg.Is<AiDispatchDecision>(d =>
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
            session, AiDispatchMode.Autonomous, tools, "analyzing fleet", CancellationToken.None);

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
            session, AiDispatchMode.Autonomous, [toolUse], "Let me check the fleet status", CancellationToken.None);

        await decisionRepo.Received(1).AddAsync(
            Arg.Is<AiDispatchDecision>(d => d.Reasoning == "Let me check the fleet status"),
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
            session, AiDispatchMode.Autonomous, [toolUse], null, CancellationToken.None);

        await broadcastService.Received(1).BroadcastDecisionAsync(
            Arg.Any<Guid>(), Arg.Any<AiDispatchDecisionDto>());
    }

    [Fact]
    public async Task ProcessToolCalls_ReadTool_BroadcastsDecision()
    {
        var session = CreateSession();
        var toolUse = CreateToolUse("get_available_trucks");

        toolExecutor.ExecuteToolAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("{}");

        await sut.ProcessToolCallsAsync(
            session, AiDispatchMode.Autonomous, [toolUse], null, CancellationToken.None);

        await broadcastService.Received(1).BroadcastDecisionAsync(
            Arg.Any<Guid>(), Arg.Any<AiDispatchDecisionDto>());
    }

    #endregion
}
