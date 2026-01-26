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
    /// <param name="endDate">Period end date.</param>
    /// <returns>Payroll invoice.</returns>
    PayrollInvoice CreatePayrollInvoice(Employee employee, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Recalculates a payroll invoice based on updated period dates.
    /// Updates Total, TotalDistanceDriven, and TotalHoursWorked.
    /// </summary>
    /// <param name="payroll">The payroll invoice to recalculate.</param>
    void RecalculatePayroll(PayrollInvoice payroll);

    /// <summary>
    /// Links time entries to a payroll invoice.
    /// Validates that time entry dates fall within the payroll period.
    /// </summary>
    /// <param name="payroll">The payroll invoice.</param>
    /// <returns>Number of time entries linked.</returns>
    Task<int> LinkTimeEntriesToPayrollAsync(PayrollInvoice payroll);

    /// <summary>
    /// Validates that a time entry date falls within a payroll period.
    /// </summary>
    /// <param name="entryDate">The time entry date.</param>
    /// <param name="periodStart">Payroll period start.</param>
    /// <param name="periodEnd">Payroll period end.</param>
    /// <returns>True if valid, false otherwise.</returns>
    bool ValidateTimeEntryDate(DateTime entryDate, DateTime periodStart, DateTime periodEnd);
}
