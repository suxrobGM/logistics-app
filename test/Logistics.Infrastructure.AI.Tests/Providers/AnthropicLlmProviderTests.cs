using System.Text.Json.Nodes;
using Anthropic.SDK.Messaging;
using Logistics.Infrastructure.AI.Llm.Contracts;
using Logistics.Infrastructure.AI.Llm;
using Logistics.Infrastructure.AI.Llm.Providers;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Providers;

/// <summary>
/// Chat replies interleave text and tool calls: keeping only the first text block drops reply
/// content, and reordering the transcript changes what the model said.
/// </summary>
public class AnthropicLlmProviderTests
{
    [Fact]
    public void MapResponse_MultipleTextBlocks_JoinsAllIntoTextContent()
    {
        var response = new MessageResponse
        {
            Content =
            [
                new TextContent { Text = "Let me check the loads." },
                new ToolUseContent { Id = "call_1", Name = "search_loads", Input = JsonNode.Parse("{}") },
                new TextContent { Text = "Found 3 delivered loads." }
            ],
            StopReason = "end_turn"
        };

        var result = AnthropicLlmProvider.MapResponse(response);

        Assert.Equal("Let me check the loads.\n\nFound 3 delivered loads.", result.TextContent);
    }

    [Fact]
    public void MapResponse_InterleavedBlocks_PreservesOrder()
    {
        var response = new MessageResponse
        {
            Content =
            [
                new TextContent { Text = "first" },
                new ToolUseContent { Id = "call_1", Name = "get_load", Input = JsonNode.Parse("{}") },
                new TextContent { Text = "second" }
            ],
            StopReason = "tool_use"
        };

        var result = AnthropicLlmProvider.MapResponse(response);

        Assert.Collection(result.AssistantMessage.Content,
            b => Assert.Equal("first", Assert.IsType<LlmTextBlock>(b).Text),
            b => Assert.Equal("call_1", Assert.IsType<LlmToolUseBlock>(b).Id),
            b => Assert.Equal("second", Assert.IsType<LlmTextBlock>(b).Text));
        Assert.Equal("call_1", Assert.Single(result.ToolCalls).Id);
    }

    [Fact]
    public void MapResponse_NoTextBlocks_TextContentIsNull()
    {
        var response = new MessageResponse
        {
            Content = [new ToolUseContent { Id = "call_1", Name = "get_load", Input = JsonNode.Parse("{}") }],
            StopReason = "tool_use"
        };

        var result = AnthropicLlmProvider.MapResponse(response);

        Assert.Null(result.TextContent);
    }
}
