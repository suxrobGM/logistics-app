namespace Logistics.Infrastructure.Integrations.Eld.Providers.Samsara;

// Shapes verified against the public reference at developers.samsara.com (Oct 2026):
// listDrivers, listVehicles, getHosClocks, getHosLogs, getHosViolations, getVehicleStats, and the
// Webhooks 1.0 / 2.0 payloads. Wire format is camelCase - see IntegrationJsonOptions.CamelCase.

/// <summary>Every list endpoint: <c>{ data: [...], pagination: { endCursor, hasNextPage } }</c>.</summary>
internal record SamsaraPage<T>(List<T>? Data, SamsaraPagination? Pagination);

internal record SamsaraPagination(string? EndCursor, bool HasNextPage);

internal record SamsaraRef(string? Id, string? Name);

#region Drivers and vehicles

internal record SamsaraDriver
{
    public string? Id { get; init; }
    public string? Name { get; init; }
    public string? Username { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? LicenseNumber { get; init; }
}

internal record SamsaraVehicle
{
    public string? Id { get; init; }
    public string? Name { get; init; }
    public string? Vin { get; init; }
    public string? LicensePlate { get; init; }
    public string? Make { get; init; }
    public string? Model { get; init; }

    /// <summary>Samsara sends the model year as a string, e.g. <c>"2008"</c>.</summary>
    public string? Year { get; init; }
}

#endregion

#region HOS clocks (GET /fleet/hos/clocks)

internal record SamsaraHosClock
{
    public SamsaraRef? Driver { get; init; }
    public SamsaraDutyStatus? CurrentDutyStatus { get; init; }
    public SamsaraClocks? Clocks { get; init; }
    public SamsaraClockViolations? Violations { get; init; }
}

/// <summary>
/// <c>hosStatusType</c>: offDuty, sleeperBed, driving, onDuty, yardMove, personalConveyance - or an
/// empty string while the driver app is disconnected.
/// </summary>
internal record SamsaraDutyStatus(string? HosStatusType);

internal record SamsaraClocks
{
    public SamsaraDriveClock? Drive { get; init; }
    public SamsaraShiftClock? Shift { get; init; }
    public SamsaraCycleClock? Cycle { get; init; }
    public SamsaraBreakClock? Break { get; init; }
}

internal record SamsaraDriveClock(long? DriveRemainingDurationMs);

internal record SamsaraShiftClock(long? ShiftRemainingDurationMs);

internal record SamsaraCycleClock(long? CycleRemainingDurationMs);

internal record SamsaraBreakClock(long? TimeUntilBreakDurationMs);

internal record SamsaraClockViolations(long? ShiftDrivingViolationDurationMs, long? CycleViolationDurationMs);

#endregion

#region HOS logs (GET /fleet/hos/logs)

internal record SamsaraDriverHosLogs(SamsaraRef? Driver, List<SamsaraHosLogEntry>? HosLogs);

internal record SamsaraHosLogEntry
{
    public DateTimeOffset LogStartTime { get; init; }
    public DateTimeOffset? LogEndTime { get; init; }
    public string? HosStatusType { get; init; }
    public SamsaraLatLng? LogRecordedLocation { get; init; }
    public string? Remark { get; init; }
    public SamsaraRef? Vehicle { get; init; }
}

internal record SamsaraLatLng(double Latitude, double Longitude);

#endregion

#region HOS violations (GET /fleet/hos/violations)

internal record SamsaraDriverViolations(List<SamsaraViolation>? Violations);

internal record SamsaraViolation
{
    /// <summary>e.g. <c>shiftHours</c>, <c>dailyDrivingHours</c>, <c>restbreakMissed</c>.</summary>
    public string? Type { get; init; }

    public string? Description { get; init; }
    public DateTimeOffset? ViolationStartTime { get; init; }
    public SamsaraRef? Driver { get; init; }
}

#endregion

#region Vehicle stats snapshot (GET /fleet/vehicles/stats?types=gps,obdOdometerMeters,gpsOdometerMeters)

internal record SamsaraVehicleStats
{
    public string? Id { get; init; }
    public string? Name { get; init; }
    public SamsaraGps? Gps { get; init; }
    public SamsaraStatValue? ObdOdometerMeters { get; init; }
    public SamsaraStatValue? GpsOdometerMeters { get; init; }
}

internal record SamsaraGps
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public DateTimeOffset Time { get; init; }
}

internal record SamsaraStatValue(DateTimeOffset Time, long Value);

#endregion

#region Webhooks

/// <summary>
/// Webhooks 2.0 envelope (<c>eventId</c>, <c>eventTime</c>, <c>eventType</c>, <c>orgId</c>, <c>data</c>). 1.0
/// deliveries (<c>Alert</c>, <c>Ping</c>) share <c>eventId</c> and <c>eventType</c> and carry no <c>data</c>.
/// </summary>
internal record SamsaraWebhookPayload
{
    public string? EventId { get; init; }
    public string? EventType { get; init; }
    public SamsaraWebhookData? Data { get; init; }
}

/// <summary>
/// <c>DriverCreated</c>/<c>DriverUpdated</c> carry <c>driver</c>, <c>VehicleCreated</c>/<c>VehicleUpdated</c>
/// carry <c>vehicle</c>, and an <c>AlertIncident</c> carries <c>conditions</c> plus <c>isResolved</c>.
/// </summary>
internal record SamsaraWebhookData
{
    public SamsaraRef? Driver { get; init; }
    public SamsaraRef? Vehicle { get; init; }
    public List<SamsaraAlertCondition>? Conditions { get; init; }
    public bool? IsResolved { get; init; }
}

internal record SamsaraAlertCondition(SamsaraAlertDetails? Details);

internal record SamsaraAlertDetails(SamsaraHosViolationDetails? HosViolation);

internal record SamsaraHosViolationDetails(SamsaraRef? Driver);

#endregion
