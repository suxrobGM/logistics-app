using Hangfire;
using Logistics.Application.Modules.Compliance.Ifta.Services;

namespace Logistics.API.Jobs;

public static class IftaFilingReminderJob
{
    public static void ScheduleJobs()
    {
        // Daily at 13:00 UTC (morning across US time zones)
        RecurringJob.AddOrUpdate<IIftaFilingReminderService>(
            "ifta-filing-reminders",
            x => x.ProcessRemindersAsync(default),
            Cron.Daily(13));
    }
}
