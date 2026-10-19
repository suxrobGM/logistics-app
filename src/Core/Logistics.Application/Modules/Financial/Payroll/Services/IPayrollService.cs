using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.Financial.Payroll.Services;

public interface IPayrollService : IApplicationService
{
    Task GenerateMonthlyPayrollsAsync();
    Task GenerateWeeklyPayrollsAsync();

    /// <summary>
    /// Builds a settlement for the period - gross pay, recurring deductions, escrow, owed cash
    /// advances and fuel card charges - without claiming the advances or fuel charges. For previews.
    /// </summary>
    Task<PayrollInvoice> BuildPayrollInvoiceAsync(Employee employee, DateOnly startDate, DateOnly endDate,
        CancellationToken ct = default);

    /// <summary>
    /// Builds a settlement like <see cref="BuildPayrollInvoiceAsync"/> and marks its cash advances and
    /// fuel card charges as deducted by it. The caller adds the invoice and saves.
    /// </summary>
    Task<PayrollInvoice> CreatePayrollInvoiceAsync(Employee employee, DateOnly startDate, DateOnly endDate,
        CancellationToken ct = default);

    /// <summary>
    /// Rebuilds a tracked settlement's generated lines after its period or employee changed. Hand-added
    /// lines are kept. The caller saves.
    /// </summary>
    Task RecalculatePayrollAsync(PayrollInvoice payroll, CancellationToken ct = default);

    /// <summary>
    /// Hands the settlement's cash advances and fuel card charges back, so the next settlement deducts
    /// them. Used when a settlement is rejected. The caller saves.
    /// </summary>
    Task ReleaseDeductionsAsync(PayrollInvoice payroll, CancellationToken ct = default);

    /// <summary>
    /// Escrow held for the employee: contributions minus releases on every settlement that was not
    /// rejected or cancelled, optionally ignoring one settlement (the one being rebuilt).
    /// </summary>
    Task<decimal> GetEscrowBalanceAsync(Guid employeeId, Guid? exceptPayrollId = null, CancellationToken ct = default);

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
    bool ValidateTimeEntryDate(DateTime entryDate, DateOnly periodStart, DateOnly periodEnd);
}
