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
            .GetAsync(c => c.ProviderType == req.ProviderType && c.IsActive, ct);

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
                await HandleViolationResolved(webhookResult, config);
                break;

            default:
                logger.LogDebug("Unhandled webhook event type: {EventType}", webhookResult.EventType);
                break;
        }

        await tenantUow.SaveChangesAsync(ct);
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
        if (string.IsNullOrEmpty(webhookResult.ExternalDriverId))
        {
            logger.LogWarning("ViolationCreated webhook missing external driver ID");
            return;
        }

        // Find driver mapping
        var mapping = await tenantUow.Repository<EldDriverMapping>()
            .GetAsync(m => m.ExternalDriverId == webhookResult.ExternalDriverId
                           && m.ProviderType == config.ProviderType);

        if (mapping == null)
        {
            logger.LogDebug("No mapping found for external driver {DriverId}", webhookResult.ExternalDriverId);
            return;
        }

        // Fetch violation details from provider API
        var providerService = eldProviderFactory.GetProvider(config);
        var violations = await providerService.GetDriverViolationsAsync(
            webhookResult.ExternalDriverId,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow);

        var latestViolation = violations.OrderByDescending(v => v.ViolationDate).FirstOrDefault();
        if (latestViolation is null)
        {
            logger.LogWarning("Could not fetch violation details for driver {DriverId}",
                webhookResult.ExternalDriverId);
            return;
        }

        // Check for duplicate violation
        if (!string.IsNullOrEmpty(latestViolation.ExternalViolationId))
        {
            var existingViolation = await tenantUow.Repository<HosViolation>()
                .GetAsync(v => v.ExternalViolationId == latestViolation.ExternalViolationId);

            if (existingViolation != null)
            {
                logger.LogDebug("Violation {ViolationId} already exists", latestViolation.ExternalViolationId);
                return;
            }
        }

        // Create new violation entity
        var violation = new HosViolation
        {
            EmployeeId = mapping.EmployeeId,
            ViolationDate = latestViolation.ViolationDate,
            ViolationType = latestViolation.ViolationType,
            Description = latestViolation.Description,
            SeverityLevel = latestViolation.SeverityLevel,
            IsResolved = false,
            ExternalViolationId = latestViolation.ExternalViolationId,
            ProviderType = config.ProviderType
        };

        await tenantUow.Repository<HosViolation>().AddAsync(violation);

        // Update driver HOS status to show in violation
        var hosStatus = await tenantUow.Repository<DriverHosStatus>()
            .GetAsync(h => h.EmployeeId == mapping.EmployeeId);

        if (hosStatus is not null)
        {
            hosStatus.IsInViolation = true;
            hosStatus.LastUpdatedAt = DateTime.UtcNow;
        }

        logger.LogInformation("Created HOS violation for employee {EmployeeId}: {ViolationType}",
            mapping.EmployeeId, latestViolation.ViolationType);
    }

    private async Task HandleViolationResolved(EldWebhookResultDto webhookResult, EldProviderConfiguration config)
    {
        if (string.IsNullOrEmpty(webhookResult.ExternalDriverId))
        {
            logger.LogWarning("ViolationResolved webhook missing external driver ID");
            return;
        }

        // Find driver mapping
        var mapping = await tenantUow.Repository<EldDriverMapping>()
            .GetAsync(m => m.ExternalDriverId == webhookResult.ExternalDriverId
                           && m.ProviderType == config.ProviderType);

        if (mapping == null)
        {
            logger.LogDebug("No mapping found for external driver {DriverId}", webhookResult.ExternalDriverId);
            return;
        }

        // Find unresolved violations for this driver
        var unresolvedViolations = await tenantUow.Repository<HosViolation>()
            .GetListAsync(v => v.EmployeeId == mapping.EmployeeId && !v.IsResolved);

        if (!unresolvedViolations.Any())
        {
            logger.LogDebug("No unresolved violations found for employee {EmployeeId}", mapping.EmployeeId);
            return;
        }

        // Mark the most recent unresolved violation as resolved
        var violationToResolve = unresolvedViolations
            .OrderByDescending(v => v.ViolationDate)
            .First();

        violationToResolve.IsResolved = true;
        violationToResolve.ResolvedAt = DateTime.UtcNow;

        // Check if there are still other unresolved violations
        var remainingUnresolved = unresolvedViolations.Count - 1;

        // Update driver HOS status if no more violations
        if (remainingUnresolved == 0)
        {
            var hosStatus = await tenantUow.Repository<DriverHosStatus>()
                .GetAsync(h => h.EmployeeId == mapping.EmployeeId);

            if (hosStatus != null)
            {
                hosStatus.IsInViolation = false;
                hosStatus.LastUpdatedAt = DateTime.UtcNow;
            }
        }

        logger.LogInformation("Resolved HOS violation for employee {EmployeeId}, {RemainingCount} violations remaining",
            mapping.EmployeeId, remainingUnresolved);
    }
}
