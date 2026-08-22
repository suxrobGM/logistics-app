using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Logistics.Infrastructure.AI.Tools;

/// <summary>
/// Reads a number or boolean into a string property. Models emit an unquoted value for a text field
/// often enough - a trip named 2026 - that failing the call over it costs a turn for nothing.
/// </summary>
internal sealed class ScalarToStringConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => Number(ref reader),
            JsonTokenType.True => "true",
            JsonTokenType.False => "false",
            JsonTokenType.Null => null,
            _ => throw new JsonException("Expected a string.")
        };

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value);

    private static string Number(ref Utf8JsonReader reader) =>
        reader.TryGetDecimal(out var value)
            ? value.ToString(CultureInfo.InvariantCulture)
            : reader.GetDouble().ToString(CultureInfo.InvariantCulture);
}
