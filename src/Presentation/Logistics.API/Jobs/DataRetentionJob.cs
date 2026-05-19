using Logistics.Application.Modules.Compliance.Privacy.Services;
using Hangfire;

namespace Logistics.API.Jobs;

public static class DataRetentionJob
{
    public static void ScheduleJobs()
    {
        // Daily at 04:00 UTC.
        RecurringJob.AddOrUpdate<IDataRetentionService>(
            "privacy-retention",
            x => x.PurgeAsync(default),
            Cron.Daily(4));
    }
}
