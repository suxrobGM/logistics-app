using System.Text.Json.Nodes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Llm.Contracts;
using Logistics.Infrastructure.AI.Llm.Providers;
using OpenAI.Responses;
using Xunit;

// OPENAI001: the SDK marks the Responses surface [Experimental]; the wire protocol is stable.
#pragma warning disable OPENAI001

namespace Logistics.Infrastructure.AI.Tests.Llm.Providers;

/// <summary>
/// Fails at the API boundary, not the build. Responses has no tool role: a call and its result are
/// each a top-level item keyed by call_id, so an assistant turn fans out into 1+N items.
/// </summary>
public class OpenAIResponsesLlmProviderTests
{
    private static LlmMessage AssistantWithToolCalls(params string[] ids) =>
        new(LlmRole.Assistant,
            [.. ids.Select(id => new LlmToolUseBlock(id, $"tool_{id}", JsonNode.Parse("""{"a":1}""")))]);

    [Fact]
    public void ToInputItems_EmitsOneFunctionCallOutputItemPerToolResult()
    {
        var message = LlmMessage.FromToolResults(
        [
            new LlmToolResultBlock("call_1", """{"loads":1}"""),
            new LlmToolResultBlock("call_2", """{"trucks":5}"""),
            new LlmToolResultBlock("call_3", """{"violations":1}""")
        ]);

        var result = OpenAIResponsesLlmProvider.ToInputItems(message).ToList();

        Assert.Equal(3, result.Count);
        Assert.All(result, i => Assert.IsType<FunctionCallOutputResponseItem>(i));
        Assert.Equal(
            ["call_1", "call_2", "call_3"],
            result.Cast<FunctionCallOutputResponseItem>().Select(i => i.CallId));
    }

    [Fact]
    public void ToInputItems_ToolResultsAreTopLevelItemsNotUserMessages()
    {
        // The point of the migration: wrapping results in a user message is the chat shape.
        var message = LlmMessage.FromToolResults([new LlmToolResultBlock("call_1", "{}")]);

        var result = OpenAIResponsesLlmProvider.ToInputItems(message).ToList();

        Assert.All(result, i => Assert.IsNotType<MessageResponseItem>(i, exactMatch: false));
    }

    [Fact]
    public void ToInputItems_ToolResultCallIdsMatchAssistantCallIds()
    {
        var assistant = AssistantWithToolCalls("call_1", "call_2");
        var results = LlmMessage.FromToolResults(
            [new LlmToolResultBlock("call_1", "{}"), new LlmToolResultBlock("call_2", "{}")]);

        var issued = OpenAIResponsesLlmProvider.ToInputItems(assistant)
            .OfType<FunctionCallResponseItem>().Select(i => i.CallId);
        var answered = OpenAIResponsesLlmProvider.ToInputItems(results)
            .Cast<FunctionCallOutputResponseItem>().Select(i => i.CallId);

        Assert.Equal(issued, answered);
    }

    [Fact]
    public void ToInputItems_MapsPlainUserTextToOneUserMessageItem()
    {
        var message = LlmMessage.FromUser("Dispatch the unassigned loads.");

        var result = Assert.Single(OpenAIResponsesLlmProvider.ToInputItems(message));

        var user = Assert.IsType<MessageResponseItem>(result, exactMatch: false);
        Assert.Equal("Dispatch the unassigned loads.", Assert.Single(user.Content).Text);
    }

    [Fact]
    public void ToInputItems_AssistantWithTwoToolCallsEmitsTwoFunctionCallItems()
    {
        // The inverse of the chat-completions case, where two calls rode on one message.
        var result = OpenAIResponsesLlmProvider.ToInputItems(
            AssistantWithToolCalls("call_1", "call_2")).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(
            ["call_1", "call_2"],
            result.Cast<FunctionCallResponseItem>().Select(i => i.CallId));
    }

    [Fact]
    public void ToInputItems_AssistantTextAndToolCalls_EmitsMessageThenCalls()
    {
        var message = new LlmMessage(LlmRole.Assistant,
        [
            new LlmTextBlock("Assigning the Denver load."),
            new LlmToolUseBlock("call_1", "assign_load", JsonNode.Parse("{}"))
        ]);

        var result = OpenAIResponsesLlmProvider.ToInputItems(message).ToList();

        Assert.Equal(2, result.Count);
        Assert.IsType<MessageResponseItem>(result[0], exactMatch: false);
        Assert.IsType<FunctionCallResponseItem>(result[1]);
    }

    [Fact]
    public void ToInputItems_AssistantToolCallWithoutText_EmitsNoEmptyMessageItem()
    {
        // An empty output_text part is a 400 here, where chat completions tolerated it.
        var result = OpenAIResponsesLlmProvider.ToInputItems(AssistantWithToolCalls("call_1"));

        Assert.IsType<FunctionCallResponseItem>(Assert.Single(result));
    }

    [Fact]
    public void ToInputItems_UserDocument_EmitsTextPartThenFilePart()
    {
        var message = new LlmMessage(LlmRole.User,
        [
            new LlmTextBlock("Extract the rate confirmation."),
            new LlmDocumentBlock("application/pdf", Convert.ToBase64String([1, 2, 3]))
        ]);

        var result = Assert.Single(OpenAIResponsesLlmProvider.ToInputItems(message));

        var parts = Assert.IsType<MessageResponseItem>(result, exactMatch: false).Content;
        Assert.Equal(2, parts.Count);
        Assert.Equal("Extract the rate confirmation.", parts[0].Text);
        Assert.Equal("document.pdf", parts[1].InputFilename);
    }

    [Fact]
    public void BuildInputItems_PutsTheSystemPromptFirst()
    {
        var result = OpenAIResponsesLlmProvider.BuildInputItems(
            Request("gpt-5.6-luna", ReasoningEffort.Low)).ToList();

        var first = Assert.IsType<MessageResponseItem>(result[0], exactMatch: false);
        Assert.Equal(MessageRole.Developer, first.Role);
    }

    #region Options

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
        var options = OpenAIResponsesLlmProvider.BuildOptions(Request("gpt-5.6-luna", ReasoningEffort.None));

        Assert.Equal(ResponseReasoningEffortLevel.None, options.ReasoningOptions?.ReasoningEffortLevel);
    }

    [Theory]
    [InlineData(ReasoningEffort.Low, "low")]
    [InlineData(ReasoningEffort.Medium, "medium")]
    [InlineData(ReasoningEffort.High, "high")]
    [InlineData(ReasoningEffort.XHigh, "high")]
    [InlineData(ReasoningEffort.Max, "high")]
    public void BuildOptions_ReasoningModel_MapsAndClampsEffortLevels(ReasoningEffort effort, string expected)
    {
        var options = OpenAIResponsesLlmProvider.BuildOptions(Request("gpt-5.6-terra", effort));

        Assert.Equal(
            new ResponseReasoningEffortLevel(expected),
            options.ReasoningOptions?.ReasoningEffortLevel);
    }

    [Theory]
    [InlineData("claude-haiku-4-5")]
    [InlineData("unknown-model")]
    public void BuildOptions_NonReasoningModel_SendsNoReasoningOptions(string model)
    {
        var options = OpenAIResponsesLlmProvider.BuildOptions(Request(model, ReasoningEffort.High));

        Assert.Null(options.ReasoningOptions);
    }

    [Fact]
    public void BuildOptions_DisablesStoredOutput()
    {
        // Responses defaults to store:true; tool outputs carry driver names and HOS data.
        var options = OpenAIResponsesLlmProvider.BuildOptions(Request("gpt-5.6-luna", ReasoningEffort.Low));

        Assert.False(options.StoredOutputEnabled);
    }

    #endregion
}
