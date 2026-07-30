using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenAI;
using OpenAI.Chat;
using Logistics.Infrastructure.AI.Models;
using Logistics.Application.Abstractions.AI;

namespace Logistics.Infrastructure.AI.Providers;

/// <summary>
/// LLM provider for OpenAI-compatible APIs (OpenAI, DeepSeek, GLM, Groq, Mistral, etc.).
/// Uses the official OpenAI .NET SDK with configurable base URL for alternative providers.
/// </summary>
internal sealed class OpenAILlmProvider(LlmProviderOptions config, HttpClient httpClient) : ILlmProvider
{
    public async Task<LlmResponse> SendAsync(LlmRequest request, CancellationToken ct)
    {
        // System.ClientModel has no Timeout of its own - the only way to bound a call is to hand it
        // a transport over our factory-pooled HttpClient, which carries the configured timeout.
        var clientOptions = new OpenAIClientOptions
        {
            Transport = new HttpClientPipelineTransport(httpClient)
        };

        if (config.BaseUrl is not null)
            clientOptions.Endpoint = new Uri(config.BaseUrl);

        var client = new OpenAIClient(new ApiKeyCredential(config.ApiKey), clientOptions);
        var chatClient = client.GetChatClient(request.Model);

        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage(request.SystemPrompt)
        };

        foreach (var message in request.Messages)
        {
            messages.AddRange(ToOpenAIMessages(message));
        }

        var tools = new List<ChatTool>();
        foreach (var tool in request.Tools)
        {
            var schema = BinaryData.FromString(tool.InputSchema.ToJsonString());

            tools.Add(ChatTool.CreateFunctionTool(tool.Name, tool.Description, schema));
        }

        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = request.MaxTokens,
            Temperature = request.Temperature.HasValue ? (float)request.Temperature.Value : null
        };

        foreach (var tool in tools)
        {
            options.Tools.Add(tool);
        }

        var completion = await chatClient.CompleteChatAsync(messages, options, ct);
        return MapResponse(completion.Value);
    }

    /// <summary>
    /// Maps one provider-agnostic message to one or more OpenAI messages. A tool-results turn holds one
    /// block per tool call (Anthropic's shape), but OpenAI needs a separate tool message per
    /// tool_call_id - collapsing them into one is an HTTP 400 as soon as the model calls two tools.
    /// </summary>
    internal static IEnumerable<ChatMessage> ToOpenAIMessages(LlmMessage message)
    {
        if (message.Role == LlmRole.User)
        {
            var toolResults = message.Content.OfType<LlmToolResultBlock>().ToList();
            if (toolResults.Count > 0)
            {
                return toolResults.Select(ChatMessage (result) =>
                    ChatMessage.CreateToolMessage(result.ToolUseId, result.Content));
            }

            var textParts = message.Content.OfType<LlmTextBlock>().ToList();
            var text = string.Join("\n", textParts.Select(t => t.Text));
            var documents = message.Content.OfType<LlmDocumentBlock>().ToList();

            if (documents.Count == 0)
                return [ChatMessage.CreateUserMessage(text)];

            // Multimodal message: text plus inline documents (e.g. PDFs) as content parts.
            var parts = new List<ChatMessageContentPart>();
            if (!string.IsNullOrEmpty(text))
            {
                parts.Add(ChatMessageContentPart.CreateTextPart(text));
            }

            foreach (var document in documents)
            {
                // CreateFilePart is annotated [Experimental(OPENAI001)] in the current SDK; the file
                // content-part wire format is stable enough for our use, so opt in explicitly.
#pragma warning disable OPENAI001
                parts.Add(ChatMessageContentPart.CreateFilePart(
                    BinaryData.FromBytes(Convert.FromBase64String(document.Base64Data)),
                    document.MediaType,
                    "document.pdf"));
#pragma warning restore OPENAI001
            }

            return [ChatMessage.CreateUserMessage(parts)];
        }

        // Text blocks may be interleaved with tool calls (Anthropic's shape); OpenAI has no
        // ordering, so they collapse into one text part.
        var assistantTextParts = message.Content.OfType<LlmTextBlock>().Select(t => t.Text).ToList();
        var assistantText = assistantTextParts.Count > 0 ? string.Join("\n\n", assistantTextParts) : null;
        var toolUses = message.Content.OfType<LlmToolUseBlock>().ToList();

        if (toolUses.Count == 0)
            return [ChatMessage.CreateAssistantMessage(assistantText ?? "")];

        var toolCalls = toolUses
            .Select(t => ChatToolCall.CreateFunctionToolCall(
                t.Id,
                t.Name,
                BinaryData.FromString(t.Input?.ToJsonString() ?? "{}")))
            .ToList();

        var assistantMessage = new AssistantChatMessage(toolCalls);
        if (assistantText is not null)
            assistantMessage.Content.Add(ChatMessageContentPart.CreateTextPart(assistantText));
        return [assistantMessage];
    }

    private static LlmResponse MapResponse(ChatCompletion completion)
    {
        var content = new List<LlmContentBlock>();
        var textParts = new List<string>();
        var toolCalls = new List<LlmToolUseBlock>();

        foreach (var part in completion.Content)
        {
            if (part.Kind == ChatMessageContentPartKind.Text)
            {
                textParts.Add(part.Text);
                content.Add(new LlmTextBlock(part.Text));
            }
        }

        var textContent = textParts.Count > 0 ? string.Join("\n\n", textParts) : null;

        foreach (var toolCall in completion.ToolCalls)
        {
            var input = string.IsNullOrEmpty(toolCall.FunctionArguments?.ToString())
                ? null
                : JsonNode.Parse(toolCall.FunctionArguments.ToString());

            var block = new LlmToolUseBlock(toolCall.Id, toolCall.FunctionName, input);
            content.Add(block);
            toolCalls.Add(block);
        }

        var stopReason = completion.FinishReason switch
        {
            ChatFinishReason.ToolCalls => "tool_use",
            _ => "end_turn"
        };

        return new LlmResponse
        {
            AssistantMessage = new LlmMessage(LlmRole.Assistant, content),
            TextContent = textContent,
            StopReason = stopReason,
            ToolCalls = toolCalls,
            Usage = new LlmTokenUsage(
                completion.Usage?.InputTokenCount ?? 0,
                completion.Usage?.OutputTokenCount ?? 0)
        };
    }
}
