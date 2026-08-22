using System.ComponentModel;
using System.Text.Json.Nodes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Tools;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools;

/// <summary>Pins what schema generation and binding produce, since every tool depends on both.</summary>
public class AgentToolJsonTests
{
    private sealed record Sample
    {
        [Description("The load ID")]
        public required Guid LoadId { get; init; }

        [Description("How far")]
        public double? DistanceKm { get; init; }

        [Description("Filter by status")]
        public LoadStatus? Status { get; init; }

        [Description("Cut-off")]
        public DateTime? StartDate { get; init; }
    }

    private static JsonObject Schema => (JsonObject)AgentToolJson.SchemaFor(typeof(Sample));

    private static JsonObject Properties => (JsonObject)Schema["properties"]!;

    [Fact]
    public void Schema_UsesSnakeCasePropertyNames()
    {
        Assert.True(Properties.ContainsKey("load_id"));
        Assert.True(Properties.ContainsKey("distance_km"));
    }

    [Fact]
    public void Schema_CarriesTheDescriptionAttribute()
    {
        Assert.Equal("The load ID", Properties["load_id"]!["description"]!.GetValue<string>());
    }

    [Fact]
    public void Schema_MarksRequiredMembersRequired()
    {
        var required = ((JsonArray)Schema["required"]!).Select(n => n!.GetValue<string>()).ToList();

        Assert.Equal(["load_id"], required);
    }

    [Fact]
    public void Schema_ListsEnumValues_RatherThanDescribingThemInProse()
    {
        var values = ((JsonArray)Properties["status"]!["enum"]!).Select(n => n!.GetValue<string>()).ToList();

        Assert.Contains(nameof(LoadStatus.Delivered), values);
    }

    [Fact]
    public void Schema_DescribesDatesAsDateTimeStrings()
    {
        Assert.Equal("string", Properties["start_date"]!["type"]!.GetValue<string>());
        Assert.Equal("date-time", Properties["start_date"]!["format"]!.GetValue<string>());
    }

    [Fact]
    public void Schema_DropsNullFromOptionalTypes()
    {
        Assert.Equal("number", Properties["distance_km"]!["type"]!.GetValue<string>());
    }

    [Fact]
    public void Bind_AcceptsQuotedNumbersAndOddCasing()
    {
        var input = JsonNode.Parse("""{"Load_Id": "8a1a1f1e-0000-0000-0000-000000000001", "DISTANCE_KM": "120.5"}""")!;

        Assert.True(AgentToolJson.TryBind<Sample>(input, out var value, out _));
        Assert.Equal(120.5, value!.DistanceKm);
    }

    [Fact]
    public void Bind_ReadsEnumsCaseInsensitively()
    {
        var input = JsonNode.Parse("""{"load_id": "8a1a1f1e-0000-0000-0000-000000000001", "status": "delivered"}""")!;

        Assert.True(AgentToolJson.TryBind<Sample>(input, out var value, out _));
        Assert.Equal(LoadStatus.Delivered, value!.Status);
    }

    [Fact]
    public void Bind_LabelsOffsetlessDatesUtc()
    {
        var input = JsonNode.Parse("""{"load_id": "8a1a1f1e-0000-0000-0000-000000000001", "start_date": "2026-03-01"}""")!;

        Assert.True(AgentToolJson.TryBind<Sample>(input, out var value, out _));
        Assert.Equal(DateTimeKind.Utc, value!.StartDate!.Value.Kind);
        Assert.Equal(new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc), value.StartDate);
    }

    [Fact]
    public void Bind_ConvertsDatesThatCarryAnOffset()
    {
        var input = JsonNode.Parse("""{"load_id": "8a1a1f1e-0000-0000-0000-000000000001", "start_date": "2026-03-01T10:00:00+02:00"}""")!;

        Assert.True(AgentToolJson.TryBind<Sample>(input, out var value, out _));
        Assert.Equal(new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc), value!.StartDate);
    }

    [Fact]
    public void Bind_MissingRequiredProperty_NamesIt()
    {
        Assert.False(AgentToolJson.TryBind<Sample>(new JsonObject(), out _, out var error));
        Assert.Contains("load_id", error);
    }

    [Fact]
    public void Bind_UnreadableValue_PointsAtTheProperty()
    {
        var input = JsonNode.Parse("""{"load_id": "not-a-guid"}""")!;

        Assert.False(AgentToolJson.TryBind<Sample>(input, out _, out var error));
        Assert.Contains("load_id", error);
    }
}
