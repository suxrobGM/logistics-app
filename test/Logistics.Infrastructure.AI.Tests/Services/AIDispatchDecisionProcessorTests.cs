using Logistics.Infrastructure.AI.Services;
using Logistics.Infrastructure.AI.Agents;
using System.Text.Json.Nodes;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Llm.Contracts;
using Logistics.Infrastructure.AI.Llm;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using Logistics.Application.Abstractions.AIDispatch;

namespace Logistics.Infrastructure.AI.Tests.Services;

public class AIDispatchDecisionProcessorTests
{
    private readonly ITenantRepository<AIDispatchDecision, Guid> decisionRepo =
        Substitute.For<ITenantRepository<AIDispatchDecision, Guid>>();

    private readonly ILogger<AIDispatchDecisionProcessor> logger = NullLogger<AIDispatchDecisionProcessor>.Instance;

    private readonly AIDispatchDecisionProcessor sut;
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IAIDispatchToolExecutor toolExecutor = Substitute.For<IAIDispatchToolExecutor>();
    private readonly IAIDispatchBroadcastService broadcastService = Substitute.For<IAIDispatchBroadcastService>();
    private readonly IAIDispatchToolRegistry toolRegistry = new AIDispatchToolRegistry();

    public AIDispatchDecisionProcessorTests()
    {
        tenantUow.Repository<AIDispatchDecision>().Returns(decisionRepo);
        tenantUow.GetCurrentTenant().Returns(new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Test Tenant",
            ConnectionString = "test-connection",
            BillingEmail = "test@test.com",
            CompanyAddress = new() { Line1 = "123 Test St", City = "Test", State = "TX", ZipCode = "12345", Country = "US" }
        });
        sut = new AIDispatchDecisionProcessor(toolExecutor, toolRegistry, tenantUow, broadcastService, logger);
    }

    private static AIDispatchSession CreateSession(AIDispatchMode mode = AIDispatchMode.Autonomous)
    {
        return new AIDispatchSession { Mode = mode, StartedAt = DateTime.UtcNow };
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
            session, new ToolCallContext(AIDispatchMode.Autonomous), [toolUse], null, CancellationToken.None);

        Assert.Single(results);

        await decisionRepo.Received(1).AddAsync(
            Arg.Is<AIDispatchDecision>(d =>
                d.Status == AIDispatchDecisionStatus.Failed &&
                d.ToolOutput!.Contains("Trip is not in Draft status")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessToolCalls_ToolCancelled_PropagatesAndSkipsRemainingTools()
    {
        var session = CreateSession();
        var first = CreateToolUse("dispatch_trip", new JsonObject { ["trip_id"] = Guid.NewGuid().ToString() });
        var second = CreateToolUse("create_trip", new JsonObject { ["truck_id"] = Guid.NewGuid().ToString() });

        toolExecutor.ExecuteToolAsync("dispatch_trip", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            sut.ProcessToolCallsAsync(
                session, new ToolCallContext(AIDispatchMode.Autonomous), [first, second], null,
                CancellationToken.None));

        // A cancel must stop the batch, not degrade into a failed decision and carry on executing
        // the remaining write tools.
        await toolExecutor.DidNotReceive().ExecuteToolAsync(
            "create_trip", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessToolCalls_ToolThrowsAuthError_RecordsSanitizedMessage()
    {
        var session = CreateSession();
        var toolUse = CreateToolUse("dispatch_trip", new JsonObject
        {
            ["trip_id"] = Guid.NewGuid().ToString()
        });

        toolExecutor.ExecuteToolAsync("dispatch_trip", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException(
                "Login failed for user 'sa'; unauthorized at Host=db-prod-01;Password=hunter2"));

        var results = await sut.ProcessToolCallsAsync(
            session, new ToolCallContext(AIDispatchMode.Autonomous), [toolUse], null, CancellationToken.None);

        // The decision row is tenant-visible and the same text is fed back to the model.
        Assert.DoesNotContain("hunter2", results[0].Content);
        Assert.DoesNotContain("db-prod-01", results[0].Content);

        await decisionRepo.Received(1).AddAsync(
            Arg.Is<AIDispatchDecision>(d =>
                d.Status == AIDispatchDecisionStatus.Failed &&
                !d.ToolOutput!.Contains("hunter2")),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region Decision type mapping

    [Theory]
    [InlineData("assign_load_to_truck", AIDispatchDecisionType.AssignLoad)]
    [InlineData("create_trip", AIDispatchDecisionType.CreateTrip)]
    [InlineData("dispatch_trip", AIDispatchDecisionType.DispatchTrip)]
    [InlineData("book_loadboard_load", AIDispatchDecisionType.BookLoadBoardLoad)]
    [InlineData("get_available_trucks", AIDispatchDecisionType.Query)]
    [InlineData("check_hos_feasibility", AIDispatchDecisionType.Query)]
    [InlineData("unknown_tool", AIDispatchDecisionType.Query)]
    public async Task ProcessToolCalls_MapsCorrectDecisionType(string toolName, AIDispatchDecisionType expectedType)
    {
        var session = CreateSession(AIDispatchMode.HumanInTheLoop);
        var toolUse = CreateToolUse(toolName);

        toolExecutor.ExecuteToolAsync(toolName, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("{}");

        await sut.ProcessToolCallsAsync(
            session, new ToolCallContext(AIDispatchMode.HumanInTheLoop), [toolUse], null, CancellationToken.None);

        await decisionRepo.Received(1).AddAsync(
            Arg.Is<AIDispatchDecision>(d => d.Type == expectedType),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region Write-tool metadata

    // Lose IsWrite on any of these and HumanInTheLoop auto-executes it instead of suggesting it.
    [Theory]
    [InlineData("assign_load_to_truck", true)]
    [InlineData("create_trip", true)]
    [InlineData("dispatch_trip", true)]
    [InlineData("book_loadboard_load", true)]
    [InlineData("get_available_trucks", false)]
    [InlineData("calculate_distance", false)]
    public void Registry_ClassifiesWriteToolsCorrectly(string toolName, bool expected)
    {
        Assert.Equal(expected, toolRegistry.TryGetDefinition(toolName)!.IsWrite);
    }

    #endregion

    #region Permission-scoped runs

    [Fact]
    public async Task ProcessToolCalls_CallerLacksToolPermission_FailsWithoutExecuting()
    {
        var session = CreateSession(AIDispatchMode.HumanInTheLoop);
        var toolUse = CreateToolUse("get_available_trucks");
        var context = new ToolCallContext(AIDispatchMode.HumanInTheLoop, CallerPermissions: new HashSet<string>());

        var results = await sut.ProcessToolCallsAsync(session, context, [toolUse], null, CancellationToken.None);

        Assert.Contains("permission_denied", Assert.Single(results).Content);
        await toolExecutor.DidNotReceive().ExecuteToolAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await decisionRepo.Received(1).AddAsync(
            Arg.Is<AIDispatchDecision>(d => d.Status == AIDispatchDecisionStatus.Failed),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessToolCalls_CallerHasToolPermission_Executes()
    {
        var session = CreateSession(AIDispatchMode.HumanInTheLoop);
        var toolUse = CreateToolUse("get_available_trucks");
        var context = new ToolCallContext(
            AIDispatchMode.HumanInTheLoop,
            CallerPermissions: new HashSet<string> { "Permission.Dispatch.View" });

        toolExecutor.ExecuteToolAsync("get_available_trucks", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("{}");

        await sut.ProcessToolCallsAsync(session, context, [toolUse], null, CancellationToken.None);

        await toolExecutor.Received(1).ExecuteToolAsync(
            "get_available_trucks", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessToolCalls_DecisionBroadcastOverride_ReplacesDefaultBroadcast()
    {
        var session = CreateSession();
        var toolUse = CreateToolUse("get_available_trucks");
        var overridden = new List<AIDispatchDecisionDto>();
        var context = new ToolCallContext(
            AIDispatchMode.Autonomous,
            DecisionBroadcastOverride: dto =>
            {
                overridden.Add(dto);
                return Task.CompletedTask;
            });

        toolExecutor.ExecuteToolAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("{}");

        await sut.ProcessToolCallsAsync(session, context, [toolUse], null, CancellationToken.None);

        Assert.Single(overridden);
        await broadcastService.DidNotReceive().BroadcastDecisionAsync(
            Arg.Any<Guid>(), Arg.Any<AIDispatchDecisionDto>());
    }

    #endregion

    #region Mode-aware execution

    [Fact]
    public async Task ProcessToolCalls_WriteTool_HumanInTheLoop_CreatesSuggestion()
    {
        var session = CreateSession(AIDispatchMode.HumanInTheLoop);
        var toolUse = CreateToolUse("assign_load_to_truck", new JsonObject
        {
            ["load_id"] = Guid.NewGuid().ToString(),
            ["truck_id"] = Guid.NewGuid().ToString()
        });

        var results = await sut.ProcessToolCallsAsync(
            session, new ToolCallContext(AIDispatchMode.HumanInTheLoop), [toolUse], "reasoning", CancellationToken.None);

        Assert.Single(results);

        // Tool executor should NOT be called - suggestion only
        await toolExecutor.DidNotReceive().ExecuteToolAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());

        // Decision should be persisted
        await decisionRepo.Received(1).AddAsync(
            Arg.Is<AIDispatchDecision>(d =>
                d.Status == AIDispatchDecisionStatus.Suggested &&
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
            session, new ToolCallContext(AIDispatchMode.Autonomous), [toolUse], null, CancellationToken.None);

        Assert.Single(results);

        await toolExecutor.Received(1).ExecuteToolAsync(
            "create_trip", Arg.Any<string>(), Arg.Any<CancellationToken>());

        await decisionRepo.Received(1).AddAsync(
            Arg.Is<AIDispatchDecision>(d =>
                d.Status == AIDispatchDecisionStatus.Executed &&
                d.ExecutedAt != null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessToolCalls_ReadTool_ExecutesRegardlessOfMode()
    {
        var session = CreateSession(AIDispatchMode.HumanInTheLoop);
        var toolUse = CreateToolUse("get_available_trucks");

        toolExecutor.ExecuteToolAsync("get_available_trucks", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("{\"total_trucks\": 5}");

        var results = await sut.ProcessToolCallsAsync(
            session, new ToolCallContext(AIDispatchMode.HumanInTheLoop), [toolUse], null, CancellationToken.None);

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
            session, new ToolCallContext(AIDispatchMode.Autonomous), [toolUse], null, CancellationToken.None);

        await decisionRepo.Received(1).AddAsync(
            Arg.Is<AIDispatchDecision>(d =>
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
            session, new ToolCallContext(AIDispatchMode.Autonomous), [toolUse], null, CancellationToken.None);

        await decisionRepo.Received(1).AddAsync(
            Arg.Is<AIDispatchDecision>(d =>
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
            session, new ToolCallContext(AIDispatchMode.Autonomous), tools, "analyzing fleet", CancellationToken.None);

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
            session, new ToolCallContext(AIDispatchMode.Autonomous), [toolUse], "Let me check the fleet status", CancellationToken.None);

        await decisionRepo.Received(1).AddAsync(
            Arg.Is<AIDispatchDecision>(d => d.Reasoning == "Let me check the fleet status"),
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
            session, new ToolCallContext(AIDispatchMode.Autonomous), [toolUse], null, CancellationToken.None);

        await broadcastService.Received(1).BroadcastDecisionAsync(
            Arg.Any<Guid>(), Arg.Any<AIDispatchDecisionDto>());
    }

    [Fact]
    public async Task ProcessToolCalls_ReadTool_BroadcastsDecision()
    {
        var session = CreateSession();
        var toolUse = CreateToolUse("get_available_trucks");

        toolExecutor.ExecuteToolAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("{}");

        await sut.ProcessToolCallsAsync(
            session, new ToolCallContext(AIDispatchMode.Autonomous), [toolUse], null, CancellationToken.None);

        await broadcastService.Received(1).BroadcastDecisionAsync(
            Arg.Any<Guid>(), Arg.Any<AIDispatchDecisionDto>());
    }

    #endregion
}
