using System.ClientModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenAI;
using OpenAI.Chat;

namespace Logistics.Infrastructure.AI.Providers;

/// <summary>
/// LLM provider for OpenAI-compatible APIs (OpenAI, DeepSeek, GLM, Groq, Mistral, etc.).
/// Uses the official OpenAI .NET SDK with configurable base URL for alternative providers.
/// </summary>
internal sealed class OpenAiLlmProvider(Options.LlmProviderOptions config) : ILlmProvider
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
            messages.Add(ToOpenAiMessage(message));
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

    private static ChatMessage ToOpenAiMessage(LlmMessage message)
    {
        if (message.Role == LlmRole.User)
        {
            // Check if this is a tool results message
            var toolResults = message.Content.OfType<LlmToolResultBlock>().ToList();
            if (toolResults.Count > 0)
            {
                // OpenAI expects individual ToolChatMessages for each result
                // We return the first one; the caller handles multiple via the message list
                // Actually, for OpenAI we need to return multiple messages - handle this specially
                return ChatMessage.CreateToolMessage(toolResults[0].ToolUseId, toolResults[0].Content);
            }

            var textParts = message.Content.OfType<LlmTextBlock>().ToList();
            var text = string.Join("\n", textParts.Select(t => t.Text));
            return ChatMessage.CreateUserMessage(text);
        }

        // Assistant message with potential tool calls
        var assistantText = message.Content.OfType<LlmTextBlock>().FirstOrDefault()?.Text;
        var toolUses = message.Content.OfType<LlmToolUseBlock>().ToList();

        if (toolUses.Count == 0)
            return ChatMessage.CreateAssistantMessage(assistantText ?? "");

        var toolCalls = toolUses
            .Select(t => ChatToolCall.CreateFunctionToolCall(
                t.Id,
                t.Name,
                BinaryData.FromString(t.Input?.ToJsonString() ?? "{}")))
            .ToList();

        var assistantMessage = new AssistantChatMessage(toolCalls);
        if (assistantText is not null)
            assistantMessage.Content.Add(ChatMessageContentPart.CreateTextPart(assistantText));
        return assistantMessage;
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
