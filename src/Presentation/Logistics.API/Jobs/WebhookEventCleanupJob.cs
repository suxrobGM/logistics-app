using Logistics.Application.Modules.Integrations.Webhooks.Services;
using Hangfire;

namespace Logistics.API.Jobs;

public static class WebhookEventCleanupJob
{
    public static void ScheduleJobs()
    {
        // Daily at 02:00 UTC. Prunes processed-webhook idempotency rows (master DB) older than 30 days.
        RecurringJob.AddOrUpdate<IProcessedWebhookEventCleanupService>(
            "webhook-event-cleanup",
            x => x.CleanupAsync(default),
            Cron.Daily(2));
    }
}
