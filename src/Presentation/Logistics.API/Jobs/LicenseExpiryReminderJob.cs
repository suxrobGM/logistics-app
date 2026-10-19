using Logistics.Application.Modules.Compliance.Safety.Services;
using Hangfire;

namespace Logistics.API.Jobs;

public static class LicenseExpiryReminderJob
{
    public static void ScheduleJobs()
    {
        RecurringJob.AddOrUpdate<ILicenseExpiryReminderService>(
            "ProcessLicenseExpiryReminders",
            x => x.ProcessLicenseExpiryRemindersAsync(default),
            Cron.Daily(6));
    }
}
