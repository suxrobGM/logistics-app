using System.Text.Json;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.Eld.Common;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Logistics.Application.Abstractions.Eld;

namespace Logistics.Infrastructure.Integrations.Eld.Providers.Geotab;

/// <summary>
///     Geotab MyGeotab provider implementation. Serves both US (FMCSA) and EU
///     (561/2006 tachograph) fleets — the active rule set is inferred from
///     the tenant's region for violation mapping. HOS counters and violations
///     themselves are always provider-sourced; we never compute them locally.
///     API reference: https://geotab.github.io/sdk/.
/// </summary>
internal class GeotabEldService(
    GeotabClient client,
    ITenantUnitOfWork tenantUow,
    IOptions<EldOptions> options,
    ILogger<GeotabEldService> logger)
    : IEldProviderService
{
    public EldProviderType ProviderType => EldProviderType.Geotab;

    public void Initialize(EldProviderConfiguration configuration)
    {
        var baseUrl = options.Value.Geotab?.BaseUrl ?? "https://my.geotab.com";
        client.SetBaseUrl(baseUrl);

        // Geotab credentials are encoded as "database|userName" in ApiKey, password in ApiSecret.
        var (database, userName) = ParseAccount(configuration.ApiKey);
        var password = configuration.ApiSecret;

        if (string.IsNullOrEmpty(database) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
        {
            logger.LogWarning("Geotab configuration is incomplete (need ApiKey='database|userName' and ApiSecret=password)");
            return;
        }

        // Fire-and-wait: Initialize is sync; auth issues will surface on first call.
        _ = client.AuthenticateAsync(database, userName, password).GetAwaiter().GetResult();
    }

    public async Task<bool> ValidateCredentialsAsync(string apiKey, string? apiSecret)
    {
        var baseUrl = options.Value.Geotab?.BaseUrl ?? "https://my.geotab.com";
        client.SetBaseUrl(baseUrl);
        var (database, userName) = ParseAccount(apiKey);
        return !string.IsNullOrEmpty(database)
               && !string.IsNullOrEmpty(userName)
               && !string.IsNullOrEmpty(apiSecret)
               && await client.AuthenticateAsync(database, userName, apiSecret);
    }

    public Task<OAuthTokenResultDto?> RefreshTokenAsync(string refreshToken)
    {
        // Geotab uses session credentials, not OAuth refresh tokens.
        return Task.FromResult<OAuthTokenResultDto?>(null);
    }

    public async Task<EldDriverHosDataDto?> GetDriverHosStatusAsync(string externalDriverId)
    {
        var availability = await client.GetAsync<GeotabDutyStatusAvailability>(
            "DutyStatusAvailability",
            new { deviceSearch = new { id = externalDriverId } });

        var first = availability?.FirstOrDefault();
        return first is null ? null : GeotabMapper.MapToHosDto(externalDriverId, first);
    }

    public async Task<IEnumerable<EldDriverHosDataDto>> GetAllDriversHosStatusAsync()
    {
        var availability = await client.GetAsync<GeotabDutyStatusAvailability>("DutyStatusAvailability");
        return availability?
            .Where(a => a.Driver?.Id is not null)
            .Select(a => GeotabMapper.MapToHosDto(a.Driver!.Id!, a))
            ?? [];
    }

    public async Task<IEnumerable<EldHosLogEntryDto>> GetDriverHosLogsAsync(
        string externalDriverId,
        DateTime startDate,
        DateTime endDate)
    {
        var logs = await client.GetAsync<GeotabDutyStatusLog>(
            "DutyStatusLog",
            new
            {
                driverSearch = new { id = externalDriverId },
                fromDate = startDate,
                toDate = endDate
            });
        return logs?.Select(GeotabMapper.MapToLogDto) ?? [];
    }

    public async Task<IEnumerable<EldViolationDataDto>> GetDriverViolationsAsync(
        string externalDriverId,
        DateTime startDate,
        DateTime endDate)
    {
        var region = tenantUow.GetCurrentTenant().Settings.Region;
        var violations = await client.GetAsync<GeotabDutyStatusViolation>(
            "DutyStatusViolation",
            new
            {
                driverSearch = new { id = externalDriverId },
                fromDate = startDate,
                toDate = endDate
            });
        return violations?.Select(v => GeotabMapper.MapToViolationDto(v, region)) ?? [];
    }

    public async Task<IEnumerable<EldDriverDto>> GetAllDriversAsync()
    {
        var users = await client.GetAsync<GeotabUser>(
            "User",
            new { isDriver = true });
        return users?.Select(GeotabMapper.MapToDriverDto) ?? [];
    }

    public async Task<IEnumerable<EldVehicleDto>> GetAllVehiclesAsync()
    {
        var devices = await client.GetAsync<GeotabDevice>("Device");
        return devices?.Select(GeotabMapper.MapToVehicleDto) ?? [];
    }

    public Task<EldWebhookResultDto> ProcessWebhookAsync(string payload, string? signature, string? webhookSecret)
    {
        // Geotab does not natively push HOS events; integrators sign Add-In / IOX
        // forwarder calls with HMAC-SHA256 of the body using the configured secret.
        // When a secret is configured, reject any payload without a matching signature.
        if (!string.IsNullOrEmpty(webhookSecret)
            && !WebhookSignature.VerifyHmacSha256(payload, signature, webhookSecret))
        {
            logger.LogWarning("Rejected Geotab webhook with invalid signature");
            return Task.FromResult(InvalidWebhook("Invalid webhook signature"));
        }

        try
        {
            var webhook = JsonSerializer.Deserialize<GeotabWebhookPayload>(payload, EldJsonOptions.CamelCase);
            if (webhook is null)
            {
                return Task.FromResult(InvalidWebhook("Failed to parse webhook payload"));
            }

            var eventType = webhook.EventType?.ToLowerInvariant() switch
            {
                "dutystatuschanged" or "duty_status_changed" => EldWebhookEventType.DutyStatusChanged,
                "violationcreated" or "violation_created" => EldWebhookEventType.ViolationCreated,
                "violationresolved" or "violation_resolved" => EldWebhookEventType.ViolationResolved,
                _ => EldWebhookEventType.Unknown
            };

            return Task.FromResult(new EldWebhookResultDto
            {
                EventType = eventType,
                ExternalDriverId = webhook.DriverId,
                ExternalVehicleId = webhook.VehicleId,
                Data = webhook.Data,
                IsValid = true
            });
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Error processing Geotab webhook");
            return Task.FromResult(InvalidWebhook(ex.Message));
        }
    }

    private static EldWebhookResultDto InvalidWebhook(string error) => new()
    {
        EventType = EldWebhookEventType.Unknown,
        IsValid = false,
        ErrorMessage = error
    };

    private static (string? database, string? userName) ParseAccount(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return (null, null);
        }

        var parts = apiKey.Split('|', 2);
        return parts.Length == 2 ? (parts[0], parts[1]) : (null, parts[0]);
    }
}
