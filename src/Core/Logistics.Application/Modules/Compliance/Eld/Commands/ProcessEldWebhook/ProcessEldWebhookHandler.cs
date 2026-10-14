using Logistics.Application.Abstractions.Eld;
using Logistics.Application.Abstractions.Features;
using Logistics.Application.Modules.Compliance.Eld.Services;
using Logistics.Application.Modules.Integrations.Webhooks.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Compliance.Eld.Commands;

internal sealed class ProcessEldWebhookHandler(
    ITenantUnitOfWork tenantUow,
    IEldProviderFactory eldProviderFactory,
    IEldSyncService eldSyncService,
    IWebhookEventTracker webhookEvents,
    IFeatureService featureService,
    ILogger<ProcessEldWebhookHandler> logger)
    : IRequestHandler<ProcessEldWebhookCommand, Result>
{
    public async Task<Result> Handle(ProcessEldWebhookCommand req, CancellationToken ct)
    {
        try
        {
            await tenantUow.SetCurrentTenantByIdAsync(req.TenantId);
        }
        catch (InvalidOperationException)
        {
            logger.LogWarning("{ProviderType} webhook for unknown tenant {TenantId}", req.ProviderType, req.TenantId);
            return Result.Fail("Unknown tenant");
        }

        // A tenant downgraded off ELD still gets deliveries until it removes the webhook; acknowledge so
        // the provider stops retrying, but write nothing.
        if (!await featureService.IsFeatureEnabledAsync(req.TenantId, TenantFeature.Eld))
        {
            return Result.Ok();
        }

        var config = await tenantUow.Repository<EldProviderConfiguration>()
            .GetAsync(c => c.ProviderType == req.ProviderType && c.IsActive, ct);

        if (config is null)
        {
            logger.LogWarning("Received webhook for unconfigured provider {ProviderType}", req.ProviderType);
            return Result.Fail("Provider not configured");
        }

        var providerService = eldProviderFactory.GetProvider(config);
        var webhookResult = await providerService.ProcessWebhookAsync(
            new EldWebhookRequestDto(req.RequestBodyJson, req.Signature, req.Timestamp),
            config.WebhookSecret);

        if (!webhookResult.IsValid)
        {
            logger.LogWarning("Invalid webhook from {ProviderType}: {Error}",
                req.ProviderType, webhookResult.ErrorMessage);
            return Result.Fail(webhookResult.ErrorMessage ?? "Invalid webhook");
        }

        // Idempotency (master DB): providers retry deliveries. Prefer a provider-supplied event id;
        // otherwise fall back to a hash of the raw body so a re-sent payload maps to the same key. Scoped
        // by tenant: the ledger is shared, and two tenants can receive byte-identical bodies.
        var provider = req.ProviderType.ToString();
        var eventKey = $"{req.TenantId}:{webhookResult.ExternalEventId ?? webhookEvents.BuildKeyFromBody(req.RequestBodyJson)}";

        if (await webhookEvents.WasAlreadyHandledAsync(provider, eventKey, ct))
        {
            logger.LogInformation("Duplicate {Provider} webhook '{EventKey}' ignored", provider, eventKey);
            return Result.Ok();
        }

        var mapping = await FindMappingAsync(config, webhookResult.ExternalDriverId, ct);
        switch (webhookResult.EventType)
        {
            case EldWebhookEventType.DutyStatusChanged when mapping is not null:
                await eldSyncService.SyncDriverHosStatusAsync(providerService, mapping, ct);
                break;

            case EldWebhookEventType.ViolationCreated when mapping is not null:
                await eldSyncService.SyncDriverViolationsAsync(providerService, mapping, ct);
                break;

            case EldWebhookEventType.ViolationResolved when mapping is not null:
                await ResolveLatestViolationAsync(mapping, ct);
                break;

            default:
                logger.LogDebug("No action for {ProviderType} webhook {EventType} (driver mapped: {Mapped})",
                    req.ProviderType, webhookResult.EventType, mapping is not null);
                break;
        }

        await webhookEvents.MarkHandledAsync(provider, eventKey, ct);
        return Result.Ok();
    }

    private async Task<EldDriverMapping?> FindMappingAsync(
        EldProviderConfiguration config, string? externalDriverId, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(externalDriverId))
        {
            return null;
        }

        return await tenantUow.Repository<EldDriverMapping>()
            .GetAsync(m => m.ExternalDriverId == externalDriverId && m.ProviderType == config.ProviderType, ct);
    }

    private async Task ResolveLatestViolationAsync(EldDriverMapping mapping, CancellationToken ct)
    {
        var unresolved = await tenantUow.Repository<HosViolation>()
            .GetListAsync(v => v.EmployeeId == mapping.EmployeeId && !v.IsResolved, ct);

        var latest = unresolved.MaxBy(v => v.ViolationDate);
        if (latest is null)
        {
            return;
        }

        latest.IsResolved = true;
        latest.ResolvedAt = DateTime.UtcNow;

        if (unresolved.Count == 1)
        {
            var hosStatus = await tenantUow.Repository<DriverHosStatus>()
                .GetAsync(h => h.EmployeeId == mapping.EmployeeId, ct);
            if (hosStatus is not null)
            {
                hosStatus.IsInViolation = false;
                hosStatus.LastUpdatedAt = DateTime.UtcNow;
            }
        }

        await tenantUow.SaveChangesAsync(ct);
        logger.LogInformation("Resolved HOS violation for employee {EmployeeId}", mapping.EmployeeId);
    }
}
