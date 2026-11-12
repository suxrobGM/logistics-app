using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Abstractions.AI;

/// <summary>
/// Single source of truth for the LLM models an admin can select as the platform-wide
/// dispatch model, and for what each one costs. Keep the ids in sync with the appsettings
/// <c>Llm:Providers:{Provider}:Model</c> values.
/// </summary>
public static class LlmModelCatalog
{
    // Prices as of October 2026, in dollars per million tokens.
    // Anthropic - https://platform.claude.com/docs/en/about-claude/pricing
    //   Haiku 5.5 rates cover prompts up to 100K tokens (0.50 / 2.50 beyond).
    // OpenAI GPT-6 - https://developers.openai.com/api/docs/pricing
    //   Short-context rates; prompts over 272K input tokens bill higher.
    // DeepSeek - https://api-docs.deepseek.com/quick_start/pricing/
    //   Peak-hour rates (off-peak is half): the call time is unknown, so charge the higher one.
    // Models[0] is the UI fallback - keep the platform default (gpt-6-luna) first.
    public static readonly IReadOnlyList<LlmModelInfo> Models =
    [
        new("gpt-6-luna", "GPT-6 Luna", LlmProvider.OpenAI, new(0.10m, 0.50m, 0.01m, 0.125m),
            ReasoningStyle.OpenAIEffort),
        new("deepseek-flash", "DeepSeek V4.1 Flash", LlmProvider.DeepSeek, new(0.30m, 1.20m, 0.006m)),
        new("deepseek-v4-pro", "DeepSeek V4 Pro", LlmProvider.DeepSeek, new(1.32m, 3.96m, 0.044m)),
        new("claude-haiku-5-5", "Claude Haiku 5.5", LlmProvider.Anthropic, new(0.10m, 0.50m, 0.01m, 0.125m),
            ReasoningStyle.AnthropicAdaptive),
        new("claude-sonnet-5-5", "Claude Sonnet 5.5", LlmProvider.Anthropic, new(2m, 10m, 0.20m, 2.50m),
            ReasoningStyle.AnthropicAdaptive),
        new("gpt-6.1-sol", "GPT-6.1 Sol", LlmProvider.OpenAI, new(2m, 10m, 0.10m, 2.50m),
            ReasoningStyle.OpenAIEffort),
    ];

    /// <summary>
    /// Cost fallback for an unrecognised model: charging Sonnet rates is the conservative choice,
    /// since an unknown model is most often a cheap or misconfigured one.
    /// </summary>
    private static readonly LlmTokenPrices DefaultPrices = Find("claude-sonnet-5-5")!.Prices;

    public static LlmModelInfo? Find(string? id) =>
        id is null ? null : Models.FirstOrDefault(m => m.Id == id);

    public static bool IsValid(string? id) => Find(id) is not null;

    /// <summary>Unknown models get <see cref="ReasoningStyle.None"/> - never send a parameter an endpoint might reject.</summary>
    public static ReasoningStyle GetReasoningStyle(string? modelId) =>
        Find(modelId)?.Reasoning ?? ReasoningStyle.None;

    /// <summary>
    /// USD cost of one call. The weekly budget and Stripe overage both derive from this - there is no
    /// separate tier or multiplier concept. Unknown models are charged Sonnet rates.
    /// </summary>
    public static decimal CalculateCost(
        string model,
        int inputTokens,
        int outputTokens,
        int cacheReadTokens = 0,
        int cacheCreationTokens = 0)
    {
        var prices = Find(model)?.Prices ?? DefaultPrices;

        return (inputTokens * prices.InputPerMToken
            + outputTokens * prices.OutputPerMToken
            + cacheReadTokens * prices.CacheReadPerMToken
            + cacheCreationTokens * prices.CacheWritePerMToken) / 1_000_000m;
    }
}

public record LlmModelInfo(
    string Id,
    string DisplayName,
    LlmProvider Provider,
    LlmTokenPrices Prices,
    ReasoningStyle Reasoning = ReasoningStyle.None);

/// <summary>A model's token prices, in dollars per million tokens.</summary>
public record LlmTokenPrices(
    decimal InputPerMToken,
    decimal OutputPerMToken,
    decimal CacheReadPerMToken = 0m,
    decimal CacheWritePerMToken = 0m);

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
