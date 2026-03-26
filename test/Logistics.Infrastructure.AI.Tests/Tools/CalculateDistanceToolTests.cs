using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Infrastructure.AI.Tools;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools;

public class CalculateDistanceToolTests
{
    private readonly CalculateDistanceTool sut = new();

    [Fact]
    public async Task Execute_ValidCoordinates_ReturnsDistanceAndDuration()
    {
        // New York (40.7128, -74.0060) to Los Angeles (34.0522, -118.2437)
        var input = new JsonObject
        {
            ["origin_lat"] = 40.7128,
            ["origin_lng"] = -74.0060,
            ["dest_lat"] = 34.0522,
            ["dest_lng"] = -118.2437
        };

        var result = await sut.ExecuteAsync(input, CancellationToken.None);
        var json = JsonDocument.Parse(result);

        Assert.True(json.RootElement.TryGetProperty("straight_line_km", out var straightLine));
        Assert.True(json.RootElement.TryGetProperty("estimated_driving_km", out var drivingKm));
        Assert.True(json.RootElement.TryGetProperty("estimated_minutes", out var minutes));

        // NY to LA straight line is ~3,940 km
        Assert.InRange(straightLine.GetDouble(), 3500, 4500);
        // Driving distance should be ~1.3x straight line
        Assert.True(drivingKm.GetDouble() > straightLine.GetDouble());
        Assert.True(minutes.GetInt32() > 0);
    }

    [Fact]
    public async Task Execute_SamePoint_ReturnsZeroDistance()
    {
        var input = new JsonObject
        {
            ["origin_lat"] = 40.0,
            ["origin_lng"] = -74.0,
            ["dest_lat"] = 40.0,
            ["dest_lng"] = -74.0
        };

        var result = await sut.ExecuteAsync(input, CancellationToken.None);
        var json = JsonDocument.Parse(result);

        Assert.Equal(0, json.RootElement.GetProperty("straight_line_km").GetDouble());
        Assert.Equal(0, json.RootElement.GetProperty("estimated_minutes").GetInt32());
    }

    [Fact]
    public async Task Execute_MissingCoordinates_ReturnsError()
    {
        var input = new JsonObject
        {
            ["origin_lat"] = 40.0
            // Missing other coordinates
        };

        var result = await sut.ExecuteAsync(input, CancellationToken.None);
        var json = JsonDocument.Parse(result);

        Assert.True(json.RootElement.TryGetProperty("error", out var error));
        Assert.Contains("Missing required coordinate", error.GetString());
    }

    [Fact]
    public async Task Execute_EmptyInput_ReturnsError()
    {
        var result = await sut.ExecuteAsync(new JsonObject(), CancellationToken.None);
        var json = JsonDocument.Parse(result);

        Assert.True(json.RootElement.TryGetProperty("error", out _));
    }

    [Fact]
    public void Name_ReturnsCorrectToolName()
    {
        Assert.Equal("calculate_distance", sut.Name);
    }
}
