using Hangfire;
using Logistics.API.Jobs.Scheduling;
using Logistics.Application.Abstractions.Context;
using Logistics.Application.Modules.Financial.Payroll.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.API.Jobs;

/// <summary>Drafts last period's settlements for every Payroll tenant at the start of each month and week.</summary>
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
            (scope, tenant, token) => GenerateIfEnabledAsync(scope, tenant, (s, t) => s.GenerateMonthlyPayrollsAsync(t), token),
            ct);

    public Task GenerateWeeklyAsync(CancellationToken ct) =>
        TenantJobRunner.ForEachTenantAsync(scopeFactory, logger, "weekly payroll generation",
            (scope, tenant, token) => GenerateIfEnabledAsync(scope, tenant, (s, t) => s.GenerateWeeklyPayrollsAsync(t), token),
            ct);

    private static async Task GenerateIfEnabledAsync(
        IServiceScope scope,
        Tenant tenant,
        Func<IPayrollService, CancellationToken, Task> generate,
        CancellationToken ct)
    {
        var featureService = scope.ServiceProvider.GetRequiredService<IFeatureService>();
        if (!await featureService.IsFeatureEnabledAsync(tenant.Id, TenantFeature.Payroll))
        {
            return;
        }

        await generate(scope.ServiceProvider.GetRequiredService<IPayrollService>(), ct);
    }
}
