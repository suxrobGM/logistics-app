using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Services;

/// <summary>
///     Samsara ELD provider implementation.
///     API Documentation: https://developers.samsara.com/docs
/// </summary>
internal class SamsaraEldService(
    HttpClient httpClient,
    IOptions<EldOptions> options,
    ILogger<SamsaraEldService> logger)
    : IEldProviderService
{
    private readonly string _baseUrl = options.Value.Samsara?.BaseUrl ?? "https://api.samsara.com";
    private string? _apiToken;

    public EldProviderType ProviderType => EldProviderType.Samsara;

    public void Initialize(EldProviderConfiguration configuration)
    {
        _apiToken = configuration.AccessToken ?? configuration.ApiKey;
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiToken);
    }

    public async Task<bool> ValidateCredentialsAsync(string apiKey, string? apiSecret)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/fleet/drivers");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to validate Samsara credentials");
            return false;
        }
    }

    public Task<OAuthTokenResultDto?> RefreshTokenAsync(string refreshToken)
    {
        // Samsara uses API tokens, not OAuth refresh tokens
        return Task.FromResult<OAuthTokenResultDto?>(null);
    }

    public async Task<EldDriverHosDataDto?> GetDriverHosStatusAsync(string externalDriverId)
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"{_baseUrl}/fleet/hos/drivers/{externalDriverId}/clocks");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to get HOS for driver {DriverId}: {StatusCode}",
                    externalDriverId, response.StatusCode);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<SamsaraHosClockResponse>();
            return result?.Data != null ? MapToDto(externalDriverId, result.Data) : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching HOS for driver {DriverId}", externalDriverId);
            return null;
        }
    }

    public async Task<IEnumerable<EldDriverHosDataDto>> GetAllDriversHosStatusAsync()
    {
        try
        {
            var response = await httpClient.GetAsync($"{_baseUrl}/fleet/hos/clocks");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to get all drivers HOS: {StatusCode}", response.StatusCode);
                return [];
            }

            var result = await response.Content.ReadFromJsonAsync<SamsaraHosClocksResponse>();
            return result?.Data?.Select(d => MapToDto(d.Driver?.Id ?? "", d)) ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching all drivers HOS");
            return [];
        }
    }

    public async Task<IEnumerable<EldHosLogEntryDto>> GetDriverHosLogsAsync(
        string externalDriverId,
        DateTime startDate,
        DateTime endDate)
    {
        try
        {
            var startMs = new DateTimeOffset(startDate).ToUnixTimeMilliseconds();
            var endMs = new DateTimeOffset(endDate).ToUnixTimeMilliseconds();

            var response = await httpClient.GetAsync(
                $"{_baseUrl}/fleet/hos/logs?driverIds={externalDriverId}&startTime={startMs}&endTime={endMs}");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to get HOS logs for driver {DriverId}: {StatusCode}",
                    externalDriverId, response.StatusCode);
                return [];
            }

            var result = await response.Content.ReadFromJsonAsync<SamsaraHosLogsResponse>();
            return result?.Data?.Select(MapToLogDto) ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching HOS logs for driver {DriverId}", externalDriverId);
            return [];
        }
    }

    public async Task<IEnumerable<EldViolationDataDto>> GetDriverViolationsAsync(
        string externalDriverId,
        DateTime startDate,
        DateTime endDate)
    {
        try
        {
            var startMs = new DateTimeOffset(startDate).ToUnixTimeMilliseconds();
            var endMs = new DateTimeOffset(endDate).ToUnixTimeMilliseconds();

            var response = await httpClient.GetAsync(
                $"{_baseUrl}/fleet/hos/violations?driverIds={externalDriverId}&startTime={startMs}&endTime={endMs}");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to get violations for driver {DriverId}: {StatusCode}",
                    externalDriverId, response.StatusCode);
                return [];
            }

            var result = await response.Content.ReadFromJsonAsync<SamsaraViolationsResponse>();
            return result?.Data?.Select(v => MapToViolationDto(externalDriverId, v)) ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching violations for driver {DriverId}", externalDriverId);
            return [];
        }
    }

    public async Task<IEnumerable<EldDriverDto>> GetAllDriversAsync()
    {
        try
        {
            var response = await httpClient.GetAsync($"{_baseUrl}/fleet/drivers");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to get drivers: {StatusCode}", response.StatusCode);
                return [];
            }

            var result = await response.Content.ReadFromJsonAsync<SamsaraDriversResponse>();
            return result?.Data?.Select(MapToDriverDto) ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching drivers");
            return [];
        }
    }

    public async Task<IEnumerable<EldVehicleDto>> GetAllVehiclesAsync()
    {
        try
        {
            var response = await httpClient.GetAsync($"{_baseUrl}/fleet/vehicles");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to get vehicles: {StatusCode}", response.StatusCode);
                return [];
            }

            var result = await response.Content.ReadFromJsonAsync<SamsaraVehiclesResponse>();
            return result?.Data?.Select(MapToVehicleDto) ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching vehicles");
            return [];
        }
    }

    public Task<EldWebhookResultDto> ProcessWebhookAsync(string payload, string? signature, string? webhookSecret)
    {
        try
        {
            var webhook = JsonSerializer.Deserialize<SamsaraWebhookPayload>(payload);
            if (webhook == null)
            {
                return Task.FromResult(new EldWebhookResultDto
                {
                    EventType = EldWebhookEventType.Unknown,
                    IsValid = false,
                    ErrorMessage = "Failed to parse webhook payload"
                });
            }

            var eventType = webhook.EventType switch
            {
                "HosClocksUpdated" => EldWebhookEventType.DutyStatusChanged,
                "HosViolationCreated" => EldWebhookEventType.ViolationCreated,
                "HosViolationResolved" => EldWebhookEventType.ViolationResolved,
                "DriverCreated" => EldWebhookEventType.DriverCreated,
                "DriverUpdated" => EldWebhookEventType.DriverUpdated,
                "VehicleCreated" => EldWebhookEventType.VehicleCreated,
                "VehicleUpdated" => EldWebhookEventType.VehicleUpdated,
                _ => EldWebhookEventType.Unknown
            };

            return Task.FromResult(new EldWebhookResultDto
            {
                EventType = eventType,
                ExternalDriverId = webhook.Data?.DriverId,
                ExternalVehicleId = webhook.Data?.VehicleId,
                Data = webhook.Data,
                IsValid = true
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing Samsara webhook");
            return Task.FromResult(new EldWebhookResultDto
            {
                EventType = EldWebhookEventType.Unknown,
                IsValid = false,
                ErrorMessage = ex.Message
            });
        }
    }

    #region Mapping Methods

    private static EldDriverHosDataDto MapToDto(string driverId, SamsaraHosClockData data) => new()
    {
        ExternalDriverId = driverId,
        ExternalDriverName = data.Driver?.Name,
        CurrentDutyStatus = MapDutyStatus(data.CurrentDutyStatus),
        StatusChangedAt = DateTimeOffset.FromUnixTimeMilliseconds(data.CurrentDutyStatusStartMs ?? 0).UtcDateTime,
        DrivingMinutesRemaining = (int)(data.DrivingTimeRemainingMs ?? 0) / 60000,
        OnDutyMinutesRemaining = (int)(data.ShiftTimeRemainingMs ?? 0) / 60000,
        CycleMinutesRemaining = (int)(data.CycleTimeRemainingMs ?? 0) / 60000,
        TimeUntilBreakRequired = data.BreakTimeRemainingMs.HasValue
            ? TimeSpan.FromMilliseconds(data.BreakTimeRemainingMs.Value)
            : null,
        IsInViolation = data.CurrentViolations?.Count > 0
    };

    private static EldHosLogEntryDto MapToLogDto(SamsaraHosLogData data) => new()
    {
        ExternalLogId = data.Id,
        ExternalDriverId = data.Driver?.Id ?? "",
        LogDate = DateTimeOffset.FromUnixTimeMilliseconds(data.StartMs ?? 0).UtcDateTime.Date,
        DutyStatus = MapDutyStatus(data.DutyStatus),
        StartTime = DateTimeOffset.FromUnixTimeMilliseconds(data.StartMs ?? 0).UtcDateTime,
        EndTime = data.EndMs.HasValue
            ? DateTimeOffset.FromUnixTimeMilliseconds(data.EndMs.Value).UtcDateTime
            : null,
        DurationMinutes = (int)((data.EndMs ?? data.StartMs ?? 0) - (data.StartMs ?? 0)) / 60000,
        Location = data.Location?.Name,
        Latitude = data.Location?.Latitude,
        Longitude = data.Location?.Longitude,
        Remark = data.Remark
    };

    private static EldViolationDataDto MapToViolationDto(string driverId, SamsaraViolationData data) => new()
    {
        ExternalViolationId = data.Id,
        ExternalDriverId = driverId,
        ViolationDate = DateTimeOffset.FromUnixTimeMilliseconds(data.StartMs ?? 0).UtcDateTime,
        ViolationType = MapViolationType(data.ViolationType),
        Description = data.ViolationType ?? "Unknown violation",
        SeverityLevel = 3
    };

    private static EldDriverDto MapToDriverDto(SamsaraDriverData data) => new()
    {
        ExternalDriverId = data.Id ?? "",
        Name = data.Name ?? "",
        Email = data.Email,
        Phone = data.Phone,
        LicenseNumber = data.LicenseNumber
    };

    private static EldVehicleDto MapToVehicleDto(SamsaraVehicleData data) => new()
    {
        ExternalVehicleId = data.Id ?? "",
        Name = data.Name ?? "",
        Vin = data.Vin,
        LicensePlate = data.LicensePlate,
        Make = data.Make,
        Model = data.Model,
        Year = data.Year
    };

    private static DutyStatus MapDutyStatus(string? status) => status?.ToLowerInvariant() switch
    {
        "off_duty" or "off" => DutyStatus.OffDuty,
        "sleeper_berth" or "sleeper" => DutyStatus.SleeperBerth,
        "driving" => DutyStatus.Driving,
        "on_duty" or "on_duty_not_driving" => DutyStatus.OnDutyNotDriving,
        "yard_move" => DutyStatus.YardMove,
        "personal_conveyance" => DutyStatus.PersonalConveyance,
        _ => DutyStatus.OffDuty
    };

    private static HosViolationType MapViolationType(string? type) => type?.ToLowerInvariant() switch
    {
        "driving" or "11_hour" => HosViolationType.Driving11Hour,
        "shift" or "14_hour" => HosViolationType.OnDuty14Hour,
        "break" or "30_minute" => HosViolationType.Break30Minute,
        "cycle" or "70_hour" => HosViolationType.Cycle70Hour,
        "restart" => HosViolationType.RestartRequired,
        _ => HosViolationType.FormAndMannerViolation
    };

    #endregion

    #region Samsara API Models

    private record SamsaraHosClockResponse(SamsaraHosClockData? Data);
    private record SamsaraHosClocksResponse(List<SamsaraHosClockData>? Data);
    private record SamsaraHosLogsResponse(List<SamsaraHosLogData>? Data);
    private record SamsaraViolationsResponse(List<SamsaraViolationData>? Data);
    private record SamsaraDriversResponse(List<SamsaraDriverData>? Data);
    private record SamsaraVehiclesResponse(List<SamsaraVehicleData>? Data);

    private record SamsaraHosClockData
    {
        [JsonPropertyName("driver")] public SamsaraDriverRef? Driver { get; init; }
        [JsonPropertyName("currentDutyStatus")] public string? CurrentDutyStatus { get; init; }
        [JsonPropertyName("currentDutyStatusStartMs")] public long? CurrentDutyStatusStartMs { get; init; }
        [JsonPropertyName("drivingTimeRemainingMs")] public long? DrivingTimeRemainingMs { get; init; }
        [JsonPropertyName("shiftTimeRemainingMs")] public long? ShiftTimeRemainingMs { get; init; }
        [JsonPropertyName("cycleTimeRemainingMs")] public long? CycleTimeRemainingMs { get; init; }
        [JsonPropertyName("breakTimeRemainingMs")] public long? BreakTimeRemainingMs { get; init; }
        [JsonPropertyName("currentViolations")] public List<object>? CurrentViolations { get; init; }
    }

    private record SamsaraDriverRef
    {
        [JsonPropertyName("id")] public string? Id { get; init; }
        [JsonPropertyName("name")] public string? Name { get; init; }
    }

    private record SamsaraHosLogData
    {
        [JsonPropertyName("id")] public string? Id { get; init; }
        [JsonPropertyName("driver")] public SamsaraDriverRef? Driver { get; init; }
        [JsonPropertyName("dutyStatus")] public string? DutyStatus { get; init; }
        [JsonPropertyName("startMs")] public long? StartMs { get; init; }
        [JsonPropertyName("endMs")] public long? EndMs { get; init; }
        [JsonPropertyName("location")] public SamsaraLocation? Location { get; init; }
        [JsonPropertyName("remark")] public string? Remark { get; init; }
    }

    private record SamsaraLocation
    {
        [JsonPropertyName("name")] public string? Name { get; init; }
        [JsonPropertyName("latitude")] public double? Latitude { get; init; }
        [JsonPropertyName("longitude")] public double? Longitude { get; init; }
    }

    private record SamsaraViolationData
    {
        [JsonPropertyName("id")] public string? Id { get; init; }
        [JsonPropertyName("violationType")] public string? ViolationType { get; init; }
        [JsonPropertyName("startMs")] public long? StartMs { get; init; }
    }

    private record SamsaraDriverData
    {
        [JsonPropertyName("id")] public string? Id { get; init; }
        [JsonPropertyName("name")] public string? Name { get; init; }
        [JsonPropertyName("email")] public string? Email { get; init; }
        [JsonPropertyName("phone")] public string? Phone { get; init; }
        [JsonPropertyName("licenseNumber")] public string? LicenseNumber { get; init; }
    }

    private record SamsaraVehicleData
    {
        [JsonPropertyName("id")] public string? Id { get; init; }
        [JsonPropertyName("name")] public string? Name { get; init; }
        [JsonPropertyName("vin")] public string? Vin { get; init; }
        [JsonPropertyName("licensePlate")] public string? LicensePlate { get; init; }
        [JsonPropertyName("make")] public string? Make { get; init; }
        [JsonPropertyName("model")] public string? Model { get; init; }
        [JsonPropertyName("year")] public int? Year { get; init; }
    }

    private record SamsaraWebhookPayload
    {
        [JsonPropertyName("eventType")] public string? EventType { get; init; }
        [JsonPropertyName("data")] public SamsaraWebhookData? Data { get; init; }
    }

    private record SamsaraWebhookData
    {
        [JsonPropertyName("driverId")] public string? DriverId { get; init; }
        [JsonPropertyName("vehicleId")] public string? VehicleId { get; init; }
    }

    #endregion
}
