namespace Logistics.Infrastructure.AI.Models;

/// <summary>
/// Token usage reported by an LLM call, used for cost and quota accounting.
/// </summary>
internal record LlmTokenUsage(
    int InputTokens,
    int OutputTokens,
    int CacheReadTokens = 0,
    int CacheCreationTokens = 0);
