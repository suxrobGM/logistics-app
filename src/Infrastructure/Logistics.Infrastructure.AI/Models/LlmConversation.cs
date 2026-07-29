using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Infrastructure.AI.Providers;

namespace Logistics.Infrastructure.AI.Models;

/// <summary>
/// Provider-agnostic conversation state. Produced by either conversation builder and consumed by
/// <c>AgentLoopRunner</c>.
/// </summary>
internal record LlmConversation(
    ILlmProvider Provider,
    string SystemPrompt,
    List<LlmMessage> Messages,
    IReadOnlyList<AIDispatchToolDefinition> Tools,
    string Model,
    int MaxTokens,
    LlmThinkingOptions? Thinking);
