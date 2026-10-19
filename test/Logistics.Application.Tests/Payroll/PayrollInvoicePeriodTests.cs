using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.ValueObjects;
using Xunit;

namespace Logistics.Application.Tests.Payroll;

public class PayrollInvoicePeriodTests
{
    private static PayrollInvoice Period(DateOnly start, DateOnly end) => new()
    {
        EmployeeId = Guid.NewGuid(),
        Subtotal = Money.Zero("USD"),
        TaxTotal = Money.Zero("USD"),
        Total = Money.Zero("USD"),
        PeriodStart = start,
        PeriodEnd = end
    };

    [Fact]
    public void PeriodUtcRange_CoversTheLastDayUpToMidnight()
    {
        var (from, to) = Period(new DateOnly(2026, 10, 4), new DateOnly(2026, 10, 10)).PeriodUtcRange();

        Assert.Equal(new DateTime(2026, 10, 4, 0, 0, 0, DateTimeKind.Utc), from);
        Assert.Equal(new DateTime(2026, 10, 11, 0, 0, 0, DateTimeKind.Utc), to);
        Assert.Equal(DateTimeKind.Utc, to.Kind);
    }

    [Theory]
    [InlineData("2026-10-04", "2026-10-10", 1)] // Sunday to Saturday
    [InlineData("2026-10-05", "2026-10-11", 1)] // Monday to Sunday
    [InlineData("2026-10-04", "2026-10-17", 2)]
    [InlineData("2026-10-04", "2026-10-14", 2)] // a partial week counts as a full one
    [InlineData("2026-10-05", "2026-10-15", 2)] // whatever weekday the period starts on
    [InlineData("2026-10-04", "2026-10-06", 1)]
    [InlineData("2026-10-05", "2026-10-07", 1)]
    [InlineData("2026-10-05", "2026-10-09", 1)] // Monday to Friday
    [InlineData("2026-10-07", "2026-10-07", 1)] // a single day
    [InlineData("2026-10-07", "2026-10-21", 3)] // 15 days
    public void PeriodWeeks_RoundsPartialWeeksUp(string start, string end, int expected)
    {
        Assert.Equal(expected, Period(DateOnly.Parse(start), DateOnly.Parse(end)).PeriodWeeks());
    }

    [Theory]
    [InlineData("2026-10-01", "2026-10-31", 1)]
    [InlineData("2026-09-01", "2026-11-30", 3)]
    [InlineData("2025-12-01", "2026-01-31", 2)]
    [InlineData("2026-10-15", "2026-11-15", 2)]
    [InlineData("2026-10-15", "2026-11-14", 1)] // the last month ends before the start day
    [InlineData("2026-10-15", "2026-10-20", 1)] // a partial month counts as a full one
    [InlineData("2026-10-20", "2026-11-25", 2)]
    public void PeriodMonths_CountsTheStartMonth(string start, string end, int expected)
    {
        Assert.Equal(expected, Period(DateOnly.Parse(start), DateOnly.Parse(end)).PeriodMonths());
    }
}
