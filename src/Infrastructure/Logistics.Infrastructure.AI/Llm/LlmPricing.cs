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

    // Prices as of October 2026
    private static readonly Dictionary<string, ModelPricing> Pricing = new()
    {
        // Anthropic - https://platform.claude.com/docs/en/about-claude/pricing
        // Haiku 5.5 rates cover prompts up to 100K tokens (0.50 / 2.50 beyond).
        ["claude-sonnet-5-5"] = new(2m, 10m, 0.20m, 2.50m),
        ["claude-haiku-5-5"] = new(0.10m, 0.50m, 0.01m, 0.125m),

        // OpenAI GPT-6 - https://developers.openai.com/api/docs/pricing
        // Short-context rates; prompts over 272K input tokens bill higher.
        ["gpt-6-sol"] = new(2m, 10m, 0.20m, 2.50m),
        ["gpt-6-luna"] = new(0.10m, 0.50m, 0.01m, 0.125m),

        // DeepSeek - https://api-docs.deepseek.com/quick_start/pricing/
        // Peak-hour rates (off-peak is half): the call time is unknown, so charge the higher one.
        ["deepseek-flash"] = new(0.30m, 1.20m, 0.006m),
        ["deepseek-v4-pro"] = new(1.32m, 3.96m, 0.044m),
    };

    /// <summary>
    /// Cost fallback for an unrecognised model: charging Sonnet rates is the conservative choice,
    /// since an unknown model is most often a cheap or misconfigured one.
    /// </summary>
    private static readonly ModelPricing DefaultPricing = Pricing["claude-sonnet-5-5"];

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
