using Hangfire;
using Logistics.Application.Modules.Financial.Payroll.Services;

namespace Logistics.API.Jobs;

/// <summary>Drafts last period's settlements for every tenant at the start of each month and week.</summary>
public class PayrollGenerationJob(
    ILogger<PayrollGenerationJob> logger,
    IServiceScopeFactory scopeFactory)
{
    public static void ScheduleJobs()
    {
        RecurringJob.AddOrUpdate<PayrollGenerationJob>("GenerateMonthlyPayrolls",
            job => job.GenerateMonthlyAsync(CancellationToken.None),
            Cron.Monthly());

        RecurringJob.AddOrUpdate<PayrollGenerationJob>("GenerateWeeklyPayrolls",
            job => job.GenerateWeeklyAsync(CancellationToken.None),
            Cron.Weekly());
    }

    public Task GenerateMonthlyAsync(CancellationToken ct) =>
        TenantJobRunner.ForEachTenantAsync(scopeFactory, logger, "monthly payroll generation",
            (scope, _, token) => scope.ServiceProvider.GetRequiredService<IPayrollService>().GenerateMonthlyPayrollsAsync(token),
            ct);

    public Task GenerateWeeklyAsync(CancellationToken ct) =>
        TenantJobRunner.ForEachTenantAsync(scopeFactory, logger, "weekly payroll generation",
            (scope, _, token) => scope.ServiceProvider.GetRequiredService<IPayrollService>().GenerateWeeklyPayrollsAsync(token),
            ct);
}
