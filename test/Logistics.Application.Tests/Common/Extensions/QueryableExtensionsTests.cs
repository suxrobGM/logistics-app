using Logistics.Application.Common.Extensions;
using Logistics.Domain.Persistence;
using Xunit;

namespace Logistics.Application.Tests.Common.Extensions;

public class QueryableExtensionsTests
{
    private sealed record Customer(string Name);

    private sealed record Row(string Name, DateTime CreatedAt, Customer Customer);

    private static readonly Row Oldest = new("oldest", new DateTime(2026, 1, 1), new Customer("zeta"));
    private static readonly Row Newest = new("newest", new DateTime(2026, 3, 1), new Customer("alpha"));

    /// <summary>Yields Oldest first, so an order left untouched reads as "oldest first".</summary>
    private static IQueryable<Row> Source() => new[] { Oldest, Newest }.AsQueryable();

    /// <summary>
    /// Field names match case-insensitively, and list screens sort by a related entity's column
    /// ("Customer.Name") as often as by a scalar. An unknown field - the DTO says CreatedDate where
    /// the entity has CreatedAt - falls back to the tie-breaker instead of throwing a parse error.
    /// </summary>
    [Theory]
    [InlineData("-CreatedAt", true)]
    [InlineData("CreatedAt", false)]
    [InlineData("-createdat", true)]
    [InlineData("Customer.Name", true)]
    [InlineData("-customer.name", false)]
    [InlineData("-CreatedDate", false)]
    [InlineData("NotAProperty", false)]
    [InlineData("Customer.NotAProperty", false)]
    [InlineData("NotAProperty.Name", false)]
    [InlineData(null, false)]
    [InlineData("", false)]
    public void OrderBy_SortField_OrdersOrFallsBackToTieBreaker(string? orderBy, bool newestFirst)
    {
        var ordered = Source().OrderBy(orderBy, r => r.CreatedAt).ToList();

        Row[] expected = newestFirst ? [Newest, Oldest] : [Oldest, Newest];
        Assert.Equal(expected, ordered);
    }

    /// <summary>
    /// List screens send an empty sort on their first page, so the default sort and then the
    /// tie-breaker (by Name, which puts Newest first) are the common path.
    /// </summary>
    [Theory]
    [InlineData(null, "-CreatedAt", true)]
    [InlineData("", "-CreatedAt", true)]
    [InlineData("CreatedAt", "-CreatedAt", false)]
    [InlineData(null, null, true)]
    [InlineData("", null, true)]
    [InlineData("NotAProperty", null, true)]
    [InlineData("CreatedAt", null, false)]
    public void OrderByWithTieBreaker_SortField_FallsBackToDefaultThenTieBreaker(
        string? orderBy, string? defaultOrderBy, bool newestFirst)
    {
        var ordered = Source().OrderBy(orderBy, r => r.Name, defaultOrderBy: defaultOrderBy).ToList();

        Row[] expected = newestFirst ? [Newest, Oldest] : [Oldest, Newest];
        Assert.Equal(expected, ordered);
    }

    // A requested sort is rarely unique, so the tie-breaker applies on the success path too.
    [Fact]
    public void OrderByWithTieBreaker_RequestedSortHasTies_BreaksThemByTheTieBreaker()
    {
        var sameDay = new DateTime(2026, 2, 1);
        var second = new Row("b", sameDay, new Customer("x"));
        var first = new Row("a", sameDay, new Customer("x"));

        var ordered = new[] { second, first }.AsQueryable()
            .OrderBy("CreatedAt", r => r.Name)
            .ToList();

        Assert.Equal([first, second], ordered);
    }
}
