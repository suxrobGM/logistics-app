using System.Text.Json;
using System.Text.Json.Serialization;

namespace Logistics.Shared.Models;

/// <summary>
/// The one declaration of what an agent tool returns: serialized snake_case for the model, and
/// reaching the UI as generated camelCase on <see cref="AgentDecisionDto.ToolResult"/>. Declaring
/// it twice is what let prompt tuning silently blank the dispatch transcript. Every member is
/// optional - this is the union of what any tool can return, and a tool sets only its own keys.
/// </summary>
public class AgentToolResultDto
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Success { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Count { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Feasible { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? FeasibleMultiDay { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Reason { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? EstimatedDrivingMinutes { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? DrivingMinutesRemaining { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? OnDutyMinutesRemaining { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsInViolation { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public AgentToolFleetSummaryDto? FleetSummary { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<AgentToolLoadDto>? Loads { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<AgentToolTruckDto>? Trucks { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<AgentToolHosCheckDto>? Results { get; set; }
}

public class AgentToolFleetSummaryDto
{
    public int TotalTrucks { get; set; }
    public int AvailableTrucks { get; set; }
    public int ActiveTrips { get; set; }
    public int DriversInViolation { get; set; }
}

public class AgentToolLoadDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Origin { get; set; }
    public string? Destination { get; set; }
    public double? OriginLat { get; set; }
    public double? OriginLng { get; set; }
    public double? DestLat { get; set; }
    public double? DestLng { get; set; }
    public double DistanceKm { get; set; }
    public decimal DeliveryCost { get; set; }
    public string? Customer { get; set; }

    // Without these the agent cannot tell a load has a container, so it never calls the intermodal tools.
    public string? ContainerNumber { get; set; }
    public string? ContainerIsoType { get; set; }
    public string? OriginTerminal { get; set; }
    public string? DestinationTerminal { get; set; }
}

public class AgentToolTruckDto
{
    public Guid Id { get; set; }
    public string? Number { get; set; }
    public string? Type { get; set; }
    public double? CurrentLat { get; set; }
    public double? CurrentLng { get; set; }
    public string? CurrentAddress { get; set; }
    public AgentToolDriverDto? MainDriver { get; set; }
}

public class AgentToolDriverDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public AgentToolHosDto? Hos { get; set; }
}

public class AgentToolHosDto
{
    public int DrivingMinutesRemaining { get; set; }
    public int OnDutyMinutesRemaining { get; set; }
    public int CycleMinutesRemaining { get; set; }
    public bool IsInViolation { get; set; }
    public bool IsAvailable { get; set; }
}

public class AgentToolHosCheckDto
{
    public Guid DriverId { get; set; }
    public double DistanceKm { get; set; }
    public bool Feasible { get; set; }
    public bool FeasibleMultiDay { get; set; }
    public int EstimatedDrivingMinutes { get; set; }
    public int? DrivingMinutesRemaining { get; set; }
    public int? OnDutyMinutesRemaining { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// The model-facing wire format: snake_case, because the system prompts name the keys verbatim.
/// Nested records still write their nulls - an explicit <c>"main_driver": null</c> tells the model
/// the truck has no driver, where an absent key leaves it to infer one.
/// </summary>
public static class AgentToolResultJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public static string Serialize(AgentToolResultDto result) =>
        JsonSerializer.Serialize(result, Options);

    /// <summary>
    /// Keys worth projecting. Without this gate every unrelated tool payload deserializes into an
    /// all-null object, since unknown properties are simply skipped.
    /// </summary>
    private static readonly string[] RenderedKeys =
        ["error", "success", "feasible", "loads", "trucks", "results", "fleet_summary"];

    /// <summary>
    /// A persisted tool output as the typed shape, or null when this union does not model it -
    /// the normal case, since most tools' output the UI never renders.
    /// </summary>
    public static AgentToolResultDto? Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind is not JsonValueKind.Object)
                return null;

            if (!RenderedKeys.Any(key => document.RootElement.TryGetProperty(key, out _)))
                return null;

            return JsonSerializer.Deserialize<AgentToolResultDto>(json, Options);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
