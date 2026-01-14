using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

/// <summary>
/// HOS data retrieved from an ELD provider for a single driver
/// </summary>
public record EldDriverHosDataDto
{
    public required string ExternalDriverId { get; set; }
    public string? ExternalDriverName { get; set; }
    public required DutyStatus CurrentDutyStatus { get; set; }
    public DateTime StatusChangedAt { get; set; }
    public int DrivingMinutesRemaining { get; set; }
    public int OnDutyMinutesRemaining { get; set; }
    public int CycleMinutesRemaining { get; set; }
    public TimeSpan? TimeUntilBreakRequired { get; set; }
    public bool IsInViolation { get; set; }
    public DateTime? NextMandatoryBreakAt { get; set; }
}

/// <summary>
/// Historical HOS log entry from an ELD provider
/// </summary>
public record EldHosLogEntryDto
{
    public string? ExternalLogId { get; set; }
    public required string ExternalDriverId { get; set; }
    public required DateTime LogDate { get; set; }
    public required DutyStatus DutyStatus { get; set; }
    public required DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public string? Location { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Remark { get; set; }
}

/// <summary>
/// HOS violation from an ELD provider
/// </summary>
public record EldViolationDataDto
{
    public string? ExternalViolationId { get; set; }
    public required string ExternalDriverId { get; set; }
    public required DateTime ViolationDate { get; set; }
    public required HosViolationType ViolationType { get; set; }
    public required string Description { get; set; }
    public int SeverityLevel { get; set; }
}

/// <summary>
/// Driver info from an ELD provider for mapping purposes
/// </summary>
public record EldDriverDto
{
    public required string ExternalDriverId { get; set; }
    public required string Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? LicenseNumber { get; set; }
}

/// <summary>
/// Vehicle info from an ELD provider for mapping purposes
/// </summary>
public record EldVehicleDto
{
    public required string ExternalVehicleId { get; set; }
    public required string Name { get; set; }
    public string? Vin { get; set; }
    public string? LicensePlate { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
}

/// <summary>
/// Result of processing a webhook from an ELD provider
/// </summary>
public record EldWebhookResultDto
{
    public required EldWebhookEventType EventType { get; set; }
    public string? ExternalDriverId { get; set; }
    public string? ExternalVehicleId { get; set; }
    public object? Data { get; set; }
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum EldWebhookEventType
{
    Unknown,
    DutyStatusChanged,
    ViolationCreated,
    ViolationResolved,
    DriverCreated,
    DriverUpdated,
    VehicleCreated,
    VehicleUpdated
}

/// <summary>
/// Result of an OAuth token refresh
/// </summary>
public record OAuthTokenResultDto
{
    public required string AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
}
