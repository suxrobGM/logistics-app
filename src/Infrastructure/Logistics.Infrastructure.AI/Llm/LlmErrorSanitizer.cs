using Logistics.Application.Abstractions.AI;

namespace Logistics.Infrastructure.AI.Llm;

/// <summary>
/// Strips credential and infrastructure detail from a failed LLM call before it reaches a
/// tenant-visible session row. Tool failures go through <see cref="ToolErrorSanitizer"/>.
/// </summary>
internal static class LlmErrorSanitizer
{
    private const string AuthenticationError = "API authentication error. Check the LLM API key configuration.";

    /// <summary>
    /// For a failed LLM call. Classified by kind: only a real auth failure may name the API key, or
    /// an outage sends the operator off to audit credentials.
    /// </summary>
    public static string ForSession(Exception ex)
    {
        switch ((ex as LlmProviderException)?.Kind)
        {
            case LlmFailureKind.Authentication:
                return AuthenticationError;
            case LlmFailureKind.RateLimited:
                return "Rate limited by the LLM API. Please try again in a few minutes.";
            case LlmFailureKind.Unavailable:
                return "The LLM provider is unavailable or overloaded. Please try again shortly.";
        }

        if (ex is UnauthorizedAccessException || ToolErrorSanitizer.LooksSensitive(ex.Message))
            return AuthenticationError;

        return ToolErrorSanitizer.Truncate(ex.Message);
    }
}
