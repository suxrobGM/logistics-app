using System.Text.Json;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.Common;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Eld;

namespace Logistics.Infrastructure.Integrations.Eld.Providers.Motive;

/// <summary>
///     Motive (KeepTruckin) ELD provider implementation.
///     API Documentation: https://developer.gomotive.com/docs
/// </summary>
internal class MotiveEldService(
    HttpClient httpClient,
    ILogger<MotiveEldService> logger)
    : IEldProviderService
{
    public EldProviderType ProviderType => EldProviderType.Motive;

    public void Initialize(EldProviderConfiguration configuration)
    {
        httpClient.DefaultRequestHeaders.Remove("X-Api-Key");
        httpClient.DefaultRequestHeaders.Add("X-Api-Key", configuration.ApiKey);
    }

    public async Task<bool> ValidateCredentialsAsync(string apiKey, string? apiSecret)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "users");
        request.Headers.Add("X-Api-Key", apiKey);
        return await httpClient.ValidateCredentialsAsync(request, logger, "Motive");
    }

    public async Task<EldDriverHosDataDto?> GetDriverHosStatusAsync(string externalDriverId)
    {
        var result = await httpClient.TryGetFromJsonAsync<MotiveHosResponse>(
            $"hours_of_service?driver_ids={externalDriverId}",
            logger,
            $"Motive HOS for driver {externalDriverId}",
            IntegrationJsonOptions.SnakeCase);
        var driverHos = result?.HoursOfService?.FirstOrDefault();
        return driverHos is null ? null : MotiveMapper.MapToDto(driverHos);
    }

    public async Task<IEnumerable<EldDriverHosDataDto>> GetAllDriversHosStatusAsync()
    {
        var result = await httpClient.TryGetFromJsonAsync<MotiveHosResponse>(
            "hours_of_service",
            logger,
            "Motive HOS (all drivers)",
            IntegrationJsonOptions.SnakeCase);
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
            $"driver_logs?driver_ids={externalDriverId}&start_date={startStr}&end_date={endStr}",
            logger,
            $"Motive HOS logs for driver {externalDriverId}",
            IntegrationJsonOptions.SnakeCase);
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
            $"hos_violations?driver_ids={externalDriverId}&start_date={startStr}&end_date={endStr}",
            logger,
            $"Motive violations for driver {externalDriverId}",
            IntegrationJsonOptions.SnakeCase);
        return result?.HosViolations?.Select(v => MotiveMapper.MapToViolationDto(externalDriverId, v)) ?? [];
    }

    public async Task<IEnumerable<EldDriverDto>> GetAllDriversAsync()
    {
        var result = await httpClient.TryGetFromJsonAsync<MotiveUsersResponse>(
            "users?role=driver",
            logger,
            "Motive users (drivers)",
            IntegrationJsonOptions.SnakeCase);
        return result?.Users?.Select(MotiveMapper.MapToDriverDto) ?? [];
    }

    public async Task<IEnumerable<EldVehicleDto>> GetAllVehiclesAsync()
    {
        var result = await httpClient.TryGetFromJsonAsync<MotiveVehiclesResponse>(
            "vehicles",
            logger,
            "Motive vehicles",
            IntegrationJsonOptions.SnakeCase);
        return result?.Vehicles?.Select(MotiveMapper.MapToVehicleDto) ?? [];
    }

    public Task<EldWebhookResultDto> ProcessWebhookAsync(EldWebhookRequestDto request, string? webhookSecret)
    {
        if (!WebhookSignature.VerifyHmacSha256(request.Body, request.Signature, webhookSecret))
        {
            logger.LogWarning("Rejected Motive webhook with invalid or unverifiable signature");
            return Task.FromResult(EldWebhookResultDto.Invalid("Invalid webhook signature"));
        }

        try
        {
            var webhook = JsonSerializer.Deserialize<MotiveWebhookPayload>(request.Body, IntegrationJsonOptions.SnakeCase);
            if (webhook is null)
            {
                return Task.FromResult(EldWebhookResultDto.Invalid("Failed to parse webhook payload"));
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
            return Task.FromResult(EldWebhookResultDto.Invalid(ex.Message));
        }
    }
}
