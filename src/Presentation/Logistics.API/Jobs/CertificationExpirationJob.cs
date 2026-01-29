using Hangfire;
using Logistics.Application.Services;

namespace Logistics.API.Jobs;

public static class CertificationExpirationJob
{
    public static void ScheduleJobs()
    {
        // Schedule daily check for expiring certifications at 6:00 AM UTC
        RecurringJob.AddOrUpdate<ICertificationReminderService>(
            "ProcessExpiringCertifications",
            x => x.ProcessExpiringCertificationsAsync(default),
            Cron.Daily(6));
    }
}
