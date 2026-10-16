using Logistics.Domain.Extensions;
using Xunit;

namespace Logistics.Application.Tests.Platform;

public class DateTimeExtensionsTests
{
    /// <summary>
    /// A US-evening query: start is local midnight (05:00Z), now is 02:00Z a week later. The last
    /// UTC day must still get a bucket, or deliveries made on it are dropped from the KPIs.
    /// </summary>
    [Fact]
    public void DaysBetween_EndTimeEarlierInDayThanStart_IncludesLastDay()
    {
        var start = new DateTime(2026, 10, 9, 5, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 10, 16, 2, 0, 0, DateTimeKind.Utc);

        var days = start.DaysBetween(end).ToList();

        Assert.Equal(8, days.Count);
        Assert.Equal((2026, 10, 9), days[0]);
        Assert.Equal((2026, 10, 16), days[^1]);
    }
}
