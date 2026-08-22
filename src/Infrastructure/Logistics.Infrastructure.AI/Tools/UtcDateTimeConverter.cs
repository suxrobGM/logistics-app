using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Logistics.Infrastructure.AI.Tools;

/// <summary>
/// Tool dates are UTC downstream: one sent without an offset is labelled UTC, one with an offset is
/// converted rather than relabelled.
/// </summary>
internal sealed class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Expected a date string.");

        if (reader.TryGetDateTime(out var value))
            return ToUtc(value);

        if (DateTime.TryParse(reader.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out value))
            return ToUtc(value);

        throw new JsonException("Expected an ISO 8601 date.");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value);

    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
