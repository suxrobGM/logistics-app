using System.Text.Json.Serialization;

namespace Logistics.Infrastructure.Integrations.Eld.Motive;

internal record MotiveHosResponse
{
    [JsonPropertyName("hours_of_service")]
    public List<MotiveHosData>? HoursOfService { get; init; }
}

internal record MotiveHosData
{
    [JsonPropertyName("driver")]
    public MotiveDriverRef? Driver { get; init; }

    [JsonPropertyName("current_duty_status")]
    public string? CurrentDutyStatus { get; init; }

    [JsonPropertyName("current_duty_status_start_time")]
    public DateTime? CurrentDutyStatusStartTime { get; init; }

    [JsonPropertyName("driving_time_remaining")]
    public int? DrivingTimeRemaining { get; init; }

    [JsonPropertyName("shift_time_remaining")]
    public int? ShiftTimeRemaining { get; init; }

    [JsonPropertyName("cycle_time_remaining")]
    public int? CycleTimeRemaining { get; init; }

    [JsonPropertyName("break_time_remaining")]
    public int? BreakTimeRemaining { get; init; }

    [JsonPropertyName("violation_count")]
    public int ViolationCount { get; init; }
}

internal record MotiveDriverRef
{
    [JsonPropertyName("id")]
    public int? Id { get; init; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; init; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; init; }
}

internal record MotiveDriverLogsResponse
{
    [JsonPropertyName("driver_logs")]
    public List<MotiveDriverLog>? DriverLogs { get; init; }
}

internal record MotiveDriverLog
{
    [JsonPropertyName("log_date")]
    public string? LogDate { get; init; }

    [JsonPropertyName("events")]
    public List<MotiveLogEvent>? Events { get; init; }
}

internal record MotiveLogEvent
{
    [JsonPropertyName("id")]
    public int? Id { get; init; }

    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("start_time")]
    public DateTime? StartTime { get; init; }

    [JsonPropertyName("end_time")]
    public DateTime? EndTime { get; init; }

    [JsonPropertyName("duration")]
    public int? Duration { get; init; }

    [JsonPropertyName("location")]
    public string? Location { get; init; }

    [JsonPropertyName("lat")]
    public double? Lat { get; init; }

    [JsonPropertyName("lon")]
    public double? Lon { get; init; }

    [JsonPropertyName("annotation")]
    public string? Annotation { get; init; }
}

internal record MotiveViolationsResponse
{
    [JsonPropertyName("hos_violations")]
    public List<MotiveViolationData>? HosViolations { get; init; }
}

internal record MotiveViolationData
{
    [JsonPropertyName("id")]
    public int? Id { get; init; }

    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("start_time")]
    public DateTime? StartTime { get; init; }
}

internal record MotiveUsersResponse
{
    [JsonPropertyName("users")]
    public List<MotiveUserData>? Users { get; init; }
}

internal record MotiveUserData
{
    [JsonPropertyName("id")]
    public int? Id { get; init; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; init; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; init; }

    [JsonPropertyName("email")]
    public string? Email { get; init; }

    [JsonPropertyName("phone")]
    public string? Phone { get; init; }

    [JsonPropertyName("driver_license_number")]
    public string? DriverLicenseNumber { get; init; }
}

internal record MotiveVehiclesResponse
{
    [JsonPropertyName("vehicles")]
    public List<MotiveVehicleData>? Vehicles { get; init; }
}

internal record MotiveVehicleData
{
    [JsonPropertyName("id")]
    public int? Id { get; init; }

    [JsonPropertyName("number")]
    public string? Number { get; init; }

    [JsonPropertyName("vin")]
    public string? Vin { get; init; }

    [JsonPropertyName("license_plate_number")]
    public string? LicensePlateNumber { get; init; }

    [JsonPropertyName("make")]
    public string? Make { get; init; }

    [JsonPropertyName("model")]
    public string? Model { get; init; }

    [JsonPropertyName("year")]
    public int? Year { get; init; }
}

internal record MotiveWebhookPayload
{
    [JsonPropertyName("event_type")]
    public string? EventType { get; init; }

    [JsonPropertyName("object_id")]
    public int? ObjectId { get; init; }
}
