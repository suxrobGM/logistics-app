using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Financial.Payroll.Services;

public interface IPayrollService : IApplicationService
{
    /// <summary>Salary types <see cref="GenerateMonthlyPayrollsAsync"/> pays.</summary>
    static readonly SalaryType[] MonthlySalaryTypes = [SalaryType.Monthly, SalaryType.ShareOfGross];

    /// <summary>Salary types <see cref="GenerateWeeklyPayrollsAsync"/> pays.</summary>
    static readonly SalaryType[] WeeklySalaryTypes = [SalaryType.Weekly, SalaryType.PerLoad, SalaryType.PerStop];

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
    /// Links an hourly employee's unlinked time entries in the period to a saved settlement. A no-op for
    /// other salary types. The caller saves.
    /// </summary>
    Task LinkTimeEntriesToPayrollAsync(PayrollInvoice payroll, CancellationToken ct = default);
}
