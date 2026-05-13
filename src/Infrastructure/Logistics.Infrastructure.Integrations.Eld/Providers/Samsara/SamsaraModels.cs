namespace Logistics.Infrastructure.Integrations.Eld.Providers.Samsara;

// Samsara wire format is camelCase — see EldJsonOptions.CamelCase. No [JsonPropertyName] needed.

internal record SamsaraHosClockResponse(SamsaraHosClockData? Data);

internal record SamsaraHosClocksResponse(List<SamsaraHosClockData>? Data);

internal record SamsaraHosLogsResponse(List<SamsaraHosLogData>? Data);

internal record SamsaraViolationsResponse(List<SamsaraViolationData>? Data);

internal record SamsaraDriversResponse(List<SamsaraDriverData>? Data);

internal record SamsaraVehiclesResponse(List<SamsaraVehicleData>? Data);

internal record SamsaraHosClockData
{
    public SamsaraDriverRef? Driver { get; init; }
    public string? CurrentDutyStatus { get; init; }
    public long? CurrentDutyStatusStartMs { get; init; }
    public long? DrivingTimeRemainingMs { get; init; }
    public long? ShiftTimeRemainingMs { get; init; }
    public long? CycleTimeRemainingMs { get; init; }
    public long? BreakTimeRemainingMs { get; init; }
    public List<object>? CurrentViolations { get; init; }
}

internal record SamsaraDriverRef
{
    public string? Id { get; init; }
    public string? Name { get; init; }
}

internal record SamsaraHosLogData
{
    public string? Id { get; init; }
    public SamsaraDriverRef? Driver { get; init; }
    public string? DutyStatus { get; init; }
    public long? StartMs { get; init; }
    public long? EndMs { get; init; }
    public SamsaraLocation? Location { get; init; }
    public string? Remark { get; init; }
}

internal record SamsaraLocation
{
    public string? Name { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
}

internal record SamsaraViolationData
{
    public string? Id { get; init; }
    public string? ViolationType { get; init; }
    public long? StartMs { get; init; }
}

internal record SamsaraDriverData
{
    public string? Id { get; init; }
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? LicenseNumber { get; init; }
}

internal record SamsaraVehicleData
{
    public string? Id { get; init; }
    public string? Name { get; init; }
    public string? Vin { get; init; }
    public string? LicensePlate { get; init; }
    public string? Make { get; init; }
    public string? Model { get; init; }
    public int? Year { get; init; }
}

internal record SamsaraWebhookPayload
{
    public string? EventType { get; init; }
    public SamsaraWebhookData? Data { get; init; }
}

internal record SamsaraWebhookData
{
    public string? DriverId { get; init; }
    public string? VehicleId { get; init; }
}
