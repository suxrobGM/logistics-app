using Hangfire;
using Logistics.Application.Services;

namespace Logistics.API.Jobs;

public static class LicenseExpiryReminderJob
{
    public static void ScheduleJobs()
    {
        // Run daily at 6:00 AM UTC, before MaintenanceReminderJob.
        RecurringJob.AddOrUpdate<ILicenseExpiryReminderService>(
            "ProcessLicenseExpiryReminders",
            x => x.ProcessLicenseExpiryRemindersAsync(default),
            Cron.Daily(6));
    }
}
