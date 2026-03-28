using System.Text.Json.Nodes;
using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using Logistics.Infrastructure.AI.Options;
using Tool = Anthropic.SDK.Common.Tool;
using Function = Anthropic.SDK.Common.Function;
using Logistics.Application.Services;

namespace Logistics.Infrastructure.AI.Providers;

/// <summary>
/// LLM provider implementation using the Anthropic (Claude) API.
/// </summary>
internal sealed class AnthropicLlmProvider(LlmProviderOptions config) : ILlmProvider
{
    public async Task<LlmResponse> SendAsync(LlmRequest request, CancellationToken ct)
    {
        var client = new AnthropicClient(config.ApiKey);

        var tools = request.Tools
            .Select<DispatchToolDefinition, Tool>(t =>
                new Function(t.Name, t.Description, (JsonNode)t.InputSchema))
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

        var textContent = response.Content?.OfType<TextContent>().FirstOrDefault()?.Text;
        var toolUseBlocks = response.Content?.OfType<ToolUseContent>().ToList() ?? [];

        var assistantContent = new List<LlmContentBlock>();
        if (textContent is not null)
        {
            assistantContent.Add(new LlmTextBlock(textContent));
        }

        foreach (var tool in toolUseBlocks)
        {
            assistantContent.Add(new LlmToolUseBlock(tool.Id, tool.Name, tool.Input));
        }

        return new LlmResponse
        {
            AssistantMessage = new LlmMessage(LlmRole.Assistant, assistantContent),
            TextContent = textContent,
            StopReason = response.StopReason ?? "end_turn",
            ToolCalls = [.. toolUseBlocks.Select(t => new LlmToolUseBlock(t.Id, t.Name, t.Input))],
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
            }
        }

        return new Message { Role = role, Content = content };
    }
}
