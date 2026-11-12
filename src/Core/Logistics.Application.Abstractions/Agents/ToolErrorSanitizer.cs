namespace Logistics.Application.Abstractions.Agents;

/// <summary>Strips credentials and infrastructure detail before exception text reaches tenants or the model.</summary>
public static class ToolErrorSanitizer
{
    private const int MaxLength = 500;

    private static readonly string[] SensitiveMarkers =
        ["api key", "authentication", "unauthorized", "password", "secret", "token=", "connection string"];

    public static bool LooksSensitive(string message) =>
        SensitiveMarkers.Any(marker => message.Contains(marker, StringComparison.OrdinalIgnoreCase));

    public static string Truncate(string message) =>
        message.Length > MaxLength ? message[..MaxLength] : message;

    /// <summary>Never blames the LLM key: a tool's auth failure comes from the database or a vendor API.</summary>
    public static string Sanitize(Exception ex) =>
        LooksSensitive(ex.Message)
            ? "The tool failed due to a configuration or authorization problem. Report this to the dispatcher rather than retrying."
            : Truncate(ex.Message);
}
