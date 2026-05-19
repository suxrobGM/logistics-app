using Logistics.Application.Modules.Compliance.Privacy.Services;
using Hangfire;

namespace Logistics.API.Jobs;

public static class DataExportExpiryJob
{
    public static void ScheduleJobs()
    {
        // Daily at 05:00 UTC.
        RecurringJob.AddOrUpdate<IDataExportExpiryService>(
            "privacy-export-expiry",
            x => x.ExpireAsync(default),
            Cron.Daily(5));
    }
}
