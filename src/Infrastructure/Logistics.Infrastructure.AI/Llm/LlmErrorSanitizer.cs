namespace Logistics.Infrastructure.AI.Llm;

/// <summary>
/// Strips credential and infrastructure detail from exception text before it reaches a
/// tenant-visible session row, a decision row, or the model's context window.
/// </summary>
internal static class LlmErrorSanitizer
{
    private const int MaxLength = 500;

    private static readonly string[] SensitiveMarkers =
        ["api key", "authentication", "unauthorized", "password", "secret", "token=", "connection string"];

    private static bool LooksSensitive(string message) =>
        SensitiveMarkers.Any(marker => message.Contains(marker, StringComparison.OrdinalIgnoreCase));

    private static string Truncate(string message) =>
        message.Length > MaxLength ? message[..MaxLength] : message;

    /// <summary>
    /// For a failure of the LLM call itself. An auth-shaped failure here really does mean the
    /// provider credentials, so it is worth naming - it is the one thing an operator can fix.
    /// </summary>
    public static string ForSession(Exception ex)
    {
        // Already our own message, and deliberately not auth: a rate-limit window must not send an
        // operator off to audit credentials that were never the problem.
        if (ex is LlmRateLimitedException)
            return ex.Message;

        if (ex is HttpRequestException or UnauthorizedAccessException || LooksSensitive(ex.Message))
            return "API authentication error. Check the LLM API key configuration.";

        return Truncate(ex.Message);
    }

    /// <summary>
    /// For a failure inside a tool. Unlike <see cref="ForSession"/> this must NOT blame the LLM
    /// credentials - a tool talks to the database and to vendor APIs, so an auth-shaped failure
    /// here means something else entirely. Say only that it failed.
    /// </summary>
    public static string ForToolCall(Exception ex) =>
        LooksSensitive(ex.Message)
            ? "The tool failed due to a configuration or authorization problem. Report this to the dispatcher rather than retrying."
            : Truncate(ex.Message);
}
