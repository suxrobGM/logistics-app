using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Realtime;

namespace Logistics.Application.Modules.Compliance.Safety.Services;

internal sealed class LicenseExpiryReminderService(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    IRealtimeNotificationService notificationService,
    ILogger<LicenseExpiryReminderService> logger)
    : ILicenseExpiryReminderService
{
    /// <summary>
    /// Reminder thresholds in days. Driver receives a notification once per threshold.
    /// </summary>
    private static readonly int[] ReminderDays = [60, 30, 7];

    public async Task ProcessLicenseExpiryRemindersAsync(CancellationToken ct = default)
    {
        var tenants = await masterUow.Repository<Tenant>().GetListAsync(ct: ct);

        foreach (var tenant in tenants)
        {
            try
            {
                tenantUow.SetCurrentTenant(tenant);
                await ProcessTenantAsync(tenant, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Error processing license expiry reminders for tenant {TenantName}",
                    tenant.Name);
            }
        }
    }

    private async Task ProcessTenantAsync(Tenant tenant, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var maxThresholdDays = ReminderDays.Max();
        var maxDate = now.AddDays(maxThresholdDays);

        var licenses = await tenantUow.Repository<DriverLicense>()
            .GetListAsync(
                l => l.Status == DriverLicenseStatus.Active && l.ExpiresAt <= maxDate,
                ct);

        var sentCount = 0;

        foreach (var license in licenses)
        {
            var daysUntilExpiry = (int)Math.Ceiling((license.ExpiresAt - now).TotalDays);

            // Pick the smallest threshold that the license is at or under.
            // e.g. 25 days â†’ uses the 30-day threshold.
            var threshold = ReminderDays.Where(t => daysUntilExpiry <= t).DefaultIfEmpty(-1).Min();
            if (threshold < 0)
            {
                continue;
            }

            // Skip if we've already sent for this threshold.
            if (license.LastReminderThresholdDays.HasValue
                && license.LastReminderThresholdDays.Value <= threshold)
            {
                continue;
            }

            await SendNotificationAsync(tenant.Id.ToString(), license, daysUntilExpiry);

            license.LastReminderSentAt = now;
            license.LastReminderThresholdDays = threshold;
            tenantUow.Repository<DriverLicense>().Update(license);
            sentCount++;
        }

        if (sentCount > 0)
        {
            await tenantUow.SaveChangesAsync(ct);
        }

        logger.LogInformation(
            "Sent {Count} license-expiry reminders for tenant {TenantName}",
            sentCount, tenant.Name);
    }

    private async Task SendNotificationAsync(string tenantId, DriverLicense license, int daysUntilExpiry)
    {
        var className = license.LicenseClass.GetDescription();
        var title = daysUntilExpiry <= 0
            ? $"Driver license expired: {className}"
            : $"Driver license expiring in {daysUntilExpiry} days: {className}";

        var notification = new NotificationDto
        {
            Title = title,
            Message =
                $"License {license.LicenseNumber} ({license.IssuingCountry}) expires on " +
                $"{license.ExpiresAt:yyyy-MM-dd}.",
            CreatedDate = DateTime.UtcNow
        };

        await notificationService.BroadcastNotificationAsync(tenantId, notification);
    }
}
