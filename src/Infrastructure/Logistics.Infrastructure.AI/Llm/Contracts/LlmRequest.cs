using Logistics.Application.Abstractions.Agents;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Llm.Contracts;

internal record LlmRequest
{
    public required string SystemPrompt { get; init; }
    public required List<LlmMessage> Messages { get; init; }
    public required List<AgentToolDefinition> Tools { get; init; }
    public required string Model { get; init; }
    public required int MaxTokens { get; init; }
    public decimal? Temperature { get; init; }

    /// <summary>Reasoning depth; providers map it per the catalog's reasoning style.</summary>
    public ReasoningEffort Effort { get; init; } = ReasoningEffort.None;
}
