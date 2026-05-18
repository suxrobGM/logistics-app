using System.Text.Json;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.Eld.Common;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Logistics.Application.Abstractions.Eld;

namespace Logistics.Infrastructure.Integrations.Eld.Providers.Motive;

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
        catch (HttpRequestException ex)
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
        var result = await httpClient.TryGetFromJsonAsync<MotiveHosResponse>(
            $"{baseUrl}/hours_of_service?driver_ids={externalDriverId}",
            EldJsonOptions.SnakeCase,
            logger,
            $"Motive HOS for driver {externalDriverId}");
        var driverHos = result?.HoursOfService?.FirstOrDefault();
        return driverHos is null ? null : MotiveMapper.MapToDto(driverHos);
    }

    public async Task<IEnumerable<EldDriverHosDataDto>> GetAllDriversHosStatusAsync()
    {
        var result = await httpClient.TryGetFromJsonAsync<MotiveHosResponse>(
            $"{baseUrl}/hours_of_service",
            EldJsonOptions.SnakeCase,
            logger,
            "Motive HOS (all drivers)");
        return result?.HoursOfService?.Select(MotiveMapper.MapToDto) ?? [];
    }

    public async Task<IEnumerable<EldHosLogEntryDto>> GetDriverHosLogsAsync(
        string externalDriverId,
        DateTime startDate,
        DateTime endDate)
    {
        var startStr = startDate.ToString("yyyy-MM-dd");
        var endStr = endDate.ToString("yyyy-MM-dd");

        var result = await httpClient.TryGetFromJsonAsync<MotiveDriverLogsResponse>(
            $"{baseUrl}/driver_logs?driver_ids={externalDriverId}&start_date={startStr}&end_date={endStr}",
            EldJsonOptions.SnakeCase,
            logger,
            $"Motive HOS logs for driver {externalDriverId}");
        return result?.DriverLogs?.SelectMany(dl =>
            dl.Events?.Select(e => MotiveMapper.MapToLogDto(externalDriverId, dl.LogDate, e)) ?? []) ?? [];
    }

    public async Task<IEnumerable<EldViolationDataDto>> GetDriverViolationsAsync(
        string externalDriverId,
        DateTime startDate,
        DateTime endDate)
    {
        var startStr = startDate.ToString("yyyy-MM-dd");
        var endStr = endDate.ToString("yyyy-MM-dd");

        var result = await httpClient.TryGetFromJsonAsync<MotiveViolationsResponse>(
            $"{baseUrl}/hos_violations?driver_ids={externalDriverId}&start_date={startStr}&end_date={endStr}",
            EldJsonOptions.SnakeCase,
            logger,
            $"Motive violations for driver {externalDriverId}");
        return result?.HosViolations?.Select(v => MotiveMapper.MapToViolationDto(externalDriverId, v)) ?? [];
    }

    public async Task<IEnumerable<EldDriverDto>> GetAllDriversAsync()
    {
        var result = await httpClient.TryGetFromJsonAsync<MotiveUsersResponse>(
            $"{baseUrl}/users?role=driver",
            EldJsonOptions.SnakeCase,
            logger,
            "Motive users (drivers)");
        return result?.Users?.Select(MotiveMapper.MapToDriverDto) ?? [];
    }

    public async Task<IEnumerable<EldVehicleDto>> GetAllVehiclesAsync()
    {
        var result = await httpClient.TryGetFromJsonAsync<MotiveVehiclesResponse>(
            $"{baseUrl}/vehicles",
            EldJsonOptions.SnakeCase,
            logger,
            "Motive vehicles");
        return result?.Vehicles?.Select(MotiveMapper.MapToVehicleDto) ?? [];
    }

    public Task<EldWebhookResultDto> ProcessWebhookAsync(string payload, string? signature, string? webhookSecret)
    {
        try
        {
            var webhook = JsonSerializer.Deserialize<MotiveWebhookPayload>(payload, EldJsonOptions.SnakeCase);
            if (webhook is null)
            {
                return Task.FromResult(InvalidWebhook("Failed to parse webhook payload"));
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
        catch (JsonException ex)
        {
            logger.LogError(ex, "Error processing Motive webhook");
            return Task.FromResult(InvalidWebhook(ex.Message));
        }
    }

    private static EldWebhookResultDto InvalidWebhook(string error) => new()
    {
        EventType = EldWebhookEventType.Unknown,
        IsValid = false,
        ErrorMessage = error
    };
}
