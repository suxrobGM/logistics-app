namespace Logistics.Infrastructure.AI.Llm;

/// <summary>
/// Strips credential and infrastructure detail from exception text before it reaches a
/// tenant-visible session row, a decision row, or the model's context window.
/// </summary>
internal static class LlmErrorSanitizer
{
    private const int MaxLength = 500;

    private const string AuthenticationError = "API authentication error. Check the LLM API key configuration.";

    private static readonly string[] SensitiveMarkers =
        ["api key", "authentication", "unauthorized", "password", "secret", "token=", "connection string"];

    private static bool LooksSensitive(string message) =>
        SensitiveMarkers.Any(marker => message.Contains(marker, StringComparison.OrdinalIgnoreCase));

    private static string Truncate(string message) =>
        message.Length > MaxLength ? message[..MaxLength] : message;

    /// <summary>
    /// For a failed LLM call. Classified by kind: only a real auth failure may name the API key, or
    /// an outage sends the operator off to audit credentials.
    /// </summary>
    public static string ForSession(Exception ex)
    {
        if (ex is LlmProviderException provider)
        {
            return provider.Kind switch
            {
                LlmFailureKind.Authentication => AuthenticationError,
                LlmFailureKind.RateLimited => "Rate limited by the LLM API. Please try again in a few minutes.",
                LlmFailureKind.Unavailable => "The LLM provider is unavailable or overloaded. Please try again shortly.",
                _ => LooksSensitive(ex.Message) ? AuthenticationError : Truncate(ex.Message)
            };
        }

        if (ex is UnauthorizedAccessException || LooksSensitive(ex.Message))
            return AuthenticationError;

        return Truncate(ex.Message);
    }

    /// <summary>
    /// For a failed tool. Never blames the LLM key: a tool talks to the database and vendor APIs, so
    /// an auth-shaped failure here is something else.
    /// </summary>
    public static string ForToolCall(Exception ex) =>
        LooksSensitive(ex.Message)
            ? "The tool failed due to a configuration or authorization problem. Report this to the dispatcher rather than retrying."
            : Truncate(ex.Message);
}
