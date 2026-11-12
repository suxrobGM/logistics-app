namespace Logistics.Application.Abstractions.Agents;

/// <summary>
/// Strips credential and infrastructure detail from exception text before it reaches a
/// tenant-visible decision row, a conversation note, or the model's context window.
/// </summary>
public static class ToolErrorSanitizer
{
    private const int MaxLength = 500;

    private static readonly string[] SensitiveMarkers =
        ["api key", "authentication", "unauthorized", "password", "secret", "token=", "connection string"];

    public static bool LooksSensitive(string message) =>
        SensitiveMarkers.Any(marker => message.Contains(marker, StringComparison.OrdinalIgnoreCase));

    public static string Truncate(string message) =>
        message.Length > MaxLength ? message[..MaxLength] : message;

    /// <summary>
    /// For a failed tool. Never blames the LLM key: a tool talks to the database and vendor APIs, so
    /// an auth-shaped failure here is something else.
    /// </summary>
    public static string Sanitize(Exception ex) =>
        LooksSensitive(ex.Message)
            ? "The tool failed due to a configuration or authorization problem. Report this to the dispatcher rather than retrying."
            : Truncate(ex.Message);
}
