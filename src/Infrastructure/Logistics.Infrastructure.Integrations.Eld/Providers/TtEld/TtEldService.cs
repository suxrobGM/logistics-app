using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.Common;
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
    private readonly string _baseUrl = options.Value.TtEld?.BaseUrl ?? "https://read.tteld.com";
    private string? _usdot;

    public EldProviderType ProviderType => EldProviderType.TtEld;

    public void Initialize(EldProviderConfiguration configuration)
    {
        _usdot = configuration.ExternalAccountId
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
                $"{_baseUrl}/api/externalservice/drivers-list/{_usdot}?page=1&perPage=1&is_active=true");
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

    /// <summary>
    /// TT ELD exposes GPS breadcrumbs, not duty-status logs; returning them as logs would fill the HOS
    /// log page with fake Driving entries.
    /// </summary>
    public Task<IEnumerable<EldHosLogEntryDto>> GetDriverHosLogsAsync(
        string externalDriverId,
        DateTime startDate,
        DateTime endDate)
        => Task.FromResult<IEnumerable<EldHosLogEntryDto>>([]);

    #endregion

    public async Task<IEnumerable<EldDriverDto>> GetAllDriversAsync()
    {
        return await GetAllPaginatedAsync<TtEldDriversResponse, TtEldDriverData, EldDriverDto>(
            page => $"{_baseUrl}/api/externalservice/drivers-list/{_usdot}?page={page}&perPage=100&is_active=true",
            r => r.Data,
            r => r.Meta?.TotalPages ?? 1,
            TtEldMapper.MapToDriverDto,
            "TT ELD drivers");
    }

    public async Task<IEnumerable<EldVehicleDto>> GetAllVehiclesAsync()
    {
        return await GetAllPaginatedAsync<TtEldUnitsResponse, TtEldUnitData, EldVehicleDto>(
            page => $"{_baseUrl}/api/externalservice/current-units/{_usdot}?page={page}&perPage=100&is_active=true",
            r => r.Data,
            r => r.Meta?.TotalPages ?? 1,
            TtEldMapper.MapToVehicleDto,
            "TT ELD units");
    }

    public Task<EldWebhookResultDto> ProcessWebhookAsync(EldWebhookRequestDto request, string? webhookSecret)
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
            $"{_baseUrl}/api/v2/units-by-usdot/{_usdot}",
            logger,
            "TT ELD vehicle locations",
            IntegrationJsonOptions.CamelCase,
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
                logger,
                $"{action} page {page}",
                IntegrationJsonOptions.CamelCase);
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
