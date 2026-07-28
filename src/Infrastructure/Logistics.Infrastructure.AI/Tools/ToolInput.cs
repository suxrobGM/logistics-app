using System.Text.Json.Nodes;

namespace Logistics.Infrastructure.AI.Tools;

/// <summary>
/// Lenient accessors for LLM-authored tool input. Models emit inconsistent JSON (numbers as
/// strings, wrong casing), so every accessor coerces where it can and returns null instead of throwing.
/// </summary>
internal static class ToolInput
{
    public static string? GetString(this JsonNode input, string key) =>
        input[key] is JsonValue value ? value.ToString() : null;

    public static Guid? GetGuid(this JsonNode input, string key) =>
        Guid.TryParse(input.GetString(key), out var guid) ? guid : null;

    public static int? GetInt(this JsonNode input, string key) =>
        int.TryParse(input.GetString(key), out var number) ? number : null;

    public static decimal? GetDecimal(this JsonNode input, string key) =>
        decimal.TryParse(input.GetString(key), out var number) ? number : null;

    public static bool? GetBool(this JsonNode input, string key) =>
        bool.TryParse(input.GetString(key), out var flag) ? flag : null;

    public static DateTime? GetDate(this JsonNode input, string key) =>
        DateTime.TryParse(input.GetString(key), out var date)
            ? DateTime.SpecifyKind(date, DateTimeKind.Utc)
            : null;

    public static TEnum? GetEnum<TEnum>(this JsonNode input, string key) where TEnum : struct, Enum =>
        Enum.TryParse<TEnum>(input.GetString(key), ignoreCase: true, out var value) ? value : null;

    public static TEnum[]? GetEnumArray<TEnum>(this JsonNode input, string key) where TEnum : struct, Enum
    {
        if (input[key] is not JsonArray array)
            return null;

        var values = array
            .Select(item => Enum.TryParse<TEnum>(item?.ToString(), ignoreCase: true, out var value)
                ? value
                : (TEnum?)null)
            .Where(v => v is not null)
            .Select(v => v!.Value)
            .ToArray();

        return values.Length > 0 ? values : null;
    }
}
