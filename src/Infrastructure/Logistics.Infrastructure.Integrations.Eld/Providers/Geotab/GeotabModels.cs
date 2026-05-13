namespace Logistics.Infrastructure.Integrations.Eld.Providers.Geotab;

/// <summary>
/// MyGeotab API uses a JSON-RPC-style envelope: POST /apiv1 with { method, params }.
/// All wire fields are camelCase — no [JsonPropertyName] needed (see EldJsonOptions.CamelCase).
/// See https://geotab.github.io/sdk/software/api/reference/.
/// </summary>
internal record GeotabRpcRequest(string Method, object Params);

internal record GeotabRpcResponse<T>
{
    public T? Result { get; init; }
    public GeotabRpcError? Error { get; init; }
}

internal record GeotabRpcError
{
    public string? Name { get; init; }
    public string? Message { get; init; }
}

internal record GeotabAuthenticateResult
{
    public GeotabCredentials? Credentials { get; init; }
    public string? Path { get; init; }
}

internal record GeotabCredentials
{
    public string? Database { get; init; }
    public string? UserName { get; init; }
    public string? SessionId { get; init; }
}

internal record GeotabUser
{
    public string? Id { get; init; }
    public string? Name { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? PhoneNumber { get; init; }
    public string? LicenseNumber { get; init; }
}

internal record GeotabDevice
{
    public string? Id { get; init; }
    public string? Name { get; init; }
    [System.Text.Json.Serialization.JsonPropertyName("vehicleIdentificationNumber")]
    public string? Vin { get; init; }
    public string? LicensePlate { get; init; }
}

/// <summary>
/// MyGeotab DutyStatusLog. Mirrors HOS log entries from a tachograph or US ELD device.
/// </summary>
internal record GeotabDutyStatusLog
{
    public string? Id { get; init; }
    public GeotabReference? Driver { get; init; }
    public string? Status { get; init; }
    public DateTime? DateTime { get; init; }
    public string? RuleSet { get; init; }
    public GeotabLocation? Location { get; init; }
    public List<GeotabAnnotation>? Annotations { get; init; }
}

internal record GeotabReference
{
    public string? Id { get; init; }
}

internal record GeotabLocation
{
    [System.Text.Json.Serialization.JsonPropertyName("x")]
    public double? Longitude { get; init; }
    [System.Text.Json.Serialization.JsonPropertyName("y")]
    public double? Latitude { get; init; }
    public string? Address { get; init; }
}

internal record GeotabAnnotation
{
    public string? Comment { get; init; }
}

internal record GeotabDutyStatusViolation
{
    public string? Id { get; init; }
    public GeotabReference? Driver { get; init; }
    public string? ViolationType { get; init; }
    public string? RuleSet { get; init; }
    public DateTime? DateTime { get; init; }
    public string? Description { get; init; }
}

internal record GeotabDutyStatusAvailability
{
    public GeotabReference? Driver { get; init; }
    public TimeSpan? Driving { get; init; }
    public TimeSpan? Duty { get; init; }
    public TimeSpan? Cycle { get; init; }
    public TimeSpan? Rest { get; init; }
    public bool? IsInViolation { get; init; }
    public List<TimeSpan>? Recap { get; init; }
    public string? CurrentDutyStatus { get; init; }
    public DateTime? CurrentDutyStatusStartDateTime { get; init; }
}

/// <summary>
/// Webhook payload shape. Geotab does not natively push HOS events; integrators typically
/// use Geotab Add-Ins or the IOX feed. We accept a thin payload mirroring the public
/// "duty status" events for parity with other providers.
/// </summary>
internal record GeotabWebhookPayload
{
    public string? EventType { get; init; }
    public string? DriverId { get; init; }
    public string? VehicleId { get; init; }
    public object? Data { get; init; }
}
