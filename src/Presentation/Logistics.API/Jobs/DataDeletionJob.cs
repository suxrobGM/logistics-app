using Logistics.Application.Modules.Compliance.Privacy.Services;
using Hangfire;

namespace Logistics.API.Jobs;

public static class DataDeletionJob
{
    public static void ScheduleJobs()
    {
        // Process due deletion requests daily at 03:00 UTC.
        RecurringJob.AddOrUpdate<IDataDeletionProcessingService>(
            "privacy-deletion",
            x => x.ProcessDueAsync(default),
            Cron.Daily(3));
    }
}
