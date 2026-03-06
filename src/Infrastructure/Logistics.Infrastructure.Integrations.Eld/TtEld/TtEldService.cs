using System.Net.Http.Json;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Integrations.Eld.TtEld;

/// <summary>
///     TT ELD provider implementation.
///     API Documentation: https://developer.tteld.com/
///     GPS tracking-focused provider — does not support HOS or violations.
/// </summary>
internal class TtEldService(
    HttpClient httpClient,
    IOptions<EldOptions> options,
    ILogger<TtEldService> logger)
    : IEldProviderService, IEldGpsTrackingProvider
{
    private readonly string baseUrl = options.Value.TtEld?.BaseUrl ?? "https://read.tteld.com";
    private string? usdot;

    public EldProviderType ProviderType => EldProviderType.TtEld;

    public void Initialize(EldProviderConfiguration configuration)
    {
        usdot = configuration.ExternalAccountId
            ?? throw new InvalidOperationException("USDOT number (ExternalAccountId) is required for TT ELD");
        httpClient.DefaultRequestHeaders.Remove("x-api-key");
        httpClient.DefaultRequestHeaders.Remove("provider-token");
        httpClient.DefaultRequestHeaders.Add("x-api-key", configuration.ApiKey);
        httpClient.DefaultRequestHeaders.Add("provider-token", configuration.ApiSecret);
    }

    public async Task<bool> ValidateCredentialsAsync(string apiKey, string? apiSecret)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get,
                $"{baseUrl}/api/externalservice/drivers-list/{usdot}?page=1&perPage=1&is_active=true");
            request.Headers.Add("x-api-key", apiKey);
            request.Headers.Add("provider-token", apiSecret);

            var response = await httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to validate TT ELD credentials");
            return false;
        }
    }

    public Task<OAuthTokenResultDto?> RefreshTokenAsync(string refreshToken)
    {
        // TT ELD uses API keys, not OAuth
        return Task.FromResult<OAuthTokenResultDto?>(null);
    }

    #region HOS Methods (not supported by TT ELD)

    public Task<EldDriverHosDataDto?> GetDriverHosStatusAsync(string externalDriverId)
    {
        // TT ELD does not provide HOS data
        return Task.FromResult<EldDriverHosDataDto?>(null);
    }

    public Task<IEnumerable<EldDriverHosDataDto>> GetAllDriversHosStatusAsync()
    {
        // TT ELD does not provide HOS data
        return Task.FromResult<IEnumerable<EldDriverHosDataDto>>([]);
    }

    public Task<IEnumerable<EldViolationDataDto>> GetDriverViolationsAsync(
        string externalDriverId,
        DateTime startDate,
        DateTime endDate)
    {
        // TT ELD does not provide violation data
        return Task.FromResult<IEnumerable<EldViolationDataDto>>([]);
    }

    #endregion

    public async Task<IEnumerable<EldHosLogEntryDto>> GetDriverHosLogsAsync(
        string externalDriverId,
        DateTime startDate,
        DateTime endDate)
    {
        try
        {
            var fromStr = startDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            var toStr = endDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            var response = await httpClient.GetAsync(
                $"{baseUrl}/api/externalservice/trackings/{usdot}/{externalDriverId}/?from={fromStr}&to={toStr}");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to get tracking data for vehicle {VehicleId}: {StatusCode}",
                    externalDriverId, response.StatusCode);
                return [];
            }

            var result = await response.Content.ReadFromJsonAsync<List<TtEldTrackingPoint>>();
            return result?.Select(p => TtEldMapper.MapToLogDto(p, externalDriverId)) ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching tracking data for vehicle {VehicleId}", externalDriverId);
            return [];
        }
    }

    public async Task<IEnumerable<EldDriverDto>> GetAllDriversAsync()
    {
        try
        {
            var allDrivers = new List<EldDriverDto>();
            var page = 1;
            int totalPages;

            do
            {
                var response = await httpClient.GetAsync(
                    $"{baseUrl}/api/externalservice/drivers-list/{usdot}?page={page}&perPage=100&is_active=true");

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("Failed to get drivers page {Page}: {StatusCode}", page, response.StatusCode);
                    break;
                }

                var result = await response.Content.ReadFromJsonAsync<TtEldDriversResponse>();
                if (result?.Data is not null)
                {
                    allDrivers.AddRange(result.Data.Select(TtEldMapper.MapToDriverDto));
                }

                totalPages = result?.Meta?.TotalPages ?? 1;
                page++;
            } while (page <= totalPages);

            return allDrivers;
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
            var allVehicles = new List<EldVehicleDto>();
            var page = 1;
            int totalPages;

            do
            {
                var response = await httpClient.GetAsync(
                    $"{baseUrl}/api/externalservice/current-units/{usdot}?page={page}&perPage=100&is_active=true");

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("Failed to get units page {Page}: {StatusCode}", page, response.StatusCode);
                    break;
                }

                var result = await response.Content.ReadFromJsonAsync<TtEldUnitsResponse>();
                if (result?.Data is not null)
                {
                    allVehicles.AddRange(result.Data.Select(TtEldMapper.MapToVehicleDto));
                }

                totalPages = result?.Meta?.TotalPages ?? 1;
                page++;
            } while (page <= totalPages);

            return allVehicles;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching vehicles");
            return [];
        }
    }

    public Task<EldWebhookResultDto> ProcessWebhookAsync(string payload, string? signature, string? webhookSecret)
    {
        // TT ELD does not support webhooks
        return Task.FromResult(new EldWebhookResultDto
        {
            EventType = EldWebhookEventType.Unknown,
            IsValid = false,
            ErrorMessage = "TT ELD does not support webhooks"
        });
    }

    #region GPS Tracking (IEldGpsTrackingProvider)

    public async Task<IEnumerable<EldVehicleLocationDto>> GetAllVehicleLocationsAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"{baseUrl}/api/v2/units-by-usdot/{usdot}", ct);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to get vehicle locations: {StatusCode}", response.StatusCode);
                return [];
            }

            var result = await response.Content.ReadFromJsonAsync<TtEldTrackingV2Response>(ct);
            return result?.Units?.Select(TtEldMapper.MapToLocationDto) ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching vehicle locations");
            return [];
        }
    }

    #endregion
}
