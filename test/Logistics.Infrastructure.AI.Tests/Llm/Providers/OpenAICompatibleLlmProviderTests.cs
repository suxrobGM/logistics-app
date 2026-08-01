using System.Text.Json.Nodes;
using Logistics.Infrastructure.AI.Llm.Contracts;
using Logistics.Infrastructure.AI.Llm.Providers;
using OpenAI.Chat;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Llm.Providers;

/// <summary>
/// Fails at the API boundary, not the build: compatible endpoints 400 unless every tool_call_id
/// gets its own tool message, while the agent loop hands over one message holding all results.
/// </summary>
public class OpenAICompatibleLlmProviderTests
{
    private static LlmMessage AssistantWithToolCalls(params string[] ids) =>
        new(LlmRole.Assistant,
            [.. ids.Select(id => new LlmToolUseBlock(id, $"tool_{id}", JsonNode.Parse("""{"a":1}""")))]);

    [Fact]
    public void ToOpenAIMessages_EmitsOneToolMessagePerResult()
    {
        var message = LlmMessage.FromToolResults(
        [
            new LlmToolResultBlock("call_1", """{"loads":1}"""),
            new LlmToolResultBlock("call_2", """{"trucks":5}"""),
            new LlmToolResultBlock("call_3", """{"violations":1}""")
        ]);

        var result = OpenAICompatibleLlmProvider.ToOpenAIMessages(message).ToList();

        Assert.Equal(3, result.Count);
        Assert.All(result, m => Assert.IsType<ToolChatMessage>(m));
        Assert.Equal(
            ["call_1", "call_2", "call_3"],
            result.Cast<ToolChatMessage>().Select(m => m.ToolCallId));
    }

    [Fact]
    public void ToOpenAIMessages_ToolResultCountMatchesToolCallCount()
    {
        // The exact invariant the API enforces: every tool_call_id the assistant issued gets answered.
        var assistant = AssistantWithToolCalls("call_1", "call_2");
        var results = LlmMessage.FromToolResults(
            [new LlmToolResultBlock("call_1", "{}"), new LlmToolResultBlock("call_2", "{}")]);

        var assistantMessages = OpenAICompatibleLlmProvider.ToOpenAIMessages(assistant).ToList();
        var toolMessages = OpenAICompatibleLlmProvider.ToOpenAIMessages(results).ToList();

        var issued = Assert.IsType<AssistantChatMessage>(Assert.Single(assistantMessages)).ToolCalls;
        var answered = toolMessages.Cast<ToolChatMessage>().Select(m => m.ToolCallId).ToList();

        Assert.Equal(issued.Select(c => c.Id), answered);
    }

    [Fact]
    public void ToOpenAIMessages_SingleToolResultStillEmitsOneToolMessage()
    {
        var message = LlmMessage.FromToolResults([new LlmToolResultBlock("call_1", "{}")]);

        var result = Assert.Single(OpenAICompatibleLlmProvider.ToOpenAIMessages(message));

        Assert.Equal("call_1", Assert.IsType<ToolChatMessage>(result).ToolCallId);
    }

    [Fact]
    public void ToOpenAIMessages_MapsPlainUserTextToOneUserMessage()
    {
        var message = LlmMessage.FromUser("Dispatch the unassigned loads.");

        var result = Assert.Single(OpenAICompatibleLlmProvider.ToOpenAIMessages(message));

        var user = Assert.IsType<UserChatMessage>(result);
        Assert.Equal("Dispatch the unassigned loads.", Assert.Single(user.Content).Text);
    }

    [Fact]
    public void ToOpenAIMessages_MapsAssistantToolCallsToOneMessageCarryingEveryCall()
    {
        // Contrast with the Responses provider, where each call is its own top-level item.
        var result = Assert.Single(OpenAICompatibleLlmProvider.ToOpenAIMessages(
            AssistantWithToolCalls("call_1", "call_2")));

        var assistant = Assert.IsType<AssistantChatMessage>(result);
        Assert.Equal(["call_1", "call_2"], assistant.ToolCalls.Select(c => c.Id));
    }

    [Fact]
    public void ToOpenAIMessages_MapsAssistantWithoutToolCallsToOneMessage()
    {
        var message = new LlmMessage(LlmRole.Assistant, [new LlmTextBlock("All loads are assigned.")]);

        var result = Assert.Single(OpenAICompatibleLlmProvider.ToOpenAIMessages(message));

        Assert.IsType<AssistantChatMessage>(result);
    }

    #region Token usage

    [Fact]
    public void Usage_SubtractsCachedTokensFromInput()
    {
        // OpenAI's input_tokens INCLUDES cached_tokens and LlmPricing adds the two buckets, so
        // passing both through raw bills the cache twice.
        var usage = OpenAIUsage.From(inputTokens: 1000, cachedTokens: 800, outputTokens: 50);

        Assert.Equal(200, usage.InputTokens);
        Assert.Equal(800, usage.CacheReadTokens);
        Assert.Equal(50, usage.OutputTokens);
        Assert.Equal(0, usage.CacheCreationTokens);
    }

    [Fact]
    public void Usage_NoCachedTokens_LeavesInputIntact()
    {
        var usage = OpenAIUsage.From(inputTokens: 1000, cachedTokens: 0, outputTokens: 50);

        Assert.Equal(1000, usage.InputTokens);
        Assert.Equal(0, usage.CacheReadTokens);
    }

    [Fact]
    public void Usage_CachedExceedsInput_DoesNotGoNegative()
    {
        var usage = OpenAIUsage.From(inputTokens: 100, cachedTokens: 500, outputTokens: 10);

        Assert.Equal(0, usage.InputTokens);
        Assert.Equal(100, usage.CacheReadTokens);
    }

    #endregion
}
