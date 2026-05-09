using Hangfire;
using Logistics.Application.Services.Privacy;

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
