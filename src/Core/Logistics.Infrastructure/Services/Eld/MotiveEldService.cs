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
///     Motive (KeepTruckin) ELD provider implementation.
///     API Documentation: https://developer.gomotive.com/docs
/// </summary>
internal class MotiveEldService(
    HttpClient httpClient,
    IOptions<EldOptions> options,
    ILogger<MotiveEldService> logger)
    : IEldProviderService
{
    private readonly string _baseUrl = options.Value.Motive?.BaseUrl ?? "https://api.keeptruckin.com/v1";
    private string? _apiKey;

    public EldProviderType ProviderType => EldProviderType.Motive;

    public void Initialize(EldProviderConfiguration configuration)
    {
        _apiKey = configuration.ApiKey;
        httpClient.DefaultRequestHeaders.Add("X-Api-Key", _apiKey);
    }

    public async Task<bool> ValidateCredentialsAsync(string apiKey, string? apiSecret)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/users");
            request.Headers.Add("X-Api-Key", apiKey);

            var response = await httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to validate Motive credentials");
            return false;
        }
    }

    public Task<OAuthTokenResultDto?> RefreshTokenAsync(string refreshToken)
    {
        // Motive uses API keys, not OAuth refresh tokens
        return Task.FromResult<OAuthTokenResultDto?>(null);
    }

    public async Task<EldDriverHosDataDto?> GetDriverHosStatusAsync(string externalDriverId)
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"{_baseUrl}/hours_of_service?driver_ids={externalDriverId}");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to get HOS for driver {DriverId}: {StatusCode}",
                    externalDriverId, response.StatusCode);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<MotiveHosResponse>();
            var driverHos = result?.HoursOfService?.FirstOrDefault();
            return driverHos != null ? MapToDto(driverHos) : null;
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
            var response = await httpClient.GetAsync($"{_baseUrl}/hours_of_service");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to get all drivers HOS: {StatusCode}", response.StatusCode);
                return [];
            }

            var result = await response.Content.ReadFromJsonAsync<MotiveHosResponse>();
            return result?.HoursOfService?.Select(MapToDto) ?? [];
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
            var startStr = startDate.ToString("yyyy-MM-dd");
            var endStr = endDate.ToString("yyyy-MM-dd");

            var response = await httpClient.GetAsync(
                $"{_baseUrl}/driver_logs?driver_ids={externalDriverId}&start_date={startStr}&end_date={endStr}");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to get HOS logs for driver {DriverId}: {StatusCode}",
                    externalDriverId, response.StatusCode);
                return [];
            }

            var result = await response.Content.ReadFromJsonAsync<MotiveDriverLogsResponse>();
            return result?.DriverLogs?.SelectMany(dl =>
                dl.Events?.Select(e => MapToLogDto(externalDriverId, dl.LogDate, e)) ?? []) ?? [];
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
            var startStr = startDate.ToString("yyyy-MM-dd");
            var endStr = endDate.ToString("yyyy-MM-dd");

            var response = await httpClient.GetAsync(
                $"{_baseUrl}/hos_violations?driver_ids={externalDriverId}&start_date={startStr}&end_date={endStr}");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to get violations for driver {DriverId}: {StatusCode}",
                    externalDriverId, response.StatusCode);
                return [];
            }

            var result = await response.Content.ReadFromJsonAsync<MotiveViolationsResponse>();
            return result?.HosViolations?.Select(v => MapToViolationDto(externalDriverId, v)) ?? [];
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
            var response = await httpClient.GetAsync($"{_baseUrl}/users?role=driver");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to get drivers: {StatusCode}", response.StatusCode);
                return [];
            }

            var result = await response.Content.ReadFromJsonAsync<MotiveUsersResponse>();
            return result?.Users?.Select(MapToDriverDto) ?? [];
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
            var response = await httpClient.GetAsync($"{_baseUrl}/vehicles");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to get vehicles: {StatusCode}", response.StatusCode);
                return [];
            }

            var result = await response.Content.ReadFromJsonAsync<MotiveVehiclesResponse>();
            return result?.Vehicles?.Select(MapToVehicleDto) ?? [];
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
            var webhook = JsonSerializer.Deserialize<MotiveWebhookPayload>(payload);
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
                "hos_status_change" => EldWebhookEventType.DutyStatusChanged,
                "hos_violation" => EldWebhookEventType.ViolationCreated,
                "user_created" => EldWebhookEventType.DriverCreated,
                "user_updated" => EldWebhookEventType.DriverUpdated,
                "vehicle_created" => EldWebhookEventType.VehicleCreated,
                "vehicle_updated" => EldWebhookEventType.VehicleUpdated,
                _ => EldWebhookEventType.Unknown
            };

            return Task.FromResult(new EldWebhookResultDto
            {
                EventType = eventType,
                ExternalDriverId = webhook.ObjectId?.ToString(),
                Data = webhook,
                IsValid = true
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing Motive webhook");
            return Task.FromResult(new EldWebhookResultDto
            {
                EventType = EldWebhookEventType.Unknown,
                IsValid = false,
                ErrorMessage = ex.Message
            });
        }
    }

    #region Mapping Methods

    private static EldDriverHosDataDto MapToDto(MotiveHosData data) => new()
    {
        ExternalDriverId = data.Driver?.Id?.ToString() ?? "",
        ExternalDriverName = $"{data.Driver?.FirstName} {data.Driver?.LastName}".Trim(),
        CurrentDutyStatus = MapDutyStatus(data.CurrentDutyStatus),
        StatusChangedAt = data.CurrentDutyStatusStartTime ?? DateTime.UtcNow,
        DrivingMinutesRemaining = data.DrivingTimeRemaining ?? 0,
        OnDutyMinutesRemaining = data.ShiftTimeRemaining ?? 0,
        CycleMinutesRemaining = data.CycleTimeRemaining ?? 0,
        TimeUntilBreakRequired = data.BreakTimeRemaining.HasValue
            ? TimeSpan.FromMinutes(data.BreakTimeRemaining.Value)
            : null,
        IsInViolation = data.ViolationCount > 0
    };

    private static EldHosLogEntryDto MapToLogDto(string driverId, string? logDate, MotiveLogEvent data)
    {
        var date = DateTime.TryParse(logDate, out var d) ? d : DateTime.UtcNow.Date;
        return new EldHosLogEntryDto
        {
            ExternalLogId = data.Id?.ToString(),
            ExternalDriverId = driverId,
            LogDate = date,
            DutyStatus = MapDutyStatus(data.Type),
            StartTime = data.StartTime ?? date,
            EndTime = data.EndTime,
            DurationMinutes = data.Duration ?? 0,
            Location = data.Location,
            Latitude = data.Lat,
            Longitude = data.Lon,
            Remark = data.Annotation
        };
    }

    private static EldViolationDataDto MapToViolationDto(string driverId, MotiveViolationData data) => new()
    {
        ExternalViolationId = data.Id?.ToString(),
        ExternalDriverId = driverId,
        ViolationDate = data.StartTime ?? DateTime.UtcNow,
        ViolationType = MapViolationType(data.Type),
        Description = data.Type ?? "Unknown violation",
        SeverityLevel = 3
    };

    private static EldDriverDto MapToDriverDto(MotiveUserData data) => new()
    {
        ExternalDriverId = data.Id?.ToString() ?? "",
        Name = $"{data.FirstName} {data.LastName}".Trim(),
        Email = data.Email,
        Phone = data.Phone,
        LicenseNumber = data.DriverLicenseNumber
    };

    private static EldVehicleDto MapToVehicleDto(MotiveVehicleData data) => new()
    {
        ExternalVehicleId = data.Id?.ToString() ?? "",
        Name = data.Number ?? "",
        Vin = data.Vin,
        LicensePlate = data.LicensePlateNumber,
        Make = data.Make,
        Model = data.Model,
        Year = data.Year
    };

    private static DutyStatus MapDutyStatus(string? status)
    {
        return status?.ToLowerInvariant() switch
        {
            "off_duty" or "off" => DutyStatus.OffDuty,
            "sleeper" or "sleeper_berth" => DutyStatus.SleeperBerth,
            "driving" or "d" => DutyStatus.Driving,
            "on_duty" or "on" => DutyStatus.OnDutyNotDriving,
            "yard_move" or "ym" => DutyStatus.YardMove,
            "personal_conveyance" or "pc" => DutyStatus.PersonalConveyance,
            _ => DutyStatus.OffDuty
        };
    }

    private static HosViolationType MapViolationType(string? type)
    {
        return type?.ToLowerInvariant() switch
        {
            "driving" or "11_hour_driving" => HosViolationType.Driving11Hour,
            "shift" or "14_hour_shift" => HosViolationType.OnDuty14Hour,
            "break" or "30_minute_break" => HosViolationType.Break30Minute,
            "cycle" or "60_70_hour" => HosViolationType.Cycle70Hour,
            "restart" => HosViolationType.RestartRequired,
            _ => HosViolationType.FormAndMannerViolation
        };
    }

    #endregion

    #region Motive API Models

    private record MotiveHosResponse
    {
        [JsonPropertyName("hours_of_service")]
        public List<MotiveHosData>? HoursOfService { get; init; }
    }

    private record MotiveHosData
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

    private record MotiveDriverRef
    {
        [JsonPropertyName("id")]
        public int? Id { get; init; }

        [JsonPropertyName("first_name")]
        public string? FirstName { get; init; }

        [JsonPropertyName("last_name")]
        public string? LastName { get; init; }
    }

    private record MotiveDriverLogsResponse
    {
        [JsonPropertyName("driver_logs")]
        public List<MotiveDriverLog>? DriverLogs { get; init; }
    }

    private record MotiveDriverLog
    {
        [JsonPropertyName("log_date")]
        public string? LogDate { get; init; }

        [JsonPropertyName("events")]
        public List<MotiveLogEvent>? Events { get; init; }
    }

    private record MotiveLogEvent
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

    private record MotiveViolationsResponse
    {
        [JsonPropertyName("hos_violations")]
        public List<MotiveViolationData>? HosViolations { get; init; }
    }

    private record MotiveViolationData
    {
        [JsonPropertyName("id")]
        public int? Id { get; init; }

        [JsonPropertyName("type")]
        public string? Type { get; init; }

        [JsonPropertyName("start_time")]
        public DateTime? StartTime { get; init; }
    }

    private record MotiveUsersResponse
    {
        [JsonPropertyName("users")]
        public List<MotiveUserData>? Users { get; init; }
    }

    private record MotiveUserData
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

    private record MotiveVehiclesResponse
    {
        [JsonPropertyName("vehicles")]
        public List<MotiveVehicleData>? Vehicles { get; init; }
    }

    private record MotiveVehicleData
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

    private record MotiveWebhookPayload
    {
        [JsonPropertyName("event_type")]
        public string? EventType { get; init; }

        [JsonPropertyName("object_id")]
        public int? ObjectId { get; init; }
    }

    #endregion
}
