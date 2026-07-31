using System.Text.Json.Nodes;
using Anthropic.SDK.Messaging;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Llm.Contracts;
using Logistics.Infrastructure.AI.Llm;
using Logistics.Infrastructure.AI.Llm.Providers;
using Xunit;
using SdkEffort = Anthropic.SDK.Messaging.ThinkingEffort;

namespace Logistics.Infrastructure.AI.Tests.Llm.Providers;

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

    #region Thinking-block replay

    [Fact]
    public void MapResponse_ThinkingBlock_IsCapturedForReplay()
    {
        // With thinking on, an assistant turn replayed without its thinking block is rejected -
        // dropping it here would break every tool loop on an adaptive-thinking model.
        var response = new MessageResponse
        {
            Content =
            [
                new ThinkingContent { Thinking = "planning...", Signature = "sig123" },
                new ToolUseContent { Id = "call_1", Name = "get_load", Input = JsonNode.Parse("{}") }
            ],
            StopReason = "tool_use"
        };

        var result = AnthropicLlmProvider.MapResponse(response);

        Assert.Collection(result.AssistantMessage.Content,
            b =>
            {
                var thinking = Assert.IsType<LlmThinkingBlock>(b);
                Assert.Equal("planning...", thinking.Thinking);
                Assert.Equal("sig123", thinking.Signature);
            },
            b => Assert.IsType<LlmToolUseBlock>(b));
    }

    [Fact]
    public void ThinkingBlock_RoundTripsThroughMessageMapping()
    {
        var response = new MessageResponse
        {
            Content =
            [
                new ThinkingContent { Thinking = "planning...", Signature = "sig123" },
                new TextContent { Text = "Done." }
            ],
            StopReason = "end_turn"
        };

        var mapped = AnthropicLlmProvider.MapResponse(response);
        var replayed = AnthropicLlmProvider.ToAnthropicMessage(mapped.AssistantMessage);

        var thinking = Assert.IsType<ThinkingContent>(replayed.Content[0]);
        Assert.Equal("planning...", thinking.Thinking);
        Assert.Equal("sig123", thinking.Signature);
    }

    #endregion

    #region Prompt caching

    [Fact]
    public void BuildMessages_MarksOnlyTheNewestMessageForCaching()
    {
        // The breakpoint has to move each iteration: it only looks back 20 content blocks for an
        // existing entry, and one turn with several parallel tool calls can add more than that.
        var messages = new List<LlmMessage>
        {
            LlmMessage.FromUser("first"),
            LlmMessage.FromUser("second"),
            LlmMessage.FromToolResults([new LlmToolResultBlock("call_1", "done")])
        };

        var mapped = AnthropicLlmProvider.BuildMessages(messages);

        Assert.Null(mapped[0].Content[^1].CacheControl);
        Assert.Null(mapped[1].Content[^1].CacheControl);
        Assert.Equal(CacheControlType.ephemeral, mapped[^1].Content[^1].CacheControl?.Type);
    }

    [Fact]
    public void BuildMessages_CachesTheLastBlockOfAMultiBlockTurn()
    {
        // A tool-results turn carries one block per call; the prefix only caches up to the
        // breakpoint, so marking anything but the last block leaves the rest re-billed.
        var toolResults = LlmMessage.FromToolResults(
        [
            new LlmToolResultBlock("call_1", "a"),
            new LlmToolResultBlock("call_2", "b")
        ]);

        var mapped = AnthropicLlmProvider.BuildMessages([toolResults]);

        Assert.Null(mapped[0].Content[0].CacheControl);
        Assert.Equal(CacheControlType.ephemeral, mapped[0].Content[1].CacheControl?.Type);
    }

    [Fact]
    public void SystemPromptAndTranscript_StayWithinTheFourBreakpointLimit()
    {
        var parameters = AnthropicLlmProvider.BuildParameters(
            Request("claude-sonnet-5", ReasoningEffort.High));
        var mapped = AnthropicLlmProvider.BuildMessages([LlmMessage.FromUser("go")]);

        var breakpoints = parameters.System.Count(s => s.CacheControl is not null)
            + mapped.Sum(m => m.Content.Count(c => c.CacheControl is not null));

        Assert.Equal(2, breakpoints);
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
    [InlineData(ReasoningEffort.Low, SdkEffort.low)]
    [InlineData(ReasoningEffort.Medium, SdkEffort.medium)]
    [InlineData(ReasoningEffort.High, SdkEffort.high)]
    [InlineData(ReasoningEffort.XHigh, SdkEffort.high)] // SDK effort scale has no xhigh
    [InlineData(ReasoningEffort.Max, SdkEffort.max)]
    public void BuildParameters_AdaptiveModel_MapsEffortToAdaptiveThinking(
        ReasoningEffort effort, SdkEffort expected)
    {
        var parameters = AnthropicLlmProvider.BuildParameters(Request("claude-sonnet-5", effort));

        Assert.NotNull(parameters.Thinking);
        Assert.Equal(ThinkingType.adaptive, parameters.Thinking!.Type);
        Assert.Equal(expected, parameters.Thinking.Effort);
    }

    [Fact]
    public void BuildParameters_AdaptiveModel_EffortNone_OmitsThinking()
    {
        var parameters = AnthropicLlmProvider.BuildParameters(Request("claude-sonnet-5", ReasoningEffort.None));

        Assert.Null(parameters.Thinking);
    }

    [Fact]
    public void BuildParameters_AdaptiveModel_NeverSendsTemperature()
    {
        // Sonnet 5 rejects non-default sampling parameters outright.
        var parameters = AnthropicLlmProvider.BuildParameters(Request("claude-sonnet-5", ReasoningEffort.None));

        Assert.Null(parameters.Temperature);
    }

    [Theory]
    [InlineData("claude-haiku-4-5")]
    [InlineData("unknown-model")]
    public void BuildParameters_NonReasoningModel_SendsNoThinkingAndKeepsTemperature(string model)
    {
        var parameters = AnthropicLlmProvider.BuildParameters(Request(model, ReasoningEffort.High));

        Assert.Null(parameters.Thinking);
        Assert.Equal(0m, parameters.Temperature);
    }

    #endregion
}
