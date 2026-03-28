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

    private static readonly Dictionary<string, ModelPricing> Pricing = new()
    {
        // Anthropic — https://platform.claude.com/docs/en/about-claude/pricing
        ["claude-sonnet-4-6"] = new(3m, 15m, 0.30m, 3.75m),
        ["claude-haiku-4-5"] = new(1m, 5m, 0.10m, 1.25m),
        ["claude-opus-4-6"] = new(5m, 25m, 0.50m, 6.25m),

        // OpenAI GPT-5.x — https://openai.com/api/pricing/
        ["gpt-5.4-pro"] = new(30m, 180m),
        ["gpt-5.4"] = new(2.50m, 15m, 0.25m),
        ["gpt-5.4-mini"] = new(0.75m, 4.50m, 0.075m),
        ["gpt-5.4-nano"] = new(0.20m, 1.25m, 0.02m),

        // DeepSeek — https://api-docs.deepseek.com/quick_start/pricing/
        ["deepseek-chat"] = new(0.28m, 0.42m, 0.028m),
        ["deepseek-reasoner"] = new(0.28m, 0.42m, 0.028m),
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
