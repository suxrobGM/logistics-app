using System.Text.Json;
using Logistics.Infrastructure.AI.Tools;
using Logistics.Infrastructure.AI.Tools.Dispatch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools;

public class AgentToolExecutorTests
{
    /// <summary>calculate_distance has no dependencies, so the whole path can run for real.</summary>
    private static AgentToolExecutor Executor()
    {
        var services = new ServiceCollection().AddScoped<CalculateDistanceTool>();
        return new AgentToolExecutor(services.BuildServiceProvider(), NullLogger<AgentToolExecutor>.Instance);
    }

    [Fact]
    public async Task ExecuteToolAsync_KnownTool_ResolvesItAndPassesTheArguments()
    {
        var result = await Executor().ExecuteToolAsync(
            "calculate_distance",
            """{"origin_lat": 34.05, "origin_lng": -118.24, "dest_lat": 32.71, "dest_lng": -117.16}""",
            CancellationToken.None);

        var json = JsonDocument.Parse(result).RootElement;
        Assert.True(json.GetProperty("straight_line_km").GetDouble() > 150);
        Assert.True(json.GetProperty("estimated_minutes").GetInt32() > 0);
    }

    [Fact]
    public async Task ExecuteToolAsync_UnknownTool_ReturnsError()
    {
        var result = await Executor().ExecuteToolAsync("nonexistent_tool", "{}", CancellationToken.None);

        Assert.Contains("nonexistent_tool", ErrorOf(result));
    }

    [Fact]
    public async Task ExecuteToolAsync_MalformedJson_ReturnsErrorRatherThanThrowing()
    {
        var result = await Executor().ExecuteToolAsync("calculate_distance", "{not json", CancellationToken.None);

        Assert.Contains("valid JSON", ErrorOf(result));
    }

    [Fact]
    public async Task ExecuteToolAsync_MissingRequiredArgument_NamesIt()
    {
        var result = await Executor().ExecuteToolAsync(
            "calculate_distance", """{"origin_lat": 34.05}""", CancellationToken.None);

        Assert.Contains("origin_lng", ErrorOf(result));
    }

    private static string ErrorOf(string result) =>
        JsonDocument.Parse(result).RootElement.GetProperty("error").GetString()!;
}
