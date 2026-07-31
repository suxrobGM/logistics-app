using Logistics.Application.Abstractions.Agents;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Llm;

namespace Logistics.Infrastructure.AI.Llm.Contracts;

/// <summary>
/// Provider-agnostic conversation state. Produced by either conversation builder and consumed by
/// <c>AgentLoopRunner</c>.
/// </summary>
internal record LlmConversation(
    ILlmProvider Provider,
    string SystemPrompt,
    List<LlmMessage> Messages,
    IReadOnlyList<AgentToolDefinition> Tools,
    string Model,
    int MaxTokens,
    ReasoningEffort Effort);
