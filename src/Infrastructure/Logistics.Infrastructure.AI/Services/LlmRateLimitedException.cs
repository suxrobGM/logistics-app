namespace Logistics.Infrastructure.AI.Services;

/// <summary>
/// The provider kept returning 429 after every retry. Distinct from a bare
/// <see cref="HttpRequestException"/> so <c>LlmErrorSanitizer</c> can tell a
/// rate-limit window apart from a credentials problem - the two send an operator to very different
/// places.
/// </summary>
internal sealed class LlmRateLimitedException(string message, Exception? innerException = null)
    : Exception(message, innerException);
