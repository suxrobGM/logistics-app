using Logistics.Application.Abstractions.AIDispatch;

namespace Logistics.Infrastructure.AI.Llm.Contracts;

internal record LlmRequest
{
    public required string SystemPrompt { get; init; }
    public required List<LlmMessage> Messages { get; init; }
    public required List<AIDispatchToolDefinition> Tools { get; init; }
    public required string Model { get; init; }
    public required int MaxTokens { get; init; }
    public decimal? Temperature { get; init; }
    public LlmThinkingOptions? Thinking { get; init; }
}
