using Logistics.Application.Abstractions.Features;
using Logistics.Application.Abstractions.Realtime;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Compliance.Ifta.Services;

internal sealed class IftaFilingReminderService(
    IMasterUnitOfWork masterUow,
    IFeatureService featureService,
    IRealtimeNotificationService notificationService,
    ILogger<IftaFilingReminderService> logger) : IIftaFilingReminderService
{
    private static readonly int[] ReminderDays = [30, 14, 7, 3, 1];

    public async Task ProcessRemindersAsync(CancellationToken ct = default)
    {
        var utcNow = DateTime.UtcNow;
        var (year, quarter) = IftaQuarters.Previous(utcNow);
        var deadline = IftaQuarters.FilingDeadline(year, quarter);
        var daysUntilDeadline = (deadline.Date - utcNow.Date).Days;

        if (!ReminderDays.Contains(daysUntilDeadline))
        {
            return;
        }

        var tenants = await masterUow.Repository<Tenant>().GetListAsync(t => t.ConnectionString != null);

        foreach (var tenant in tenants)
        {
            try
            {
                if (!await featureService.IsFeatureEnabledAsync(tenant.Id, TenantFeature.Ifta))
                {
                    continue;
                }

                await notificationService.BroadcastNotificationAsync(tenant.Id.ToString(), new NotificationDto
                {
                    Title = $"IFTA Q{quarter} {year} filing due in {daysUntilDeadline} day(s)",
                    Message = $"The IFTA return for Q{quarter} {year} is due {deadline:MMMM d, yyyy}. Review the IFTA report and file with your base jurisdiction.",
                    CreatedDate = utcNow
                }, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending IFTA filing reminder for tenant {TenantName}", tenant.Name);
            }
        }

        logger.LogInformation("Sent IFTA filing reminders ({Days} days before deadline)", daysUntilDeadline);
    }
}
