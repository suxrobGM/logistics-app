using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Realtime;

namespace Logistics.Application.Modules.Compliance.Safety.Services;

internal sealed class LicenseExpiryReminderService(
    ITenantUnitOfWork tenantUow,
    IRealtimeNotificationService notificationService,
    ILogger<LicenseExpiryReminderService> logger)
    : ILicenseExpiryReminderService
{
    /// <summary>A license is reminded once per threshold.</summary>
    private static readonly int[] ReminderDays = [60, 30, 7];

    public async Task ProcessLicenseExpiryRemindersAsync(Tenant tenant, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var maxDate = today.AddDays(ReminderDays.Max());

        var licenses = await tenantUow.Repository<DriverLicense>()
            .GetListAsync(
                l => l.Status == DriverLicenseStatus.Active && l.ExpiresAt <= maxDate,
                ct);

        var sentCount = 0;

        foreach (var license in licenses)
        {
            var daysUntilExpiry = license.ExpiresAt.DayNumber - today.DayNumber;

            // Smallest threshold the license is at or under: 25 days uses the 30-day one.
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

            license.RecordReminderSent(threshold);
            sentCount++;
        }

        if (sentCount > 0)
        {
            await tenantUow.SaveChangesAsync(ct);
            logger.LogInformation(
                "Sent {Count} license-expiry reminders for tenant {TenantName}",
                sentCount, tenant.Name);
        }
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
