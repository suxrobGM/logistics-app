using System.Text.Json.Nodes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Llm.Contracts;
using Logistics.Infrastructure.AI.Llm;
using Logistics.Infrastructure.AI.Llm.Providers;
using OpenAI.Chat;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Llm.Providers;

/// <summary>
/// Fails at the API boundary, not the build: OpenAI 400s unless every tool_call_id gets its own tool
/// message. The agent loop hands over one message holding all results (Anthropic's shape).
/// </summary>
public class OpenAILlmProviderTests
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

        var result = OpenAILlmProvider.ToOpenAIMessages(message).ToList();

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

        var assistantMessages = OpenAILlmProvider.ToOpenAIMessages(assistant).ToList();
        var toolMessages = OpenAILlmProvider.ToOpenAIMessages(results).ToList();

        var issued = Assert.IsType<AssistantChatMessage>(Assert.Single(assistantMessages)).ToolCalls;
        var answered = toolMessages.Cast<ToolChatMessage>().Select(m => m.ToolCallId).ToList();

        Assert.Equal(issued.Select(c => c.Id), answered);
    }

    [Fact]
    public void ToOpenAIMessages_SingleToolResultStillEmitsOneToolMessage()
    {
        var message = LlmMessage.FromToolResults([new LlmToolResultBlock("call_1", "{}")]);

        var result = Assert.Single(OpenAILlmProvider.ToOpenAIMessages(message));

        Assert.Equal("call_1", Assert.IsType<ToolChatMessage>(result).ToolCallId);
    }

    [Fact]
    public void ToOpenAIMessages_MapsPlainUserTextToOneUserMessage()
    {
        var message = LlmMessage.FromUser("Dispatch the unassigned loads.");

        var result = Assert.Single(OpenAILlmProvider.ToOpenAIMessages(message));

        var user = Assert.IsType<UserChatMessage>(result);
        Assert.Equal("Dispatch the unassigned loads.", Assert.Single(user.Content).Text);
    }

    [Fact]
    public void ToOpenAIMessages_MapsAssistantToolCallsToOneMessageCarryingEveryCall()
    {
        var result = Assert.Single(OpenAILlmProvider.ToOpenAIMessages(
            AssistantWithToolCalls("call_1", "call_2")));

        var assistant = Assert.IsType<AssistantChatMessage>(result);
        Assert.Equal(["call_1", "call_2"], assistant.ToolCalls.Select(c => c.Id));
    }

    [Fact]
    public void ToOpenAIMessages_MapsAssistantWithoutToolCallsToOneMessage()
    {
        var message = new LlmMessage(LlmRole.Assistant, [new LlmTextBlock("All loads are assigned.")]);

        var result = Assert.Single(OpenAILlmProvider.ToOpenAIMessages(message));

        Assert.IsType<AssistantChatMessage>(result);
    }

    #region Reasoning effort

    private static LlmRequest Request(string model, ReasoningEffort effort) => new()
    {
        SystemPrompt = "system",
        Messages = [LlmMessage.FromUser("go")],
        Tools = [],
        Model = model,
        MaxTokens = 1000,
        Effort = effort
    };

    [Fact]
    public void BuildOptions_ReasoningModelWithEffortNone_SendsExplicitNone()
    {
        // Reasoning models must always get an explicit reasoning_effort: their server-side
        // default is rejected on chat completions once function tools are present.
        var options = OpenAILlmProvider.BuildOptions(Request("gpt-5.6-luna", ReasoningEffort.None));

        Assert.Equal(new ChatReasoningEffortLevel("none"), options.ReasoningEffortLevel);
    }

    [Theory]
    [InlineData(ReasoningEffort.Low, "low")]
    [InlineData(ReasoningEffort.Medium, "medium")]
    [InlineData(ReasoningEffort.High, "high")]
    [InlineData(ReasoningEffort.XHigh, "high")]
    [InlineData(ReasoningEffort.Max, "high")]
    public void BuildOptions_ReasoningModel_MapsAndClampsEffortLevels(ReasoningEffort effort, string expected)
    {
        var options = OpenAILlmProvider.BuildOptions(Request("gpt-5.6-terra", effort));

        Assert.Equal(new ChatReasoningEffortLevel(expected), options.ReasoningEffortLevel);
    }

    [Theory]
    [InlineData("deepseek-v4-flash")]
    [InlineData("deepseek-v4-pro")]
    [InlineData("unknown-model")]
    public void BuildOptions_NonReasoningModel_NeverSendsTheParameter(string model)
    {
        // Some OpenAI-compatible endpoints reject reasoning_effort outright.
        var options = OpenAILlmProvider.BuildOptions(Request(model, ReasoningEffort.High));

        Assert.Null(options.ReasoningEffortLevel);
    }

    #endregion
}
