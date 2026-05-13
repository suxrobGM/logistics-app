using System.Net.Http.Headers;
using System.Text.Json;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.Eld.Common;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Logistics.Application.Abstractions.Eld;

namespace Logistics.Infrastructure.Integrations.Eld.Providers.Samsara;

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
        catch (HttpRequestException ex)
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
        var result = await httpClient.TryGetFromJsonAsync<SamsaraHosClockResponse>(
            $"{baseUrl}/fleet/hos/drivers/{externalDriverId}/clocks",
            EldJsonOptions.CamelCase,
            logger,
            $"Samsara HOS clocks for driver {externalDriverId}");
        return result?.Data is null ? null : SamsaraMapper.MapToDto(externalDriverId, result.Data);
    }

    public async Task<IEnumerable<EldDriverHosDataDto>> GetAllDriversHosStatusAsync()
    {
        var result = await httpClient.TryGetFromJsonAsync<SamsaraHosClocksResponse>(
            $"{baseUrl}/fleet/hos/clocks",
            EldJsonOptions.CamelCase,
            logger,
            "Samsara HOS clocks (all drivers)");
        return result?.Data?.Select(d => SamsaraMapper.MapToDto(d.Driver?.Id ?? "", d)) ?? [];
    }

    public async Task<IEnumerable<EldHosLogEntryDto>> GetDriverHosLogsAsync(
        string externalDriverId,
        DateTime startDate,
        DateTime endDate)
    {
        var startMs = new DateTimeOffset(startDate).ToUnixTimeMilliseconds();
        var endMs = new DateTimeOffset(endDate).ToUnixTimeMilliseconds();

        var result = await httpClient.TryGetFromJsonAsync<SamsaraHosLogsResponse>(
            $"{baseUrl}/fleet/hos/logs?driverIds={externalDriverId}&startTime={startMs}&endTime={endMs}",
            EldJsonOptions.CamelCase,
            logger,
            $"Samsara HOS logs for driver {externalDriverId}");
        return result?.Data?.Select(SamsaraMapper.MapToLogDto) ?? [];
    }

    public async Task<IEnumerable<EldViolationDataDto>> GetDriverViolationsAsync(
        string externalDriverId,
        DateTime startDate,
        DateTime endDate)
    {
        var startMs = new DateTimeOffset(startDate).ToUnixTimeMilliseconds();
        var endMs = new DateTimeOffset(endDate).ToUnixTimeMilliseconds();

        var result = await httpClient.TryGetFromJsonAsync<SamsaraViolationsResponse>(
            $"{baseUrl}/fleet/hos/violations?driverIds={externalDriverId}&startTime={startMs}&endTime={endMs}",
            EldJsonOptions.CamelCase,
            logger,
            $"Samsara violations for driver {externalDriverId}");
        return result?.Data?.Select(v => SamsaraMapper.MapToViolationDto(externalDriverId, v)) ?? [];
    }

    public async Task<IEnumerable<EldDriverDto>> GetAllDriversAsync()
    {
        var result = await httpClient.TryGetFromJsonAsync<SamsaraDriversResponse>(
            $"{baseUrl}/fleet/drivers",
            EldJsonOptions.CamelCase,
            logger,
            "Samsara drivers list");
        return result?.Data?.Select(SamsaraMapper.MapToDriverDto) ?? [];
    }

    public async Task<IEnumerable<EldVehicleDto>> GetAllVehiclesAsync()
    {
        var result = await httpClient.TryGetFromJsonAsync<SamsaraVehiclesResponse>(
            $"{baseUrl}/fleet/vehicles",
            EldJsonOptions.CamelCase,
            logger,
            "Samsara vehicles list");
        return result?.Data?.Select(SamsaraMapper.MapToVehicleDto) ?? [];
    }

    public Task<EldWebhookResultDto> ProcessWebhookAsync(string payload, string? signature, string? webhookSecret)
    {
        try
        {
            var webhook = JsonSerializer.Deserialize<SamsaraWebhookPayload>(payload, EldJsonOptions.CamelCase);
            if (webhook is null)
            {
                return Task.FromResult(InvalidWebhook("Failed to parse webhook payload"));
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
        catch (JsonException ex)
        {
            logger.LogError(ex, "Error processing Samsara webhook");
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
