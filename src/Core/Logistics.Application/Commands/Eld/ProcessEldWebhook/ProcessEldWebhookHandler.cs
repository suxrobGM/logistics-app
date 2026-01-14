using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class ProcessEldWebhookHandler(
    ITenantUnitOfWork tenantUow,
    IEldProviderFactory eldProviderFactory,
    ILogger<ProcessEldWebhookHandler> logger)
    : IAppRequestHandler<ProcessEldWebhookCommand, Result>
{
    public async Task<Result> Handle(ProcessEldWebhookCommand req, CancellationToken ct)
    {
        // Get provider configuration
        var config = await tenantUow.Repository<EldProviderConfiguration>()
            .GetAsync(c => c.ProviderType == req.ProviderType && c.IsActive);

        if (config == null)
        {
            logger.LogWarning("Received webhook for unconfigured provider {ProviderType}", req.ProviderType);
            return Result.Fail("Provider not configured");
        }

        // Get provider service and process webhook
        var providerService = eldProviderFactory.GetProvider(config);
        var webhookResult = await providerService.ProcessWebhookAsync(
            req.RequestBodyJson,
            req.Signature,
            config.WebhookSecret);

        if (!webhookResult.IsValid)
        {
            logger.LogWarning("Invalid webhook from {ProviderType}: {Error}",
                req.ProviderType, webhookResult.ErrorMessage);
            return Result.Fail(webhookResult.ErrorMessage ?? "Invalid webhook");
        }

        // Handle different webhook event types
        switch (webhookResult.EventType)
        {
            case EldWebhookEventType.DutyStatusChanged:
                await HandleDutyStatusChanged(webhookResult, config);
                break;

            case EldWebhookEventType.ViolationCreated:
                await HandleViolationCreated(webhookResult, config);
                break;

            case EldWebhookEventType.ViolationResolved:
                await HandleViolationResolved(webhookResult);
                break;

            default:
                logger.LogDebug("Unhandled webhook event type: {EventType}", webhookResult.EventType);
                break;
        }

        await tenantUow.SaveChangesAsync();
        return Result.Ok();
    }

    private async Task HandleDutyStatusChanged(EldWebhookResultDto webhookResult, EldProviderConfiguration config)
    {
        if (string.IsNullOrEmpty(webhookResult.ExternalDriverId))
        {
            return;
        }

        // Find the driver mapping
        var mapping = await tenantUow.Repository<EldDriverMapping>()
            .GetAsync(m => m.ExternalDriverId == webhookResult.ExternalDriverId
                           && m.ProviderType == config.ProviderType);

        if (mapping == null)
        {
            logger.LogDebug("No mapping found for external driver {DriverId}", webhookResult.ExternalDriverId);
            return;
        }

        // Trigger a sync for this driver's HOS status
        var providerService = eldProviderFactory.GetProvider(config);
        var hosData = await providerService.GetDriverHosStatusAsync(webhookResult.ExternalDriverId);

        if (hosData == null)
        {
            return;
        }

        // Update HOS status
        var hosStatus = await tenantUow.Repository<DriverHosStatus>()
            .GetAsync(h => h.EmployeeId == mapping.EmployeeId);

        if (hosStatus == null)
        {
            hosStatus = new DriverHosStatus
            {
                EmployeeId = mapping.EmployeeId,
                ExternalDriverId = mapping.ExternalDriverId,
                ProviderType = config.ProviderType,
                CurrentDutyStatus = hosData.CurrentDutyStatus
            };
            await tenantUow.Repository<DriverHosStatus>().AddAsync(hosStatus);
        }

        hosStatus.CurrentDutyStatus = hosData.CurrentDutyStatus;
        hosStatus.StatusChangedAt = hosData.StatusChangedAt;
        hosStatus.DrivingMinutesRemaining = hosData.DrivingMinutesRemaining;
        hosStatus.OnDutyMinutesRemaining = hosData.OnDutyMinutesRemaining;
        hosStatus.CycleMinutesRemaining = hosData.CycleMinutesRemaining;
        hosStatus.IsInViolation = hosData.IsInViolation;
        hosStatus.LastUpdatedAt = DateTime.UtcNow;

        logger.LogInformation("Updated HOS status for employee {EmployeeId} via webhook", mapping.EmployeeId);
    }

    private async Task HandleViolationCreated(EldWebhookResultDto webhookResult, EldProviderConfiguration config)
    {
        // Implementation for handling new violations
        logger.LogInformation("Violation created webhook received for driver {DriverId}",
            webhookResult.ExternalDriverId);
    }

    private async Task HandleViolationResolved(EldWebhookResultDto webhookResult)
    {
        // Implementation for handling resolved violations
        logger.LogInformation("Violation resolved webhook received for driver {DriverId}",
            webhookResult.ExternalDriverId);
    }
}
