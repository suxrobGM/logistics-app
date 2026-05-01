using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Services;

/// <summary>
/// Calculates estimated cost in USD for LLM API usage based on model and token counts.
/// Pricing from provider documentation. Per-million-token rates.
/// </summary>
internal static class LlmPricing
{
    private record ModelPricing(
        decimal InputPerMToken, // format: dollars per 1 million tokens
        decimal OutputPerMToken,
        decimal CacheReadPerMToken = 0m,
        decimal CacheWritePerMToken = 0m);

    // Prices as of April 2026
    private static readonly Dictionary<string, ModelPricing> Pricing = new()
    {
        // Anthropic — https://platform.claude.com/docs/en/about-claude/pricing
        ["claude-opus-4-6"] = new(5m, 25m, 0.50m, 6.25m),
        ["claude-sonnet-4-6"] = new(3m, 15m, 0.30m, 3.75m),
        ["claude-haiku-4-5"] = new(1m, 5m, 0.10m, 1.25m),

        // OpenAI GPT-5.x — https://openai.com/api/pricing/
        ["gpt-5.4"] = new(2.50m, 15m, 0.25m),
        ["gpt-5.4-mini"] = new(0.75m, 4.50m, 0.075m),

        // DeepSeek — https://api-docs.deepseek.com/quick_start/pricing/
        ["deepseek-v4-flash"] = new(0.14m, 0.28m, 0.0028m),
        ["deepseek-v4-pro"] = new(1.74m, 3.48m, 0.0145m),
    };

    private static readonly ModelPricing DefaultPricing = Pricing["claude-sonnet-4-6"];

    /// <summary>
    /// Returns the quota cost multiplier for a model (1 = base, 5 = premium, 10 = ultra).
    /// Used to deduct from the tenant's weekly AI request quota.
    /// </summary>
    public static int GetMultiplier(string model) => model switch
    {
        "deepseek-v4-flash" or "deepseek-v4-pro" or "gpt-5.4-mini" or "claude-haiku-4-5" => 1,
        "gpt-5.4" or "claude-sonnet-4-6" => 5,
        "claude-opus-4-6" => 10,
        _ => 1
    };

    /// <summary>
    /// Returns the model tier classification.
    /// </summary>
    public static LlmModelTier GetModelTier(string model) => model switch
    {
        "gpt-5.4" or "claude-sonnet-4-6" => LlmModelTier.Premium,
        "claude-opus-4-6" => LlmModelTier.Ultra,
        _ => LlmModelTier.Base
    };

    /// <summary>
    /// Returns the Stripe billing units for overage reporting.
    /// At $0.20/unit: base=1 ($0.20), premium=2 ($0.40), ultra=4 ($0.80).
    /// </summary>
    public static int GetOverageBillingUnits(string model) => model switch
    {
        "gpt-5.4" or "claude-sonnet-4-6" => 2,
        "claude-opus-4-6" => 4,
        _ => 1
    };

    public static decimal Calculate(
        string model,
        int inputTokens,
        int outputTokens,
        int cacheReadTokens = 0,
        int cacheCreationTokens = 0)
    {
        var pricing = Pricing.GetValueOrDefault(model, DefaultPricing);

        return (inputTokens * pricing.InputPerMToken
            + outputTokens * pricing.OutputPerMToken
            + cacheReadTokens * pricing.CacheReadPerMToken
            + cacheCreationTokens * pricing.CacheWritePerMToken) / 1_000_000m;
    }
}
