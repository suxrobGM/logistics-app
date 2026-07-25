using System.ClientModel;
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
internal sealed class OpenAILlmProvider(LlmProviderOptions config) : ILlmProvider
{
    public async Task<LlmResponse> SendAsync(LlmRequest request, CancellationToken ct)
    {
        var clientOptions = new OpenAIClientOptions();
        if (config.BaseUrl is not null)
            clientOptions.Endpoint = new Uri(config.BaseUrl);

        var client = new OpenAIClient(new ApiKeyCredential(config.ApiKey), clientOptions);
        var chatClient = client.GetChatClient(request.Model);

        var messages = new List<ChatMessage>
        {
            // System prompt
            ChatMessage.CreateSystemMessage(request.SystemPrompt)
        };

        // Conversation history
        foreach (var message in request.Messages)
        {
            messages.AddRange(ToOpenAIMessages(message));
        }

        // Tools
        var tools = new List<ChatTool>();
        foreach (var tool in request.Tools)
        {
            var schema = tool.InputSchema is JsonNode node
                ? BinaryData.FromString(node.ToJsonString())
                : BinaryData.FromString(JsonSerializer.Serialize(tool.InputSchema));

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
            var images = message.Content.OfType<LlmImageBlock>().ToList();
            var documents = message.Content.OfType<LlmDocumentBlock>().ToList();

            if (images.Count == 0 && documents.Count == 0)
                return [ChatMessage.CreateUserMessage(text)];

            // Multimodal message: text plus inline images and/or documents (e.g. PDFs) as content parts.
            var parts = new List<ChatMessageContentPart>();
            if (!string.IsNullOrEmpty(text))
            {
                parts.Add(ChatMessageContentPart.CreateTextPart(text));
            }

            foreach (var image in images)
            {
                parts.Add(ChatMessageContentPart.CreateImagePart(
                    BinaryData.FromBytes(Convert.FromBase64String(image.Base64Data)),
                    image.MediaType));
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

        // Assistant message with potential tool calls
        var assistantText = message.Content.OfType<LlmTextBlock>().FirstOrDefault()?.Text;
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
        string? textContent = null;
        var toolCalls = new List<LlmToolUseBlock>();

        // Extract text content
        foreach (var part in completion.Content)
        {
            if (part.Kind == ChatMessageContentPartKind.Text)
            {
                textContent = part.Text;
                content.Add(new LlmTextBlock(part.Text));
            }
        }

        // Extract tool calls
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
