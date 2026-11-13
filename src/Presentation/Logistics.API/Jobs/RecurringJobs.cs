using Hangfire;
using Logistics.Application.Abstractions.ProductLicense;
using Logistics.Application.Modules.Compliance.Ifta.Services;
using Logistics.Application.Modules.Compliance.Privacy.Services;
using Logistics.Application.Modules.IdentityAccess.Invitations.Services;
using Logistics.Application.Modules.Integrations.Webhooks.Services;

namespace Logistics.API.Jobs;

/// <summary>
/// Single-call jobs on the master database; Hangfire resolves them with no tenant set, so per-tenant
/// work gets a job class on TenantJobRunner instead.
/// </summary>
public static class RecurringJobs
{
    public static void Schedule()
    {
        RecurringJob.AddOrUpdate<IProcessedWebhookEventCleanupService>(
            "webhook-event-cleanup",
            x => x.CleanupAsync(default),
            Cron.Daily(2));

        RecurringJob.AddOrUpdate<IDataDeletionProcessingService>(
            "privacy-deletion",
            x => x.ProcessDueAsync(default),
            Cron.Daily(3));

        RecurringJob.AddOrUpdate<IInvitationExpiryService>(
            "ExpireStaleInvitations",
            x => x.ExpireStaleAsync(default),
            Cron.Daily(3));

        RecurringJob.AddOrUpdate<IProductLicenseHeartbeatService>(
            "product-license-heartbeat",
            x => x.SendHeartbeatAsync(default),
            Cron.Daily(3));

        RecurringJob.AddOrUpdate<IDataExportExpiryService>(
            "privacy-export-expiry",
            x => x.ExpireAsync(default),
            Cron.Daily(5));

        // 13:00 UTC is morning in every US time zone.
        RecurringJob.AddOrUpdate<IIftaFilingReminderService>(
            "ifta-filing-reminders",
            x => x.ProcessRemindersAsync(default),
            Cron.Daily(13));

        RecurringJob.AddOrUpdate<IDataExportProcessingService>(
            "privacy-export-processing",
            x => x.ProcessPendingAsync(default),
            Cron.MinuteInterval(10));
    }
}
