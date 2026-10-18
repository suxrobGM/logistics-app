namespace Logistics.Infrastructure.AI.Llm;

internal enum LlmFailureKind
{
    /// <summary>401/403.</summary>
    Authentication,

    /// <summary>429, after the SDK's own retries.</summary>
    RateLimited,

    /// <summary>5xx or no response, after the SDK's own retries.</summary>
    Unavailable,

    /// <summary>Any other 4xx.</summary>
    Rejected
}

/// <summary>
/// A failed LLM call, translated by the provider so nothing past it needs SDK types to tell an
/// outage from a bad API key.
/// </summary>
internal sealed class LlmProviderException(LlmFailureKind kind, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public LlmFailureKind Kind { get; } = kind;

    public static LlmFailureKind KindFor(int status) => status switch
    {
        401 or 403 => LlmFailureKind.Authentication,
        429 => LlmFailureKind.RateLimited,
        >= 500 => LlmFailureKind.Unavailable,
        _ => LlmFailureKind.Rejected
    };
}
