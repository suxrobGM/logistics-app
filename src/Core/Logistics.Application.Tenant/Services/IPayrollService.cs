namespace Logistics.Application.Tenant.Services;

public interface IPayrollService
{
    Task GenerateMonthlyPayrollsAsync();
    Task GenerateWeeklyPayrollsAsync();
    Payroll CreatePayroll(Employee employee, DateTime startDate, DateTime endDate);
}
