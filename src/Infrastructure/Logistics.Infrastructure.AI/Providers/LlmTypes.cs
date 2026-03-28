using System.Text.Json.Nodes;
using Logistics.Application.Services;

namespace Logistics.Infrastructure.AI.Providers;

/// <summary>
/// Defines the structure of requests sent to LLM providers and the expected response format.
/// </summary>
internal record LlmRequest
{
    public required string SystemPrompt { get; init; }
    public required List<LlmMessage> Messages { get; init; }
    public required List<DispatchToolDefinition> Tools { get; init; }
    public required string Model { get; init; }
    public int MaxTokens { get; init; } = 8192;
    public decimal? Temperature { get; init; } = 0m;
    public LlmThinkingOptions? Thinking { get; init; }
}

internal record LlmMessage(LlmRole Role, List<LlmContentBlock> Content)
{
    public static LlmMessage FromUser(string text) =>
        new(LlmRole.User, [new LlmTextBlock(text)]);

    public static LlmMessage FromToolResults(List<LlmToolResultBlock> results) =>
        new(LlmRole.User, [.. results]);
}

internal enum LlmRole { User, Assistant }

internal abstract record LlmContentBlock;
internal record LlmTextBlock(string Text) : LlmContentBlock;
internal record LlmToolUseBlock(string Id, string Name, JsonNode? Input) : LlmContentBlock;
internal record LlmToolResultBlock(string ToolUseId, string Content) : LlmContentBlock;

internal record LlmResponse
{
    public required LlmMessage AssistantMessage { get; init; }
    public string? TextContent { get; init; }
    public required string StopReason { get; init; }
    public required List<LlmToolUseBlock> ToolCalls { get; init; }
    public required LlmTokenUsage Usage { get; init; }
}

internal record LlmTokenUsage(
    int InputTokens,
    int OutputTokens,
    int CacheReadTokens = 0,
    int CacheCreationTokens = 0);

internal record LlmThinkingOptions(int BudgetTokens);
