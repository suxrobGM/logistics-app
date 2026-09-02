using Hangfire;
using Logistics.Application.Abstractions.ProductLicense;

namespace Logistics.API.Jobs;

public static class ProductLicenseHeartbeatJob
{
    public static void ScheduleJobs()
    {
        // Daily at 03:00 UTC. Reports this deployment to the author's receiver (master DB settings).
        RecurringJob.AddOrUpdate<IProductLicenseHeartbeatService>(
            "product-license-heartbeat",
            x => x.SendHeartbeatAsync(default),
            Cron.Daily(3));
    }
}
