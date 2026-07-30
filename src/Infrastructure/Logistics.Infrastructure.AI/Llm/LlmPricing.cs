namespace Logistics.Infrastructure.AI.Llm;

/// <summary>
/// Per-model cost, quota multiplier, and Stripe overage units. Pricing from provider documentation,
/// per million tokens.
/// </summary>
internal static class LlmPricing
{
    /// <param name="InputPerMToken">Dollars per 1 million tokens.</param>
    /// <param name="QuotaMultiplier">Weekly-quota cost: 1 = base, 5 = premium, 10 = ultra.</param>
    /// <param name="OverageUnits">
    /// Stripe billing units at $0.20/unit: base=1 ($0.20), premium=2 ($0.40), ultra=4 ($0.80).
    /// </param>
    private record ModelPricing(
        decimal InputPerMToken,
        decimal OutputPerMToken,
        decimal CacheReadPerMToken,
        decimal CacheWritePerMToken,
        int QuotaMultiplier,
        int OverageUnits);

    private static ModelPricing Base(
        decimal input, decimal output, decimal cacheRead = 0m, decimal cacheWrite = 0m) =>
        new(input, output, cacheRead, cacheWrite, QuotaMultiplier: 1, OverageUnits: 1);

    private static ModelPricing Premium(
        decimal input, decimal output, decimal cacheRead = 0m, decimal cacheWrite = 0m) =>
        new(input, output, cacheRead, cacheWrite, QuotaMultiplier: 5, OverageUnits: 2);

    private static ModelPricing Ultra(
        decimal input, decimal output, decimal cacheRead = 0m, decimal cacheWrite = 0m) =>
        new(input, output, cacheRead, cacheWrite, QuotaMultiplier: 10, OverageUnits: 4);

    // Prices as of April 2026
    private static readonly Dictionary<string, ModelPricing> Pricing = new()
    {
        // Anthropic - https://platform.claude.com/docs/en/about-claude/pricing
        ["claude-opus-4-8"] = Ultra(5m, 25m, 0.50m, 6.25m),
        ["claude-sonnet-4-6"] = Premium(3m, 15m, 0.30m, 3.75m),
        ["claude-haiku-4-5"] = Base(1m, 5m, 0.10m, 1.25m),

        // OpenAI GPT-5.x - https://openai.com/api/pricing/
        ["gpt-5.4"] = Premium(2.50m, 15m, 0.25m),
        ["gpt-5.4-mini"] = Base(0.75m, 4.50m, 0.075m),

        // DeepSeek - https://api-docs.deepseek.com/quick_start/pricing/
        ["deepseek-v4-flash"] = Base(0.14m, 0.28m, 0.0028m),
        ["deepseek-v4-pro"] = Base(0.435m, 0.87m, 0.003625m),
    };

    /// <summary>
    /// Cost fallback for an unrecognised model. Deliberately NOT the fallback used for quota and
    /// overage: charging Sonnet rates for an unknown model is conservative, but billing it at
    /// Sonnet's tier would over-deduct quota and over-bill overage for what is most often a cheap
    /// or misconfigured model.
    /// </summary>
    private static readonly ModelPricing DefaultPricing = Pricing["claude-sonnet-4-6"];

    /// <summary>
    /// Weekly AI request quota cost. Unknown models count as base tier - see
    /// <see cref="DefaultPricing"/> for why this differs from the cost fallback.
    /// </summary>
    public static int GetMultiplier(string model) =>
        Pricing.TryGetValue(model, out var pricing) ? pricing.QuotaMultiplier : 1;

    /// <summary>Stripe billing units for overage reporting. Unknown models count as base tier.</summary>
    public static int GetOverageBillingUnits(string model) =>
        Pricing.TryGetValue(model, out var pricing) ? pricing.OverageUnits : 1;

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
