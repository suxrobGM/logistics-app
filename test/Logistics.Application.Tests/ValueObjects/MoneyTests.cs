using Logistics.Domain.Primitives.ValueObjects;
using Xunit;

namespace Logistics.Application.Tests.ValueObjects;

public class MoneyTests
{
    public static TheoryData<decimal, string, string> FormatCases => new()
    {
        { 1234.5m, "USD", "$1,234.50" },
        { 1234.5m, "eur", "1,234.50 EUR" },
        { -20m, "USD", "-$20.00" }
    };

    /// <summary>Emails and messages go to tenants abroad, so the server's culture must never leak in.</summary>
    [Theory]
    [MemberData(nameof(FormatCases))]
    public void Format_IgnoresTheServerCulture(decimal amount, string currency, string expected)
    {
        var previous = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
        try
        {
            Assert.Equal(expected, Money.Format(amount, currency));
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = previous;
        }
    }
}
