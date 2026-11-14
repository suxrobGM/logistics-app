using FluentValidation;
using Logistics.Application.Common.Extensions;
using Xunit;

namespace Logistics.Application.Tests.Common.Extensions;

public class SortFieldsTests
{
    private sealed record Customer(string Name);

    private sealed record Row(string Name, DateTime CreatedAt, Customer Customer);

    private static readonly Row Oldest = new("oldest", new DateTime(2026, 1, 1), new Customer("zeta"));
    private static readonly Row Newest = new("newest", new DateTime(2026, 3, 1), new Customer("alpha"));

    private static readonly SortFields<Row> Sorts = new SortFields<Row>("-createdAt")
        .Add("createdAt", r => r.CreatedAt)
        .Add("customer.name", r => r.Customer.Name);

    /// <summary>Yields Oldest first, so an order left untouched reads as "oldest first".</summary>
    private static IQueryable<Row> Source() => new[] { Oldest, Newest }.AsQueryable();

    [Theory]
    [InlineData("-createdAt", true)]
    [InlineData("createdAt", false)]
    [InlineData("customer.name", true)]
    [InlineData("-customer.name", false)]
    [InlineData(null, true)]
    [InlineData("", true)]
    public void Apply_KnownKeyOrDefault_Orders(string? orderBy, bool newestFirst)
    {
        var ordered = Sorts.Apply(Source(), orderBy, r => r.Name).ToList();

        Row[] expected = newestFirst ? [Newest, Oldest] : [Oldest, Newest];
        Assert.Equal(expected, ordered);
    }

    /// <summary>Keys are the API contract, so a stale or misspelled key fails loudly instead of sorting by id.</summary>
    [Theory]
    [InlineData("CreatedAt")]
    [InlineData("-createdDate")]
    [InlineData("customer")]
    public void Apply_UnknownKey_ThrowsValidationError(string orderBy)
    {
        var error = Assert.Throws<ValidationException>(() => Sorts.Apply(Source(), orderBy, r => r.Name));

        Assert.Contains("createdAt, customer.name", error.Message);
    }

    [Fact]
    public void Apply_NoKeyAndNoDefault_OrdersByTieBreaker()
    {
        var ordered = new SortFields<Row>().Apply(Source(), null, r => r.Name).ToList();

        Assert.Equal([Newest, Oldest], ordered);
    }

    // A requested sort is rarely unique, so the tie-breaker applies on the success path too.
    [Fact]
    public void Apply_RequestedSortHasTies_BreaksThemByTheTieBreaker()
    {
        var sameDay = new DateTime(2026, 2, 1);
        var second = new Row("b", sameDay, new Customer("x"));
        var first = new Row("a", sameDay, new Customer("x"));

        var ordered = Sorts.Apply(new[] { second, first }.AsQueryable(), "createdAt", r => r.Name).ToList();

        Assert.Equal([first, second], ordered);
    }
}
