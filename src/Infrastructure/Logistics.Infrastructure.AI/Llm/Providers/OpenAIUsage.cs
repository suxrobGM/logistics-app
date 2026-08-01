using Logistics.Infrastructure.AI.Llm.Contracts;

namespace Logistics.Infrastructure.AI.Llm.Providers;

/// <summary>Token accounting shared by both OpenAI-shaped providers.</summary>
internal static class OpenAIUsage
{
    /// <summary>
    /// Splits an OpenAI usage report into the buckets <see cref="LlmTokenUsage"/> expects.
    /// </summary>
    /// <remarks>
    /// The subtraction is the point: OpenAI's <c>input_tokens</c> <b>includes</b>
    /// <c>cached_tokens</c> where Anthropic's excludes them, and <c>LlmPricing.Calculate</c> adds
    /// the two buckets - so raw counts bill every cached token twice. Cache creation stays zero;
    /// OpenAI's caching is implicit and free to write.
    /// </remarks>
    public static LlmTokenUsage From(int inputTokens, int cachedTokens, int outputTokens)
    {
        var cached = Math.Clamp(cachedTokens, 0, inputTokens);

        return new LlmTokenUsage(inputTokens - cached, outputTokens, cached);
    }
}
