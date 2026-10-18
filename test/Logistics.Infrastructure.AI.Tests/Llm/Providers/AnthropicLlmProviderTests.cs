using System.Text.Json;
using Anthropic.Models.Messages;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Llm.Contracts;
using Logistics.Infrastructure.AI.Llm.Providers;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Llm.Providers;

/// <summary>
/// Responses are built from wire JSON and requests are asserted as wire JSON, so these pin what the
/// API sees rather than the SDK's object model.
/// </summary>
public class AnthropicLlmProviderTests
{
    private static Message Response(string stopReason, params string[] blocks) =>
        JsonSerializer.Deserialize<Message>($$"""
            {
              "id": "msg_1", "type": "message", "role": "assistant", "model": "claude-sonnet-5-5",
              "stop_reason": "{{stopReason}}", "stop_sequence": null,
              "content": [{{string.Join(",", blocks)}}],
              "usage": { "input_tokens": 10, "output_tokens": 5, "cache_read_input_tokens": 3, "cache_creation_input_tokens": 2 }
            }
            """)!;

    private const string ToolUse = """{"type":"tool_use","id":"call_1","name":"get_load","input":{"load_id":"abc"}}""";

    private static string Text(string text) => $$"""{"type":"text","text":"{{text}}"}""";

    private static JsonElement Wire(object value) => JsonSerializer.SerializeToElement(value);

    #region Response mapping

    [Fact]
    public void MapResponse_MultipleTextBlocks_JoinsAllIntoTextContent()
    {
        var result = AnthropicLlmProvider.MapResponse(
            Response("end_turn", Text("Let me check the loads."), ToolUse, Text("Found 3 delivered loads.")));

        Assert.Equal("Let me check the loads.\n\nFound 3 delivered loads.", result.TextContent);
    }

    [Fact]
    public void MapResponse_InterleavedBlocks_PreservesOrder()
    {
        var result = AnthropicLlmProvider.MapResponse(Response("tool_use", Text("first"), ToolUse, Text("second")));

        Assert.Collection(result.AssistantMessage.Content,
            b => Assert.Equal("first", Assert.IsType<LlmTextBlock>(b).Text),
            b => Assert.Equal("call_1", Assert.IsType<LlmToolUseBlock>(b).Id),
            b => Assert.Equal("second", Assert.IsType<LlmTextBlock>(b).Text));
        var call = Assert.Single(result.ToolCalls);
        Assert.Equal("abc", call.Input!["load_id"]!.GetValue<string>());
        Assert.Equal(LlmStopReason.ToolUse, result.StopReason);
    }

    [Fact]
    public void MapResponse_NoTextBlocks_TextContentIsNull()
    {
        var result = AnthropicLlmProvider.MapResponse(Response("tool_use", ToolUse));

        Assert.Null(result.TextContent);
    }

    [Fact]
    public void MapResponse_ReportsCacheUsageBuckets()
    {
        var usage = AnthropicLlmProvider.MapResponse(Response("end_turn", Text("hi"))).Usage;

        Assert.Equal(new LlmTokenUsage(10, 5, 3, 2), usage);
    }

    [Theory]
    [InlineData("end_turn", nameof(LlmStopReason.EndTurn))]
    [InlineData("tool_use", nameof(LlmStopReason.ToolUse))]
    [InlineData("max_tokens", nameof(LlmStopReason.MaxTokens))]
    [InlineData("pause_turn", nameof(LlmStopReason.PauseTurn))]
    [InlineData("refusal", nameof(LlmStopReason.Refusal))]
    public void MapResponse_MapsStopReason(string wire, string expected)
    {
        Assert.Equal(Enum.Parse<LlmStopReason>(expected), AnthropicLlmProvider.MapResponse(Response(wire, Text("x"))).StopReason);
    }

    #endregion

    #region Replay

    [Fact]
    public void ThinkingBlock_RoundTripsWithItsSignature()
    {
        // With thinking on, an assistant turn replayed without its thinking block is rejected.
        var mapped = AnthropicLlmProvider.MapResponse(Response("tool_use",
            """{"type":"thinking","thinking":"planning...","signature":"sig123"}""", ToolUse));

        var block = Wire(AnthropicLlmProvider.ToMessageParam(mapped.AssistantMessage)).GetProperty("content")[0];

        Assert.Equal("thinking", block.GetProperty("type").GetString());
        Assert.Equal("planning...", block.GetProperty("thinking").GetString());
        Assert.Equal("sig123", block.GetProperty("signature").GetString());
    }

    [Fact]
    public void RedactedThinkingBlock_RoundTripsAsRedacted()
    {
        // Replaying it as a plain thinking block sends the opaque payload in the wrong field.
        var mapped = AnthropicLlmProvider.MapResponse(Response("tool_use",
            """{"type":"redacted_thinking","data":"encrypted-payload"}""", ToolUse));

        var block = Wire(AnthropicLlmProvider.ToMessageParam(mapped.AssistantMessage)).GetProperty("content")[0];

        Assert.IsType<LlmRedactedThinkingBlock>(mapped.AssistantMessage.Content[0]);
        Assert.Equal("redacted_thinking", block.GetProperty("type").GetString());
        Assert.Equal("encrypted-payload", block.GetProperty("data").GetString());
    }

    [Fact]
    public void ServerToolBlocks_ReplayVerbatimInPlace()
    {
        // A pause_turn resumes only if the server_tool_use comes back, and a later thinking block
        // was signed over a prefix that contains these blocks.
        var mapped = AnthropicLlmProvider.MapResponse(Response("pause_turn",
            """{"type":"server_tool_use","id":"srvtoolu_1","name":"web_search","input":{"query":"I-40 closures"}}""",
            """{"type":"web_search_tool_result","tool_use_id":"srvtoolu_1","content":[{"type":"web_search_result","url":"https://example.com","title":"Closures","encrypted_content":"enc","page_age":null}]}""",
            Text("Searching.")));

        Assert.Collection(mapped.AssistantMessage.Content,
            b => Assert.IsType<LlmProviderBlock>(b),
            b => Assert.IsType<LlmProviderBlock>(b),
            b => Assert.IsType<LlmTextBlock>(b));

        var content = Wire(AnthropicLlmProvider.ToMessageParam(mapped.AssistantMessage)).GetProperty("content");
        Assert.Equal("server_tool_use", content[0].GetProperty("type").GetString());
        Assert.Equal("srvtoolu_1", content[0].GetProperty("id").GetString());
        Assert.Equal("web_search_tool_result", content[1].GetProperty("type").GetString());
        Assert.Equal("enc", content[1].GetProperty("content")[0].GetProperty("encrypted_content").GetString());
    }

    #endregion

    #region Prompt caching

    [Fact]
    public void BuildMessages_MarksOnlyTheNewestMessageForCaching()
    {
        // The breakpoint has to move each iteration: it only looks back 20 content blocks for an
        // existing entry, and one turn with several parallel tool calls can add more than that.
        var mapped = Wire(AnthropicLlmProvider.BuildMessages(
        [
            LlmMessage.FromUser("first"),
            LlmMessage.FromUser("second"),
            LlmMessage.FromToolResults([new LlmToolResultBlock("call_1", "done")])
        ]));

        Assert.False(mapped[0].GetProperty("content")[0].TryGetProperty("cache_control", out _));
        Assert.False(mapped[1].GetProperty("content")[0].TryGetProperty("cache_control", out _));
        Assert.Equal("ephemeral",
            mapped[2].GetProperty("content")[0].GetProperty("cache_control").GetProperty("type").GetString());
    }

    [Fact]
    public void BuildMessages_CachesTheLastBlockOfAMultiBlockTurn()
    {
        // The prefix only caches up to the breakpoint, so marking anything but the last block
        // leaves the rest re-billed.
        var content = Wire(AnthropicLlmProvider.BuildMessages(
        [
            LlmMessage.FromToolResults([new LlmToolResultBlock("call_1", "a"), new LlmToolResultBlock("call_2", "b")])
        ]))[0].GetProperty("content");

        Assert.False(content[0].TryGetProperty("cache_control", out _));
        Assert.True(content[1].TryGetProperty("cache_control", out _));
    }

    [Fact]
    public void BuildParameters_CachesTheSystemPrompt()
    {
        var system = Wire(AnthropicLlmProvider.BuildParameters(Request("claude-sonnet-5-5", ReasoningEffort.High)).System!);

        Assert.Equal("ephemeral", system[0].GetProperty("cache_control").GetProperty("type").GetString());
    }

    #endregion

    #region Reasoning effort

    private static LlmRequest Request(string model, ReasoningEffort effort) => new()
    {
        SystemPrompt = "system",
        Messages = [LlmMessage.FromUser("go")],
        Tools = [],
        Model = model,
        MaxTokens = 1000,
        Temperature = 0m,
        Effort = effort
    };

    [Theory]
    [InlineData(ReasoningEffort.None, "low")] // the 5.5 models have no clean "off"
    [InlineData(ReasoningEffort.Low, "low")]
    [InlineData(ReasoningEffort.Medium, "medium")]
    [InlineData(ReasoningEffort.High, "high")]
    [InlineData(ReasoningEffort.XHigh, "xhigh")]
    [InlineData(ReasoningEffort.Max, "max")]
    public void BuildParameters_AdaptiveModel_SendsAdaptiveThinkingAndEffort(ReasoningEffort effort, string expected)
    {
        var parameters = AnthropicLlmProvider.BuildParameters(Request("claude-sonnet-5-5", effort));

        Assert.Equal("adaptive", Wire(parameters.Thinking!).GetProperty("type").GetString());
        Assert.Equal(expected, Wire(parameters.OutputConfig!).GetProperty("effort").GetString());
    }

    [Fact]
    public void BuildParameters_AdaptiveModel_NeverSendsTemperature()
    {
        // The 5.5 models reject non-default sampling parameters outright.
        var parameters = AnthropicLlmProvider.BuildParameters(Request("claude-sonnet-5-5", ReasoningEffort.None));

        Assert.Null(parameters.Temperature);
    }

    [Fact]
    public void BuildParameters_NonReasoningModel_SendsNoThinkingAndKeepsTemperature()
    {
        var parameters = AnthropicLlmProvider.BuildParameters(Request("unknown-model", ReasoningEffort.High));

        Assert.Null(parameters.Thinking);
        Assert.Null(parameters.OutputConfig);
        Assert.Equal(0d, parameters.Temperature);
    }

    #endregion

    #region Web search

    [Theory]
    [InlineData("claude-sonnet-5-5", "web_search_20260209")]
    [InlineData("claude-haiku-5-5", "web_search_20250305")]
    public void BuildTools_AppendsTheWebSearchVariantTheModelAccepts(string model, string expectedType)
    {
        var tools = AnthropicLlmProvider.BuildTools(Request(model, ReasoningEffort.None));

        var tool = Wire(Assert.Single(tools));
        Assert.Equal(expectedType, tool.GetProperty("type").GetString());
        Assert.Equal(3, tool.GetProperty("max_uses").GetInt32());
    }

    #endregion
}
