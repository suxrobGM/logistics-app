using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Abstractions.AI;

/// <summary>Models an admin can pick, with prices. Ids must match appsettings <c>Llm:Providers:{Provider}:Model</c>.</summary>
public static class LlmModelCatalog
{
    // Vendor pricing pages as of October 2026 (short-context, peak-hour rates), $ per million tokens.
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

    private static readonly LlmTokenPrices DefaultPrices = Find("claude-sonnet-5-5")!.Prices;

    public static LlmModelInfo? Find(string? id) =>
        id is null ? null : Models.FirstOrDefault(m => m.Id == id);

    public static bool IsValid(string? id) => Find(id) is not null;

    /// <summary>Unknown models get <see cref="ReasoningStyle.None"/> - never send a parameter an endpoint might reject.</summary>
    public static ReasoningStyle GetReasoningStyle(string? modelId) =>
        Find(modelId)?.Reasoning ?? ReasoningStyle.None;

    /// <summary>USD cost of one call, used by the weekly budget and Stripe overage. Unknown models pay Sonnet rates.</summary>
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

/// <summary>Which reasoning control a model exposes; <see cref="None"/> models never get a reasoning parameter.</summary>
public enum ReasoningStyle
{
    None,
    OpenAIEffort,
    AnthropicAdaptive
}
