using Logistics.Domain.Entities;

namespace Logistics.Application.Services;

public interface IPayrollService
{
    Task GenerateMonthlyPayrollsAsync();
    Task GenerateWeeklyPayrollsAsync();
    
    /// <summary>
    /// Creates a payroll invoice for the given employee and date range.
    /// </summary>
    /// <param name="employee">Employee for whom the payroll is created.</param>
    /// <param name="startDate">Period start date.</param>
    /// <param name="endDate">>Period end date.</param>
    /// <returns>Payroll invoice.</returns>
    PayrollInvoice CreatePayroll(Employee employee, DateTime startDate, DateTime endDate);
}
