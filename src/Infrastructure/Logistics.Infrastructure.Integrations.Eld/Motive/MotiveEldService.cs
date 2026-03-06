using System.Net.Http.Json;
using System.Text.Json;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Integrations.Eld.Motive;

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
    private readonly string baseUrl = options.Value.Motive?.BaseUrl ?? "https://api.keeptruckin.com/v1";

    public EldProviderType ProviderType => EldProviderType.Motive;

    public void Initialize(EldProviderConfiguration configuration)
    {
        httpClient.DefaultRequestHeaders.Add("X-Api-Key", configuration.ApiKey);
    }

    public async Task<bool> ValidateCredentialsAsync(string apiKey, string? apiSecret)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/users");
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
                $"{baseUrl}/hours_of_service?driver_ids={externalDriverId}");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to get HOS for driver {DriverId}: {StatusCode}",
                    externalDriverId, response.StatusCode);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<MotiveHosResponse>();
            var driverHos = result?.HoursOfService?.FirstOrDefault();
            return driverHos != null ? MotiveMapper.MapToDto(driverHos) : null;
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
            var response = await httpClient.GetAsync($"{baseUrl}/hours_of_service");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to get all drivers HOS: {StatusCode}", response.StatusCode);
                return [];
            }

            var result = await response.Content.ReadFromJsonAsync<MotiveHosResponse>();
            return result?.HoursOfService?.Select(MotiveMapper.MapToDto) ?? [];
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
                $"{baseUrl}/driver_logs?driver_ids={externalDriverId}&start_date={startStr}&end_date={endStr}");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to get HOS logs for driver {DriverId}: {StatusCode}",
                    externalDriverId, response.StatusCode);
                return [];
            }

            var result = await response.Content.ReadFromJsonAsync<MotiveDriverLogsResponse>();
            return result?.DriverLogs?.SelectMany(dl =>
                dl.Events?.Select(e => MotiveMapper.MapToLogDto(externalDriverId, dl.LogDate, e)) ?? []) ?? [];
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
                $"{baseUrl}/hos_violations?driver_ids={externalDriverId}&start_date={startStr}&end_date={endStr}");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to get violations for driver {DriverId}: {StatusCode}",
                    externalDriverId, response.StatusCode);
                return [];
            }

            var result = await response.Content.ReadFromJsonAsync<MotiveViolationsResponse>();
            return result?.HosViolations?.Select(v => MotiveMapper.MapToViolationDto(externalDriverId, v)) ?? [];
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
            var response = await httpClient.GetAsync($"{baseUrl}/users?role=driver");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to get drivers: {StatusCode}", response.StatusCode);
                return [];
            }

            var result = await response.Content.ReadFromJsonAsync<MotiveUsersResponse>();
            return result?.Users?.Select(MotiveMapper.MapToDriverDto) ?? [];
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
            var response = await httpClient.GetAsync($"{baseUrl}/vehicles");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to get vehicles: {StatusCode}", response.StatusCode);
                return [];
            }

            var result = await response.Content.ReadFromJsonAsync<MotiveVehiclesResponse>();
            return result?.Vehicles?.Select(MotiveMapper.MapToVehicleDto) ?? [];
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
}
