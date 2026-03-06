using System.Text.Json.Serialization;

namespace Logistics.Infrastructure.Integrations.Eld.Samsara;

internal record SamsaraHosClockResponse(SamsaraHosClockData? Data);

internal record SamsaraHosClocksResponse(List<SamsaraHosClockData>? Data);

internal record SamsaraHosLogsResponse(List<SamsaraHosLogData>? Data);

internal record SamsaraViolationsResponse(List<SamsaraViolationData>? Data);

internal record SamsaraDriversResponse(List<SamsaraDriverData>? Data);

internal record SamsaraVehiclesResponse(List<SamsaraVehicleData>? Data);

internal record SamsaraHosClockData
{
    [JsonPropertyName("driver")]
    public SamsaraDriverRef? Driver { get; init; }

    [JsonPropertyName("currentDutyStatus")]
    public string? CurrentDutyStatus { get; init; }

    [JsonPropertyName("currentDutyStatusStartMs")]
    public long? CurrentDutyStatusStartMs { get; init; }

    [JsonPropertyName("drivingTimeRemainingMs")]
    public long? DrivingTimeRemainingMs { get; init; }

    [JsonPropertyName("shiftTimeRemainingMs")]
    public long? ShiftTimeRemainingMs { get; init; }

    [JsonPropertyName("cycleTimeRemainingMs")]
    public long? CycleTimeRemainingMs { get; init; }

    [JsonPropertyName("breakTimeRemainingMs")]
    public long? BreakTimeRemainingMs { get; init; }

    [JsonPropertyName("currentViolations")]
    public List<object>? CurrentViolations { get; init; }
}

internal record SamsaraDriverRef
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }
}

internal record SamsaraHosLogData
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("driver")]
    public SamsaraDriverRef? Driver { get; init; }

    [JsonPropertyName("dutyStatus")]
    public string? DutyStatus { get; init; }

    [JsonPropertyName("startMs")]
    public long? StartMs { get; init; }

    [JsonPropertyName("endMs")]
    public long? EndMs { get; init; }

    [JsonPropertyName("location")]
    public SamsaraLocation? Location { get; init; }

    [JsonPropertyName("remark")]
    public string? Remark { get; init; }
}

internal record SamsaraLocation
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("latitude")]
    public double? Latitude { get; init; }

    [JsonPropertyName("longitude")]
    public double? Longitude { get; init; }
}

internal record SamsaraViolationData
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("violationType")]
    public string? ViolationType { get; init; }

    [JsonPropertyName("startMs")]
    public long? StartMs { get; init; }
}

internal record SamsaraDriverData
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("email")]
    public string? Email { get; init; }

    [JsonPropertyName("phone")]
    public string? Phone { get; init; }

    [JsonPropertyName("licenseNumber")]
    public string? LicenseNumber { get; init; }
}

internal record SamsaraVehicleData
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("vin")]
    public string? Vin { get; init; }

    [JsonPropertyName("licensePlate")]
    public string? LicensePlate { get; init; }

    [JsonPropertyName("make")]
    public string? Make { get; init; }

    [JsonPropertyName("model")]
    public string? Model { get; init; }

    [JsonPropertyName("year")]
    public int? Year { get; init; }
}

internal record SamsaraWebhookPayload
{
    [JsonPropertyName("eventType")]
    public string? EventType { get; init; }

    [JsonPropertyName("data")]
    public SamsaraWebhookData? Data { get; init; }
}

internal record SamsaraWebhookData
{
    [JsonPropertyName("driverId")]
    public string? DriverId { get; init; }

    [JsonPropertyName("vehicleId")]
    public string? VehicleId { get; init; }
}
