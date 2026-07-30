using Logistics.Application.Abstractions.Agents;
using System.Text.Json.Nodes;
using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using Logistics.Infrastructure.AI.Llm.Contracts;
using Logistics.Application.Abstractions.AI;
using Tool = Anthropic.SDK.Common.Tool;
using Function = Anthropic.SDK.Common.Function;

namespace Logistics.Infrastructure.AI.Llm.Providers;

/// <summary>
/// LLM provider implementation using the Anthropic (Claude) API.
/// </summary>
internal sealed class AnthropicLlmProvider(LlmProviderOptions config, HttpClient httpClient) : ILlmProvider
{
    public async Task<LlmResponse> SendAsync(LlmRequest request, CancellationToken ct)
    {
        // The HttpClient is factory-pooled and carries the configured timeout; AnthropicClient
        // must not dispose it, so it is never wrapped in a using.
        var client = new AnthropicClient(new APIAuthentication(config.ApiKey), httpClient);

        var tools = request.Tools
            .Select<AgentToolDefinition, Tool>(t =>
                new Function(t.Name, t.Description, t.InputSchema))
            .ToList();

        var messages = request.Messages.Select(ToAnthropicMessage).ToList();

        var parameters = new MessageParameters
        {
            Messages = messages,
            MaxTokens = request.MaxTokens,
            Model = request.Model,
            Stream = false,
            Temperature = request.Temperature,
            System =
            [
                new SystemMessage(request.SystemPrompt)
                {
                    CacheControl = new CacheControl { Type = CacheControlType.ephemeral }
                }
            ],
            Tools = tools
        };

        if (request.Thinking is not null)
        {
            parameters.Temperature = null;
            parameters.Thinking = new ThinkingParameters
            {
                Type = ThinkingType.enabled,
                BudgetTokens = request.Thinking.BudgetTokens
            };
            parameters.MaxTokens = Math.Max(request.MaxTokens, request.Thinking.BudgetTokens + 4096);
        }

        var response = await client.Messages.GetClaudeMessageAsync(parameters, ct);
        return MapResponse(response);
    }

    /// <summary>
    /// Preserves block order and keeps every text block: replies interleave text with tool calls,
    /// and the transcript must replay in the shape it arrived.
    /// </summary>
    internal static LlmResponse MapResponse(MessageResponse response)
    {
        var assistantContent = new List<LlmContentBlock>();
        foreach (var block in response.Content ?? [])
        {
            switch (block)
            {
                case TextContent text:
                    assistantContent.Add(new LlmTextBlock(text.Text));
                    break;
                case ToolUseContent toolUse:
                    assistantContent.Add(new LlmToolUseBlock(toolUse.Id, toolUse.Name, toolUse.Input));
                    break;
            }
        }

        var textBlocks = assistantContent.OfType<LlmTextBlock>().Select(t => t.Text).ToList();

        return new LlmResponse
        {
            AssistantMessage = new LlmMessage(LlmRole.Assistant, assistantContent),
            TextContent = textBlocks.Count > 0 ? string.Join("\n\n", textBlocks) : null,
            StopReason = response.StopReason ?? "end_turn",
            ToolCalls = [.. assistantContent.OfType<LlmToolUseBlock>()],
            Usage = new LlmTokenUsage(
                response.Usage?.InputTokens ?? 0,
                response.Usage?.OutputTokens ?? 0,
                response.Usage?.CacheReadInputTokens ?? 0,
                response.Usage?.CacheCreationInputTokens ?? 0)
        };
    }

    private static Message ToAnthropicMessage(LlmMessage message)
    {
        var role = message.Role == LlmRole.User ? RoleType.User : RoleType.Assistant;
        var content = new List<ContentBase>();

        foreach (var block in message.Content)
        {
            switch (block)
            {
                case LlmTextBlock text:
                    content.Add(new TextContent { Text = text.Text });
                    break;
                case LlmToolUseBlock toolUse:
                    content.Add(new ToolUseContent { Id = toolUse.Id, Name = toolUse.Name, Input = toolUse.Input });
                    break;
                case LlmToolResultBlock toolResult:
                    content.Add(new ToolResultContent
                    {
                        ToolUseId = toolResult.ToolUseId,
                        Content = [new TextContent { Text = toolResult.Content }]
                    });
                    break;
                case LlmDocumentBlock document:
                    content.Add(new DocumentContent
                    {
                        Source = new DocumentSource
                        {
                            Type = SourceType.base64,
                            MediaType = document.MediaType,
                            Data = document.Base64Data
                        }
                    });
                    break;
            }
        }

        return new Message { Role = role, Content = content };
    }
}
