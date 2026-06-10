using Logistics.Application.Abstractions.AiDispatch;

namespace Logistics.Infrastructure.AI.Models;

/// <summary>
/// Defines the structure of a request sent to an LLM provider.
/// </summary>
internal record LlmRequest
{
    public required string SystemPrompt { get; init; }
    public required List<LlmMessage> Messages { get; init; }
    public required List<AiDispatchToolDefinition> Tools { get; init; }
    public required string Model { get; init; }
    public int MaxTokens { get; init; } = 8192;
    public decimal? Temperature { get; init; } = 0m;
    public LlmThinkingOptions? Thinking { get; init; }
}
