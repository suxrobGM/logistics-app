using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Infrastructure.AI.Services;
using Logistics.Infrastructure.AI.Tools;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Services;

public class DispatchToolExecutorTests
{
    private readonly ILogger<DispatchToolExecutor> logger = NullLogger<DispatchToolExecutor>.Instance;

    private static IDispatchTool CreateMockTool(string name, string result)
    {
        var tool = Substitute.For<IDispatchTool>();
        tool.Name.Returns(name);
        tool.ExecuteAsync(Arg.Any<JsonNode>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(result));
        return tool;
    }

    [Fact]
    public async Task ExecuteToolAsync_KnownTool_DelegatesToTool()
    {
        var tool = CreateMockTool("test_tool", "{\"ok\": true}");
        var sut = new DispatchToolExecutor([tool], logger);

        var result = await sut.ExecuteToolAsync("test_tool", "{}", CancellationToken.None);

        Assert.Equal("{\"ok\": true}", result);
        await tool.Received(1).ExecuteAsync(Arg.Any<JsonNode>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteToolAsync_UnknownTool_ReturnsError()
    {
        var sut = new DispatchToolExecutor([], logger);

        var result = await sut.ExecuteToolAsync("nonexistent_tool", "{}", CancellationToken.None);

        var json = JsonDocument.Parse(result);
        Assert.True(json.RootElement.TryGetProperty("error", out var error));
        Assert.Contains("nonexistent_tool", error.GetString());
    }

    [Fact]
    public async Task ExecuteToolAsync_ParsesInputJsonAndPassesToTool()
    {
        JsonNode? capturedInput = null;
        var tool = Substitute.For<IDispatchTool>();
        tool.Name.Returns("my_tool");
        tool.ExecuteAsync(Arg.Any<JsonNode>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                capturedInput = callInfo.ArgAt<JsonNode>(0);
                return Task.FromResult("{}");
            });

        var sut = new DispatchToolExecutor([tool], logger);

        await sut.ExecuteToolAsync("my_tool", """{"key": "value"}""", CancellationToken.None);

        Assert.NotNull(capturedInput);
        Assert.Equal("value", capturedInput!["key"]?.GetValue<string>());
    }

    [Fact]
    public async Task ExecuteToolAsync_MultipleTool_DispatchesToCorrectOne()
    {
        var tool1 = CreateMockTool("tool_a", "result_a");
        var tool2 = CreateMockTool("tool_b", "result_b");

        var sut = new DispatchToolExecutor([tool1, tool2], logger);

        var result = await sut.ExecuteToolAsync("tool_b", "{}", CancellationToken.None);

        Assert.Equal("result_b", result);
        await tool1.DidNotReceive().ExecuteAsync(Arg.Any<JsonNode>(), Arg.Any<CancellationToken>());
        await tool2.Received(1).ExecuteAsync(Arg.Any<JsonNode>(), Arg.Any<CancellationToken>());
    }
}
