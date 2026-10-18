using Logistics.Infrastructure.AI.Llm.Contracts;

namespace Logistics.Infrastructure.AI.Llm.Providers;

internal static class OpenAIUsage
{
    /// <summary>
    /// OpenAI's <c>input_tokens</c> <b>includes</b> <c>cached_tokens</c> (Anthropic's excludes them),
    /// and <c>LlmPricing.Calculate</c> adds both buckets - so subtract, or every cached token bills
    /// twice. Cache writes are free on OpenAI, so creation stays zero.
    /// </summary>
    public static LlmTokenUsage From(int inputTokens, int cachedTokens, int outputTokens)
    {
        var cached = Math.Clamp(cachedTokens, 0, inputTokens);

        return new LlmTokenUsage(inputTokens - cached, outputTokens, cached);
    }
}
