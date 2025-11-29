using System.Text.Json;
using System.Text.Json.Serialization;

namespace Logistics.API.Converters;

/// <summary>
/// JSON converter to handle DateTime in ISO 8601 format with UTC timezone.
/// </summary>
public class Iso8601UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateString = reader.GetString();
        if (string.IsNullOrEmpty(dateString))
        {
            return default;
        }

        return DateTime.Parse(dateString).ToUniversalTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Always output in UTC with "Z" suffix
        writer.WriteStringValue(value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }
}
