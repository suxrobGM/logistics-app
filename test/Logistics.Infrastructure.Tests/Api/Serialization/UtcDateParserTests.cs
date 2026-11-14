using Logistics.API.Serialization;

namespace Logistics.Infrastructure.Tests.Api.Serialization;

public class UtcDateParserTests
{
    [Theory]
    [InlineData("2026-03-01T10:00:00Z")]
    [InlineData("2026-03-01T10:00:00")]
    [InlineData("2026-03-01T05:00:00-05:00")]
    [InlineData("2026-03-01T12:00:00+02:00")]
    public void TryParse_AnyIsoForm_ReturnsTheSameUtcInstant(string value)
    {
        Assert.True(UtcDateParser.TryParse(value, out var utc));

        Assert.Equal(DateTimeKind.Utc, utc.Kind);
        Assert.Equal(new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc), utc);
    }

    [Fact]
    public void TryParse_DateOnly_ReturnsUtcMidnight()
    {
        Assert.True(UtcDateParser.TryParse("2026-03-01", out var utc));

        Assert.Equal(new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc), utc);
        Assert.Equal(DateTimeKind.Utc, utc.Kind);
    }

    [Fact]
    public void TryParse_NotADate_ReturnsFalse()
    {
        Assert.False(UtcDateParser.TryParse("yesterday", out _));
    }
}
