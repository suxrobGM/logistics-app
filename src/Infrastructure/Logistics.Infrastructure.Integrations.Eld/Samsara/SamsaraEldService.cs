using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Integrations.Eld.Samsara;

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
    private readonly string baseUrl = options.Value.Samsara?.BaseUrl ?? "https://api.samsara.com";

    public EldProviderType ProviderType => EldProviderType.Samsara;

    public void Initialize(EldProviderConfiguration configuration)
    {
        var apiToken = configuration.AccessToken ?? configuration.ApiKey;
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiToken);
    }

    public async Task<bool> ValidateCredentialsAsync(string apiKey, string? apiSecret)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/fleet/drivers");
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
                $"{baseUrl}/fleet/hos/drivers/{externalDriverId}/clocks");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to get HOS for driver {DriverId}: {StatusCode}",
                    externalDriverId, response.StatusCode);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<SamsaraHosClockResponse>();
            return result?.Data != null ? SamsaraMapper.MapToDto(externalDriverId, result.Data) : null;
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
            var response = await httpClient.GetAsync($"{baseUrl}/fleet/hos/clocks");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to get all drivers HOS: {StatusCode}", response.StatusCode);
                return [];
            }

            var result = await response.Content.ReadFromJsonAsync<SamsaraHosClocksResponse>();
            return result?.Data?.Select(d => SamsaraMapper.MapToDto(d.Driver?.Id ?? "", d)) ?? [];
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
                $"{baseUrl}/fleet/hos/logs?driverIds={externalDriverId}&startTime={startMs}&endTime={endMs}");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to get HOS logs for driver {DriverId}: {StatusCode}",
                    externalDriverId, response.StatusCode);
                return [];
            }

            var result = await response.Content.ReadFromJsonAsync<SamsaraHosLogsResponse>();
            return result?.Data?.Select(SamsaraMapper.MapToLogDto) ?? [];
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
                $"{baseUrl}/fleet/hos/violations?driverIds={externalDriverId}&startTime={startMs}&endTime={endMs}");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to get violations for driver {DriverId}: {StatusCode}",
                    externalDriverId, response.StatusCode);
                return [];
            }

            var result = await response.Content.ReadFromJsonAsync<SamsaraViolationsResponse>();
            return result?.Data?.Select(v => SamsaraMapper.MapToViolationDto(externalDriverId, v)) ?? [];
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
            var response = await httpClient.GetAsync($"{baseUrl}/fleet/drivers");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to get drivers: {StatusCode}", response.StatusCode);
                return [];
            }

            var result = await response.Content.ReadFromJsonAsync<SamsaraDriversResponse>();
            return result?.Data?.Select(SamsaraMapper.MapToDriverDto) ?? [];
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
            var response = await httpClient.GetAsync($"{baseUrl}/fleet/vehicles");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to get vehicles: {StatusCode}", response.StatusCode);
                return [];
            }

            var result = await response.Content.ReadFromJsonAsync<SamsaraVehiclesResponse>();
            return result?.Data?.Select(SamsaraMapper.MapToVehicleDto) ?? [];
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
}
