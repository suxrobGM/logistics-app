using Hangfire;
using Logistics.API.Jobs.Scheduling;
using Logistics.Application.Modules.Compliance.Safety.Services;

namespace Logistics.API.Jobs;

/// <summary>Reminds each tenant about driver licenses approaching expiry.</summary>
public class LicenseExpiryReminderJob(
    ILogger<LicenseExpiryReminderJob> logger,
    IServiceScopeFactory scopeFactory)
{
    public static void ScheduleJobs()
    {
        RecurringJob.AddOrUpdate<LicenseExpiryReminderJob>(
            "ProcessLicenseExpiryReminders",
            job => job.RemindAllTenantsAsync(CancellationToken.None),
            Cron.Daily(6));
    }

    public Task RemindAllTenantsAsync(CancellationToken ct) =>
        TenantJobRunner.ForEachTenantAsync(scopeFactory, logger, "license expiry reminders",
            (scope, tenant, token) => scope.ServiceProvider.GetRequiredService<ILicenseExpiryReminderService>()
                .ProcessLicenseExpiryRemindersAsync(tenant, token),
            ct);
}
