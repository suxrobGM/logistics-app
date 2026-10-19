using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Abstractions.AI;

/// <summary>
/// Single source of truth for the LLM models an admin can select as the platform-wide
/// dispatch model. Keep the ids in sync with <c>LlmPricing</c> (cost keys)
/// and the appsettings <c>Llm:Providers:{Provider}:Model</c> values.
/// </summary>
public static class LlmModelCatalog
{
    // Models[0] is the UI fallback - keep the platform default (gpt-6-luna) first.
    public static readonly IReadOnlyList<LlmModelInfo> Models =
    [
        new("gpt-6-luna", "GPT-6 Luna", LlmProvider.OpenAI, ReasoningStyle.OpenAIEffort),
        new("deepseek-flash", "DeepSeek V4.1 Flash", LlmProvider.DeepSeek),
        new("deepseek-v4-pro", "DeepSeek V4 Pro", LlmProvider.DeepSeek),
        new("claude-haiku-5-5", "Claude Haiku 5.5", LlmProvider.Anthropic, ReasoningStyle.AnthropicAdaptive),
        new("claude-sonnet-5-5", "Claude Sonnet 5.5", LlmProvider.Anthropic, ReasoningStyle.AnthropicAdaptive),
        new("gpt-6.1-sol", "GPT-6.1 Sol", LlmProvider.OpenAI, ReasoningStyle.OpenAIEffort),
    ];

    public static LlmModelInfo? Find(string? id) =>
        id is null ? null : Models.FirstOrDefault(m => m.Id == id);

    public static bool IsValid(string? id) => Find(id) is not null;

    /// <summary>Unknown models get <see cref="ReasoningStyle.None"/> - never send a parameter an endpoint might reject.</summary>
    public static ReasoningStyle GetReasoningStyle(string? modelId) =>
        Find(modelId)?.Reasoning ?? ReasoningStyle.None;
}

public record LlmModelInfo(
    string Id,
    string DisplayName,
    LlmProvider Provider,
    ReasoningStyle Reasoning = ReasoningStyle.None);

/// <summary>
/// Which reasoning control a model exposes. <see cref="None"/> models never receive a reasoning
/// parameter.
/// </summary>
public enum ReasoningStyle
{
    None,
    OpenAIEffort,
    AnthropicAdaptive
}
