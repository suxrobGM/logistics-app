namespace Logistics.Application.Tenant.Services;

public interface IPayrollService
{
    Task GenerateMonthlyPayrollsAsync();
    Task GenerateWeeklyPayrollsAsync();
}
