using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Abstractions.AI;

/// <summary>
/// Single source of truth for the LLM models an admin can select as the platform-wide
/// dispatch model. Keep the ids in sync with <c>LlmPricing</c> (cost keys)
/// and the appsettings <c>Llm:Providers:{Provider}:Model</c> values.
/// </summary>
public static class LlmModelCatalog
{
    // Models[0] is the UI fallback - keep the platform default (gpt-5.6-luna) first.
    public static readonly IReadOnlyList<LlmModelInfo> Models =
    [
        new("gpt-5.6-luna", "GPT-5.6 Luna", LlmProvider.OpenAI, ReasoningStyle.OpenAIEffort),
        new("deepseek-v4-flash", "DeepSeek V4 Flash", LlmProvider.DeepSeek),
        new("deepseek-v4-pro", "DeepSeek V4 Pro", LlmProvider.DeepSeek),
        new("claude-haiku-4-5", "Claude Haiku 4.5", LlmProvider.Anthropic),
        new("claude-sonnet-5", "Claude Sonnet 5", LlmProvider.Anthropic, ReasoningStyle.AnthropicAdaptive),
        new("gpt-5.6-terra", "GPT-5.6 Terra", LlmProvider.OpenAI, ReasoningStyle.OpenAIEffort),
    ];

    public static LlmModelInfo? Find(string? id) =>
        id is null ? null : Models.FirstOrDefault(m => m.Id == id);

    public static bool IsValid(string? id) => Find(id) is not null;

    /// <summary>Unknown models get <see cref="ReasoningStyle.None"/> - never send a parameter an endpoint might reject.</summary>
    public static ReasoningStyle ReasoningStyleOf(string? modelId) =>
        Find(modelId)?.Reasoning ?? ReasoningStyle.None;
}

public record LlmModelInfo(
    string Id,
    string DisplayName,
    LlmProvider Provider,
    ReasoningStyle Reasoning = ReasoningStyle.None);

/// <summary>
/// Which reasoning control a model exposes. <see cref="None"/> models never receive a reasoning
/// parameter (Haiku's legacy budget-token thinking is deliberately unsupported).
/// </summary>
public enum ReasoningStyle
{
    None,
    OpenAIEffort,
    AnthropicAdaptive
}
