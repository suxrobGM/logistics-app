using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using Logistics.Application.Abstractions.Eld;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.Common;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Integrations.Eld.Providers.Samsara;

/// <summary>
///     Samsara ELD and telematics provider. API reference: https://developers.samsara.com/reference
/// </summary>
internal class SamsaraEldService(
    HttpClient httpClient,
    IOptions<EldOptions> options,
    ILogger<SamsaraEldService> logger)
    : IEldProviderService, IEldGpsTrackingProvider
{
    private readonly SamsaraOptions _options = options.Value.Samsara ?? new SamsaraOptions();
    private string? _region;

    private string BaseUrl => _options.BaseUrlFor(_region);

    public EldProviderType ProviderType => EldProviderType.Samsara;

    public void Initialize(EldProviderConfiguration configuration)
    {
        _region = configuration.Region;
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", configuration.ApiKey);
    }

    public async Task<bool> ValidateCredentialsAsync(string apiKey, string? apiSecret)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/fleet/drivers?limit=1");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            using var response = await httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to validate Samsara credentials");
            return false;
        }
    }

    public async Task<EldDriverHosDataDto?> GetDriverHosStatusAsync(string externalDriverId)
    {
        var clocks = await GetAllPagesAsync<SamsaraHosClock>(
            $"/fleet/hos/clocks?driverIds={Uri.EscapeDataString(externalDriverId)}",
            $"Samsara HOS clocks for driver {externalDriverId}");
        return clocks?.Select(SamsaraMapper.MapToDto).FirstOrDefault(c => c is not null);
    }

    public async Task<IEnumerable<EldDriverHosDataDto>> GetAllDriversHosStatusAsync()
    {
        var clocks = await GetAllPagesAsync<SamsaraHosClock>("/fleet/hos/clocks", "Samsara HOS clocks");
        return clocks?.Select(SamsaraMapper.MapToDto).OfType<EldDriverHosDataDto>().ToList() ?? [];
    }

    public async Task<IEnumerable<EldHosLogEntryDto>> GetDriverHosLogsAsync(
        string externalDriverId,
        DateTime startDate,
        DateTime endDate)
    {
        var drivers = await GetAllPagesAsync<SamsaraDriverHosLogs>(
            $"/fleet/hos/logs?driverIds={Uri.EscapeDataString(externalDriverId)}" +
            $"&startTime={Rfc3339(startDate)}&endTime={Rfc3339(endDate)}",
            $"Samsara HOS logs for driver {externalDriverId}");

        return drivers?
            .SelectMany(d => (d.HosLogs ?? []).Select(l => SamsaraMapper.MapToLogDto(d.Driver?.Id ?? externalDriverId, l)))
            .OfType<EldHosLogEntryDto>()
            .ToList() ?? [];
    }

    public async Task<IEnumerable<EldViolationDataDto>> GetDriverViolationsAsync(
        string externalDriverId,
        DateTime startDate,
        DateTime endDate)
    {
        var groups = await GetAllPagesAsync<SamsaraDriverViolations>(
            $"/fleet/hos/violations?driverIds={Uri.EscapeDataString(externalDriverId)}" +
            $"&startTime={Rfc3339(startDate)}&endTime={Rfc3339(endDate)}",
            $"Samsara HOS violations for driver {externalDriverId}");

        return groups?
            .SelectMany(g => g.Violations ?? [])
            .Select(SamsaraMapper.MapToViolationDto)
            .OfType<EldViolationDataDto>()
            .ToList() ?? [];
    }

    public async Task<IEnumerable<EldDriverDto>> GetAllDriversAsync()
    {
        var drivers = await GetAllPagesAsync<SamsaraDriver>("/fleet/drivers", "Samsara drivers");
        return drivers?.Where(d => !string.IsNullOrEmpty(d.Id)).Select(SamsaraMapper.MapToDriverDto).ToList() ?? [];
    }

    public async Task<IEnumerable<EldVehicleDto>> GetAllVehiclesAsync()
    {
        var vehicles = await GetAllPagesAsync<SamsaraVehicle>("/fleet/vehicles", "Samsara vehicles");
        return vehicles?.Where(v => !string.IsNullOrEmpty(v.Id)).Select(SamsaraMapper.MapToVehicleDto).ToList() ?? [];
    }

    public async Task<IEnumerable<EldVehicleLocationDto>> GetAllVehicleLocationsAsync(CancellationToken ct = default)
    {
        var stats = await GetAllPagesAsync<SamsaraVehicleStats>(
            "/fleet/vehicles/stats?types=gps,obdOdometerMeters,gpsOdometerMeters",
            "Samsara vehicle stats",
            ct);
        return stats?.Select(SamsaraMapper.MapToLocationDto).OfType<EldVehicleLocationDto>().ToList() ?? [];
    }

    public Task<EldWebhookResultDto> ProcessWebhookAsync(EldWebhookRequestDto request, string? webhookSecret)
    {
        if (!WebhookSignature.VerifySamsara(request.Body, request.Timestamp, request.Signature, webhookSecret))
        {
            logger.LogWarning("Rejected Samsara webhook with invalid or unverifiable signature");
            return Task.FromResult(InvalidWebhook("Invalid webhook signature"));
        }

        try
        {
            var webhook = JsonSerializer.Deserialize<SamsaraWebhookPayload>(request.Body, IntegrationJsonOptions.CamelCase);
            return Task.FromResult(webhook is null
                ? InvalidWebhook("Failed to parse webhook payload")
                : SamsaraMapper.MapToWebhookResult(webhook));
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Unparseable Samsara webhook payload");
            return Task.FromResult(InvalidWebhook("Failed to parse webhook payload"));
        }
    }

    /// <summary>
    /// Follows <c>pagination.endCursor</c> through every page. Null when any page fails, so a caller
    /// never mistakes a partial read for the full set.
    /// </summary>
    private async Task<List<T>?> GetAllPagesAsync<T>(string pathAndQuery, string action, CancellationToken ct = default)
    {
        var items = new List<T>();
        var separator = pathAndQuery.Contains('?') ? '&' : '?';
        string? cursor = null;

        do
        {
            var url = cursor is null
                ? $"{BaseUrl}{pathAndQuery}"
                : $"{BaseUrl}{pathAndQuery}{separator}after={Uri.EscapeDataString(cursor)}";

            var page = await httpClient.TryGetFromJsonAsync<SamsaraPage<T>>(
                url, logger, action, IntegrationJsonOptions.CamelCase, ct);
            if (page is null)
            {
                return null;
            }

            items.AddRange(page.Data ?? []);

            var next = page.Pagination is { HasNextPage: true, EndCursor.Length: > 0 }
                ? page.Pagination.EndCursor
                : null;
            cursor = next == cursor ? null : next;
        } while (cursor is not null);

        return items;
    }

    private static string Rfc3339(DateTime value) =>
        (value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : value)
        .ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);

    private static EldWebhookResultDto InvalidWebhook(string error) => new()
    {
        EventType = EldWebhookEventType.Unknown,
        IsValid = false,
        ErrorMessage = error
    };
}
