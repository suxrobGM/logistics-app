namespace Logistics.Infrastructure.AI.Llm;

/// <summary>What kind of failure an <see cref="LlmProviderException"/> reports.</summary>
internal enum LlmFailureKind
{
    /// <summary>The provider refused the credentials (401/403). The one thing an operator can fix.</summary>
    Authentication,

    /// <summary>Still rate limited (429) after the SDK's own retries.</summary>
    RateLimited,

    /// <summary>The provider is down or overloaded (5xx, 529, network) after the SDK's own retries.</summary>
    Unavailable,

    /// <summary>The provider rejected the request itself (other 4xx).</summary>
    Rejected
}

/// <summary>
/// A failed LLM call, translated from the vendor SDK's own exception by the provider class so
/// nothing past it needs SDK types to tell an outage from a bad API key.
/// </summary>
internal sealed class LlmProviderException(LlmFailureKind kind, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public LlmFailureKind Kind { get; } = kind;

    /// <summary>Maps an HTTP status to a failure kind. Shared by every provider.</summary>
    public static LlmFailureKind KindFor(int status) => status switch
    {
        401 or 403 => LlmFailureKind.Authentication,
        429 => LlmFailureKind.RateLimited,
        >= 500 => LlmFailureKind.Unavailable,
        _ => LlmFailureKind.Rejected
    };
}
