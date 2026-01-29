using Hangfire;
using Logistics.Application.Services;

namespace Logistics.API.Jobs;

public static class MaintenanceReminderJob
{
    public static void ScheduleJobs()
    {
        // Schedule daily check for upcoming and overdue maintenance at 7:00 AM UTC
        RecurringJob.AddOrUpdate<IMaintenanceReminderService>(
            "ProcessMaintenanceReminders",
            x => x.ProcessMaintenanceRemindersAsync(default),
            Cron.Daily(7));
    }
}
