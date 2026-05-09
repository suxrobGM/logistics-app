using Hangfire;
using Logistics.Application.Services.Privacy;

namespace Logistics.API.Jobs;

public static class DataExportProcessingJob
{
    public static void ScheduleJobs()
    {
        // Pick up pending data exports every 10 minutes.
        RecurringJob.AddOrUpdate<IDataExportProcessingService>(
            "privacy-export-processing",
            x => x.ProcessPendingAsync(default),
            Cron.MinuteInterval(10));
    }
}
