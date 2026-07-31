namespace Logistics.Infrastructure.AI.Llm;

/// <summary>
/// Per-model cost. Pricing from provider documentation, per million tokens. The weekly budget and
/// Stripe overage both derive from the USD cost this class computes - there is no separate tier
/// or multiplier concept.
/// </summary>
internal static class LlmPricing
{
    /// <param name="InputPerMToken">Dollars per 1 million tokens.</param>
    private record ModelPricing(
        decimal InputPerMToken,
        decimal OutputPerMToken,
        decimal CacheReadPerMToken = 0m,
        decimal CacheWritePerMToken = 0m);

    // Prices as of July 2026
    private static readonly Dictionary<string, ModelPricing> Pricing = new()
    {
        // Anthropic - https://platform.claude.com/docs/en/about-claude/pricing
        // Sonnet 5 intro pricing ends 2026-08-31 (reverts to 3 / 15 / 0.30 / 3.75).
        ["claude-sonnet-5"] = new(2m, 10m, 0.20m, 2.50m),
        ["claude-haiku-4-5"] = new(1m, 5m, 0.10m, 1.25m),

        // OpenAI GPT-5.6 - https://openai.com/api/pricing/ (cache writes: 1.25x input)
        ["gpt-5.6-terra"] = new(2m, 12m, 0.20m, 2.50m),
        ["gpt-5.6-luna"] = new(0.20m, 1.20m, 0.02m, 0.25m),

        // DeepSeek - https://api-docs.deepseek.com/quick_start/pricing/
        ["deepseek-v4-flash"] = new(0.14m, 0.28m, 0.0028m),
        ["deepseek-v4-pro"] = new(0.435m, 0.87m, 0.003625m),
    };

    /// <summary>
    /// Cost fallback for an unrecognised model: charging Sonnet rates is the conservative choice,
    /// since an unknown model is most often a cheap or misconfigured one.
    /// </summary>
    private static readonly ModelPricing DefaultPricing = Pricing["claude-sonnet-5"];

    /// <summary>All model ids with a pricing entry. Used by parity tests against the catalog.</summary>
    internal static IReadOnlyCollection<string> KnownModels => Pricing.Keys;

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
