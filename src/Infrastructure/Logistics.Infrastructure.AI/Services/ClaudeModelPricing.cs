namespace Logistics.Infrastructure.AI.Services;

/// <summary>
/// Calculates estimated cost in USD for Claude API usage based on model and token counts.
/// </summary>
internal static class ClaudeModelPricing
{
    private record ModelPricing(
        decimal InputPerMToken,
        decimal OutputPerMToken,
        decimal CacheReadPerMToken,
        decimal CacheWritePerMToken);

    // Pricing from https://platform.claude.com/docs/en/about-claude/pricing
    // Cache write = 5m cache write rate, Cache read = cache hit rate
    private static readonly Dictionary<string, ModelPricing> Pricing = new()
    {
        ["claude-sonnet-4-6"] = new(3m, 15m, 0.30m, 3.75m),
        ["claude-haiku-4-5"] = new(1m, 5m, 0.10m, 1.25m),
        ["claude-opus-4-6"] = new(5m, 25m, 0.50m, 6.25m)
    };

    private static readonly ModelPricing DefaultPricing = Pricing["claude-sonnet-4-6"];

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
