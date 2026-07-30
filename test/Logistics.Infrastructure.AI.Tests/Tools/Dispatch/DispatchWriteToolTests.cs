using Logistics.Infrastructure.AI.Tools.Dispatch;
using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Modules.Operations.Loads.Commands;
using Logistics.Application.Modules.Operations.Trips.Commands;
using Logistics.Infrastructure.AI.Tools;
using Logistics.Shared.Models;
using MediatR;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools.Dispatch;

/// <summary>
/// Pins the JSON emitted by the three dispatch write tools. The agent is instructed to read
/// <c>success</c> and <c>error</c> by name, and the same payload is persisted on
/// <c>AIDispatchDecision.ToolOutput</c> - so these key names are a contract, not an implementation
/// detail.
/// </summary>
public class DispatchWriteToolTests
{
    private readonly IMediator mediator = Substitute.For<IMediator>();

    private static JsonElement Parse(string json) => JsonDocument.Parse(json).RootElement;

    #region assign_load_to_truck

    [Fact]
    public async Task AssignLoad_Success_EmitsSuccessWithBothIds()
    {
        var loadId = Guid.NewGuid();
        var truckId = Guid.NewGuid();
        mediator.Send(Arg.Any<AssignLoadToTruckCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok());

        var result = await new AssignLoadToTruckTool(mediator).ExecuteAsync(
            new JsonObject { ["load_id"] = loadId.ToString(), ["truck_id"] = truckId.ToString() },
            CancellationToken.None);

        var root = Parse(result);
        Assert.True(root.GetProperty("success").GetBoolean());
        Assert.Equal(loadId, root.GetProperty("load_id").GetGuid());
        Assert.Equal(truckId, root.GetProperty("truck_id").GetGuid());
    }

    [Fact]
    public async Task AssignLoad_CommandFails_EmitsSuccessFalseAndError()
    {
        mediator.Send(Arg.Any<AssignLoadToTruckCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail("Truck is not available"));

        var result = await new AssignLoadToTruckTool(mediator).ExecuteAsync(
            new JsonObject
            {
                ["load_id"] = Guid.NewGuid().ToString(),
                ["truck_id"] = Guid.NewGuid().ToString()
            },
            CancellationToken.None);

        var root = Parse(result);
        Assert.False(root.GetProperty("success").GetBoolean());
        Assert.Equal("Truck is not available", root.GetProperty("error").GetString());
    }

    [Theory]
    [InlineData("load_id", "Invalid or missing load_id")]
    [InlineData("truck_id", "Invalid or missing truck_id")]
    public async Task AssignLoad_MissingId_EmitsBareErrorWithoutSuccessKey(string missing, string expected)
    {
        var input = new JsonObject
        {
            ["load_id"] = Guid.NewGuid().ToString(),
            ["truck_id"] = Guid.NewGuid().ToString()
        };
        input.Remove(missing);

        var result = await new AssignLoadToTruckTool(mediator).ExecuteAsync(input, CancellationToken.None);

        var root = Parse(result);
        Assert.Equal(expected, root.GetProperty("error").GetString());
        Assert.False(root.TryGetProperty("success", out _));
        await mediator.DidNotReceive().Send(Arg.Any<AssignLoadToTruckCommand>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region create_trip

    [Fact]
    public async Task CreateTrip_Success_EmitsTripId()
    {
        var tripId = Guid.NewGuid();
        var loadId = Guid.NewGuid();
        mediator.Send(Arg.Any<CreateTripCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.Ok(tripId));

        var result = await new CreateTripTool(mediator).ExecuteAsync(
            new JsonObject
            {
                ["truck_id"] = Guid.NewGuid().ToString(),
                ["load_ids"] = new JsonArray(loadId.ToString()),
                ["name"] = "Overnight run"
            },
            CancellationToken.None);

        var root = Parse(result);
        Assert.True(root.GetProperty("success").GetBoolean());
        Assert.Equal(tripId, root.GetProperty("trip_id").GetGuid());

        await mediator.Received(1).Send(
            Arg.Is<CreateTripCommand>(c => c.Name == "Overnight run" && c.AttachedLoadIds.Count() == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateTrip_NoName_DefaultsToAiGeneratedTrip()
    {
        mediator.Send(Arg.Any<CreateTripCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.Ok(Guid.NewGuid()));

        await new CreateTripTool(mediator).ExecuteAsync(
            new JsonObject
            {
                ["truck_id"] = Guid.NewGuid().ToString(),
                ["load_ids"] = new JsonArray(Guid.NewGuid().ToString())
            },
            CancellationToken.None);

        await mediator.Received(1).Send(
            Arg.Is<CreateTripCommand>(c => c.Name == "AI-Generated Trip"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateTrip_EmptyLoadIds_EmitsError()
    {
        var result = await new CreateTripTool(mediator).ExecuteAsync(
            new JsonObject
            {
                ["truck_id"] = Guid.NewGuid().ToString(),
                ["load_ids"] = new JsonArray()
            },
            CancellationToken.None);

        Assert.Equal("Missing or empty load_ids", Parse(result).GetProperty("error").GetString());
    }

    [Fact]
    public async Task CreateTrip_UnparseableLoadId_EmitsErrorNamingTheValue()
    {
        var result = await new CreateTripTool(mediator).ExecuteAsync(
            new JsonObject
            {
                ["truck_id"] = Guid.NewGuid().ToString(),
                ["load_ids"] = new JsonArray("not-a-guid")
            },
            CancellationToken.None);

        Assert.Contains("not-a-guid", Parse(result).GetProperty("error").GetString());
    }

    #endregion

    #region dispatch_trip

    [Fact]
    public async Task DispatchTrip_Success_EchoesTripId()
    {
        var tripId = Guid.NewGuid();
        mediator.Send(Arg.Any<DispatchTripCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok());

        var result = await new DispatchTripTool(mediator).ExecuteAsync(
            new JsonObject { ["trip_id"] = tripId.ToString() }, CancellationToken.None);

        var root = Parse(result);
        Assert.True(root.GetProperty("success").GetBoolean());
        Assert.Equal(tripId, root.GetProperty("trip_id").GetGuid());
    }

    [Fact]
    public async Task DispatchTrip_CommandFails_EmitsSuccessFalseAndError()
    {
        mediator.Send(Arg.Any<DispatchTripCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail("Trip has no loads"));

        var result = await new DispatchTripTool(mediator).ExecuteAsync(
            new JsonObject { ["trip_id"] = Guid.NewGuid().ToString() }, CancellationToken.None);

        var root = Parse(result);
        Assert.False(root.GetProperty("success").GetBoolean());
        Assert.Equal("Trip has no loads", root.GetProperty("error").GetString());
    }

    [Fact]
    public async Task DispatchTrip_MissingTripId_DoesNotDispatch()
    {
        var result = await new DispatchTripTool(mediator).ExecuteAsync(
            new JsonObject(), CancellationToken.None);

        Assert.Equal("Invalid or missing trip_id", Parse(result).GetProperty("error").GetString());
        await mediator.DidNotReceive().Send(Arg.Any<DispatchTripCommand>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region LLM-authored JSON is not always the type the schema asked for

    [Fact]
    public async Task AssignLoad_IdsEmittedAsNumbers_ReturnsErrorRatherThanThrowing()
    {
        // Models intermittently emit a bare number where the schema says string. That must surface
        // as a readable {error} the agent can act on, not an InvalidOperationException that the
        // decision processor turns into an opaque failed decision.
        var result = await new AssignLoadToTruckTool(mediator).ExecuteAsync(
            new JsonObject { ["load_id"] = 12345, ["truck_id"] = 67890 }, CancellationToken.None);

        Assert.Equal("Invalid or missing load_id", Parse(result).GetProperty("error").GetString());
    }

    [Fact]
    public async Task DispatchTrip_IdEmittedAsNumber_ReturnsErrorRatherThanThrowing()
    {
        var result = await new DispatchTripTool(mediator).ExecuteAsync(
            new JsonObject { ["trip_id"] = 42 }, CancellationToken.None);

        Assert.Equal("Invalid or missing trip_id", Parse(result).GetProperty("error").GetString());
    }

    [Fact]
    public async Task CreateTrip_NameEmittedAsNumber_CoercesInsteadOfThrowing()
    {
        mediator.Send(Arg.Any<CreateTripCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result<Guid>.Ok(Guid.NewGuid()));

        await new CreateTripTool(mediator).ExecuteAsync(
            new JsonObject
            {
                ["truck_id"] = Guid.NewGuid().ToString(),
                ["load_ids"] = new JsonArray(Guid.NewGuid().ToString()),
                ["name"] = 2026
            },
            CancellationToken.None);

        await mediator.Received(1).Send(
            Arg.Is<CreateTripCommand>(c => c.Name == "2026"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateTrip_LoadIdEmittedAsNumber_ReturnsErrorRatherThanThrowing()
    {
        var result = await new CreateTripTool(mediator).ExecuteAsync(
            new JsonObject
            {
                ["truck_id"] = Guid.NewGuid().ToString(),
                ["load_ids"] = new JsonArray(12345)
            },
            CancellationToken.None);

        Assert.Contains("12345", Parse(result).GetProperty("error").GetString());
        await mediator.DidNotReceive().Send(Arg.Any<CreateTripCommand>(), Arg.Any<CancellationToken>());
    }

    #endregion

    [Fact]
    public void Names_AreSnakeCase()
    {
        Assert.Equal("assign_load_to_truck", new AssignLoadToTruckTool(mediator).Name);
        Assert.Equal("create_trip", new CreateTripTool(mediator).Name);
        Assert.Equal("dispatch_trip", new DispatchTripTool(mediator).Name);
    }
}
