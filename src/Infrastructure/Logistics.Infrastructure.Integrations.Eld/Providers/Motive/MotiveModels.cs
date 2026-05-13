namespace Logistics.Infrastructure.Integrations.Eld.Providers.Motive;

// Motive wire format is snake_case — see EldJsonOptions.SnakeCase. No [JsonPropertyName] needed.

internal record MotiveHosResponse
{
    public List<MotiveHosData>? HoursOfService { get; init; }
}

internal record MotiveHosData
{
    public MotiveDriverRef? Driver { get; init; }
    public string? CurrentDutyStatus { get; init; }
    public DateTime? CurrentDutyStatusStartTime { get; init; }
    public int? DrivingTimeRemaining { get; init; }
    public int? ShiftTimeRemaining { get; init; }
    public int? CycleTimeRemaining { get; init; }
    public int? BreakTimeRemaining { get; init; }
    public int ViolationCount { get; init; }
}

internal record MotiveDriverRef
{
    public int? Id { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
}

internal record MotiveDriverLogsResponse
{
    public List<MotiveDriverLog>? DriverLogs { get; init; }
}

internal record MotiveDriverLog
{
    public string? LogDate { get; init; }
    public List<MotiveLogEvent>? Events { get; init; }
}

internal record MotiveLogEvent
{
    public int? Id { get; init; }
    public string? Type { get; init; }
    public DateTime? StartTime { get; init; }
    public DateTime? EndTime { get; init; }
    public int? Duration { get; init; }
    public string? Location { get; init; }
    public double? Lat { get; init; }
    public double? Lon { get; init; }
    public string? Annotation { get; init; }
}

internal record MotiveViolationsResponse
{
    public List<MotiveViolationData>? HosViolations { get; init; }
}

internal record MotiveViolationData
{
    public int? Id { get; init; }
    public string? Type { get; init; }
    public DateTime? StartTime { get; init; }
}

internal record MotiveUsersResponse
{
    public List<MotiveUserData>? Users { get; init; }
}

internal record MotiveUserData
{
    public int? Id { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? DriverLicenseNumber { get; init; }
}

internal record MotiveVehiclesResponse
{
    public List<MotiveVehicleData>? Vehicles { get; init; }
}

internal record MotiveVehicleData
{
    public int? Id { get; init; }
    public string? Number { get; init; }
    public string? Vin { get; init; }
    public string? LicensePlateNumber { get; init; }
    public string? Make { get; init; }
    public string? Model { get; init; }
    public int? Year { get; init; }
}

internal record MotiveWebhookPayload
{
    public string? EventType { get; init; }
    public int? ObjectId { get; init; }
}
