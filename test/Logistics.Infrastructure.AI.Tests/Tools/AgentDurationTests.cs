using Logistics.Infrastructure.AI.Tools;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools;

public class AgentDurationTests
{
    [Theory]
    [InlineData(45, "45m")]
    [InlineData(120, "2h")]
    [InlineData(765, "12h 45m")]
    [InlineData(-90, "-1h 30m")]
    public void Format_QuotesHoursAndMinutes(int minutes, string expected)
    {
        Assert.Equal(expected, AgentDuration.Format(minutes));
    }
}
