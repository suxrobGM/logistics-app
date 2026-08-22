using System.Globalization;
using System.Text.Json.Nodes;
using Logistics.Application.Abstractions.Agents;
using Xunit;

namespace Logistics.Application.Tests.Agents;

public class ToolInputTests
{
    private static readonly JsonNode Input = JsonNode.Parse("""{"rate": 1234.56, "id": "not-a-guid"}""")!;

    private static T UnderCulture<T>(string culture, Func<T> read)
    {
        var original = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo(culture);

        try
        {
            return read();
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    /// <summary>
    /// The JSON was written invariant, so it must be read invariant: a comma-decimal culture parses
    /// "1234.56" as 123456 and the recorded rate comes back a hundred times too big.
    /// </summary>
    [Theory]
    [InlineData("en-US")]
    [InlineData("de-DE")]
    [InlineData("ru-RU")]
    public void GetDecimal_AnyAmbientCulture_ReadsTheInvariantNumber(string culture)
    {
        Assert.Equal(1234.56m, UnderCulture(culture, () => Input.GetDecimal("rate")));
    }

    [Fact]
    public void GetDecimal_MissingKey_ReturnsNull()
    {
        Assert.Null(Input.GetDecimal("missing"));
    }

    [Fact]
    public void GetGuid_UnparseableValue_ReturnsNull()
    {
        Assert.Null(Input.GetGuid("id"));
    }
}
