using Logistics.Domain.Persistence;
using Xunit;

namespace Logistics.Application.Tests.Persistence;

public class QueryableExtensionsTests
{
    private sealed record Customer(string Name);

    private sealed record Row(string Name, DateTime CreatedAt, Customer Customer);

    private static readonly Row Oldest = new("oldest", new DateTime(2026, 1, 1), new Customer("zeta"));
    private static readonly Row Newest = new("newest", new DateTime(2026, 3, 1), new Customer("alpha"));

    private static IQueryable<Row> Source() => new[] { Oldest, Newest }.AsQueryable();

    [Fact]
    public void OrderBy_DescendingField_SortsNewestFirst()
    {
        var ordered = Source().OrderBy("-CreatedAt").ToList();

        Assert.Equal([Newest, Oldest], ordered);
    }

    [Fact]
    public void OrderBy_AscendingField_SortsOldestFirst()
    {
        var ordered = Source().OrderBy("CreatedAt").ToList();

        Assert.Equal([Oldest, Newest], ordered);
    }

    [Fact]
    public void OrderBy_FieldNameCasingDiffers_StillMatches()
    {
        var ordered = Source().OrderBy("-createdat").ToList();

        Assert.Equal([Newest, Oldest], ordered);
    }

    /// <summary>
    /// List screens sort by a related entity's column ("Customer.Name") more often than by a
    /// scalar, so the whole dotted path has to resolve segment by segment.
    /// </summary>
    [Fact]
    public void OrderBy_NestedPath_SortsByTheRelatedProperty()
    {
        var ordered = Source().OrderBy("Customer.Name").ToList();

        Assert.Equal([Newest, Oldest], ordered);
    }

    [Fact]
    public void OrderBy_NestedPathDescending_SortsByTheRelatedProperty()
    {
        var ordered = Source().OrderBy("-customer.name").ToList();

        Assert.Equal([Oldest, Newest], ordered);
    }

    /// <summary>
    /// The DTO exposes CreatedDate while the entity has CreatedAt, so callers pass the wrong name
    /// often enough that this must not throw - it used to surface as a dynamic-LINQ parse error.
    /// </summary>
    [Theory]
    [InlineData("-CreatedDate")]
    [InlineData("NotAProperty")]
    [InlineData("Customer.NotAProperty")]
    [InlineData("NotAProperty.Name")]
    public void OrderBy_UnknownField_LeavesOrderUntouched(string orderBy)
    {
        var ordered = Source().OrderBy(orderBy).ToList();

        Assert.Equal([Oldest, Newest], ordered);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void OrderBy_NoField_LeavesOrderUntouched(string? orderBy)
    {
        var ordered = Source().OrderBy(orderBy).ToList();

        Assert.Equal([Oldest, Newest], ordered);
    }
}
