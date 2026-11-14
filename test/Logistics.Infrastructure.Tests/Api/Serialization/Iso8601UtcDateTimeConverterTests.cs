using System.Text.Json;
using Logistics.API.Serialization;

namespace Logistics.Infrastructure.Tests.Api.Serialization;

public class Iso8601UtcDateTimeConverterTests
{
    private static readonly JsonSerializerOptions Options = new() { Converters = { new Iso8601UtcDateTimeConverter() } };

    [Theory]
    [InlineData("\"2026-03-01T10:00:00\"")]
    [InlineData("\"2026-03-01T05:00:00-05:00\"")]
    public void Read_IsoDate_ReturnsUtc(string json)
    {
        var value = JsonSerializer.Deserialize<DateTime>(json, Options);

        Assert.Equal(new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc), value);
        Assert.Equal(DateTimeKind.Utc, value.Kind);
    }

    [Fact]
    public void Read_NotADate_ThrowsJsonException()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<DateTime>("\"soon\"", Options));
    }
}
