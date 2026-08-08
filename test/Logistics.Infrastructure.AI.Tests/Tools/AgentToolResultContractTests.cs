using System.Text.Json;
using Logistics.Shared.Models;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools;

/// <summary>
/// The shape is a contract in two directions: the model reads the snake_case keys its prompt names,
/// and the UI reads the same type off the decision DTO. These pin both ends against a rename.
/// </summary>
public class AgentToolResultContractTests
{
    [Fact]
    public void Serialize_UsesTheSnakeCaseKeysTheSystemPromptsName()
    {
        var json = AgentToolResultJson.Serialize(new AgentToolResultDto
        {
            Feasible = true,
            EstimatedDrivingMinutes = 120,
            DrivingMinutesRemaining = 400,
            FleetSummary = new AgentToolFleetSummaryDto { TotalTrucks = 9, DriversInViolation = 1 }
        });

        Assert.Contains("\"estimated_driving_minutes\":120", json);
        Assert.Contains("\"driving_minutes_remaining\":400", json);
        Assert.Contains("\"fleet_summary\":", json);
        Assert.Contains("\"total_trucks\":9", json);
        Assert.Contains("\"drivers_in_violation\":1", json);
    }

    /// <summary>A tool's payload must carry its own keys only, not the whole union padded with nulls.</summary>
    [Fact]
    public void Serialize_OmitsUnionMembersTheToolDidNotSet()
    {
        var json = AgentToolResultJson.Serialize(new AgentToolResultDto { Feasible = false, Reason = "no hours" });

        Assert.Equal("""{"feasible":false,"reason":"no hours"}""", json);
    }

    /// <summary>"No driver" is information; an absent key leaves the model to infer it.</summary>
    [Fact]
    public void Serialize_NestedRecord_StillWritesItsNulls()
    {
        var json = AgentToolResultJson.Serialize(new AgentToolResultDto
        {
            Trucks = [new AgentToolTruckDto { Number = "T-1", MainDriver = null }]
        });

        Assert.Contains("\"main_driver\":null", json);
        Assert.Contains("\"current_address\":null", json);
    }

    [Fact]
    public void RoundTrip_PreservesNestedTruckAndDriverHos()
    {
        var original = new AgentToolResultDto
        {
            Count = 1,
            Trucks =
            [
                new AgentToolTruckDto
                {
                    Id = Guid.NewGuid(),
                    Number = "T-100",
                    CurrentAddress = "Dallas, TX",
                    MainDriver = new AgentToolDriverDto
                    {
                        Id = Guid.NewGuid(),
                        Name = "Jane Doe",
                        Hos = new AgentToolHosDto { DrivingMinutesRemaining = 300, IsAvailable = true }
                    }
                }
            ]
        };

        var restored = AgentToolResultJson.Deserialize(AgentToolResultJson.Serialize(original));

        var truck = Assert.Single(restored!.Trucks!);
        Assert.Equal("T-100", truck.Number);
        Assert.Equal("Jane Doe", truck.MainDriver!.Name);
        Assert.Equal(300, truck.MainDriver.Hos!.DrivingMinutesRemaining);
        Assert.True(truck.MainDriver.Hos.IsAvailable);
    }

    [Theory]
    [InlineData("""{"items":[],"count":0,"total":0,"truncated":false}""")]  // the paged search envelope
    [InlineData("""{"trip_id":"abc"}""")]
    [InlineData("[1,2,3]")]
    [InlineData("not json")]
    [InlineData("")]
    [InlineData(null)]
    public void Deserialize_PayloadTheUiCannotRender_IsNull(string? json)
    {
        Assert.Null(AgentToolResultJson.Deserialize(json));
    }

    [Theory]
    [InlineData("""{"error":"boom"}""")]
    [InlineData("""{"success":true}""")]
    [InlineData("""{"loads":[]}""")]
    [InlineData("""{"results":[]}""")]
    public void Deserialize_PayloadTheUiRenders_IsProjected(string json)
    {
        Assert.NotNull(AgentToolResultJson.Deserialize(json));
    }

    /// <summary>The write tools' failure arm still has to reach the transcript as an error.</summary>
    [Fact]
    public void Deserialize_WriteToolFailure_CarriesSuccessAndError()
    {
        var result = AgentToolResultJson.Deserialize("""{"success":false,"error":"Load not found"}""");

        Assert.False(result!.Success);
        Assert.Equal("Load not found", result.Error);
    }

    /// <summary>A tool reaching for the default options would emit PascalCase the model cannot read.</summary>
    [Fact]
    public void Deserialize_DefaultSerializerOutput_IsNotAccepted()
    {
        var wrong = JsonSerializer.Serialize(new AgentToolResultDto { Loads = [] });

        Assert.Null(AgentToolResultJson.Deserialize(wrong));
    }
}
