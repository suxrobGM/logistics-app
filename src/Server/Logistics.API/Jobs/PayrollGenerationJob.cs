using Hangfire;
using Logistics.Application.Tenant.Services;

namespace Logistics.API.Jobs;

public static class PayrollGenerationJob
{
    public static void ScheduleJobs()
    {
        // Schedule GenerateMonthlyPayrollsAsync to run at the start of each month
        RecurringJob.AddOrUpdate<IPayrollService>("GenerateMonthlyPayrolls", 
            x => x.GenerateMonthlyPayrollsAsync(), 
            Cron.Monthly());

        // Schedule GenerateWeeklyPayrollsAsync to run at the start of each week
        RecurringJob.AddOrUpdate<IPayrollService>("GenerateWeeklyPayrolls", 
            x => x.GenerateWeeklyPayrollsAsync(), 
            Cron.Weekly());
    }
}
