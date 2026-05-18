using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.Eld.Common;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Logistics.Application.Abstractions.Eld;

namespace Logistics.Infrastructure.Integrations.Eld.Providers.TtEld;

/// <summary>
///     TT ELD provider implementation.
///     API Documentation: https://developer.tteld.com/
///     GPS tracking-focused provider - does not support HOS or violations.
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
        catch (HttpRequestException ex)
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
        => Task.FromResult<EldDriverHosDataDto?>(null);

    public Task<IEnumerable<EldDriverHosDataDto>> GetAllDriversHosStatusAsync()
        => Task.FromResult<IEnumerable<EldDriverHosDataDto>>([]);

    public Task<IEnumerable<EldViolationDataDto>> GetDriverViolationsAsync(
        string externalDriverId,
        DateTime startDate,
        DateTime endDate)
        => Task.FromResult<IEnumerable<EldViolationDataDto>>([]);

    #endregion

    public async Task<IEnumerable<EldHosLogEntryDto>> GetDriverHosLogsAsync(
        string externalDriverId,
        DateTime startDate,
        DateTime endDate)
    {
        var fromStr = startDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        var toStr = endDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

        var result = await httpClient.TryGetFromJsonAsync<List<TtEldTrackingPoint>>(
            $"{baseUrl}/api/externalservice/trackings/{usdot}/{externalDriverId}/?from={fromStr}&to={toStr}",
            EldJsonOptions.CamelCase,
            logger,
            $"TT ELD tracking for vehicle {externalDriverId}");
        return result?.Select(p => TtEldMapper.MapToLogDto(p, externalDriverId)) ?? [];
    }

    public async Task<IEnumerable<EldDriverDto>> GetAllDriversAsync()
    {
        return await GetAllPaginatedAsync<TtEldDriversResponse, TtEldDriverData, EldDriverDto>(
            page => $"{baseUrl}/api/externalservice/drivers-list/{usdot}?page={page}&perPage=100&is_active=true",
            r => r.Data,
            r => r.Meta?.TotalPages ?? 1,
            TtEldMapper.MapToDriverDto,
            "TT ELD drivers");
    }

    public async Task<IEnumerable<EldVehicleDto>> GetAllVehiclesAsync()
    {
        return await GetAllPaginatedAsync<TtEldUnitsResponse, TtEldUnitData, EldVehicleDto>(
            page => $"{baseUrl}/api/externalservice/current-units/{usdot}?page={page}&perPage=100&is_active=true",
            r => r.Data,
            r => r.Meta?.TotalPages ?? 1,
            TtEldMapper.MapToVehicleDto,
            "TT ELD units");
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
        var result = await httpClient.TryGetFromJsonAsync<TtEldTrackingV2Response>(
            $"{baseUrl}/api/v2/units-by-usdot/{usdot}",
            EldJsonOptions.CamelCase,
            logger,
            "TT ELD vehicle locations",
            ct);
        return result?.Units?.Select(TtEldMapper.MapToLocationDto) ?? [];
    }

    #endregion

    private async Task<List<TOut>> GetAllPaginatedAsync<TResponse, TItem, TOut>(
        Func<int, string> urlForPage,
        Func<TResponse, IEnumerable<TItem>?> selectItems,
        Func<TResponse, int> selectTotalPages,
        Func<TItem, TOut> map,
        string action)
    {
        var all = new List<TOut>();
        var page = 1;
        int totalPages;

        do
        {
            var result = await httpClient.TryGetFromJsonAsync<TResponse>(
                urlForPage(page),
                EldJsonOptions.CamelCase,
                logger,
                $"{action} page {page}");
            if (result is null)
            {
                break;
            }

            var items = selectItems(result);
            if (items is not null)
            {
                all.AddRange(items.Select(map));
            }

            totalPages = selectTotalPages(result);
            page++;
        } while (page <= totalPages);

        return all;
    }
}
