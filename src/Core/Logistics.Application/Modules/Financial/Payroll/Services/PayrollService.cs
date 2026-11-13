using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Financial.Payroll.Services;

internal partial class PayrollService(
    ITenantUnitOfWork tenantUow,
    IDriverPayCalculator payCalculator,
    ILogger<PayrollService> logger)
    : IPayrollService
{
    public Task GenerateMonthlyPayrollsAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var start = new DateOnly(today.Year, today.Month, 1).AddMonths(-1);
        return GenerateAsync("monthly", start, start.AddMonths(1).AddDays(-1),
            IPayrollService.MonthlySalaryTypes, ct);
    }

    public Task GenerateWeeklyPayrollsAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var start = today.AddDays(-((int)today.DayOfWeek + 7));
        return GenerateAsync("weekly", start, start.AddDays(6),
            IPayrollService.WeeklySalaryTypes, ct);
    }

    public async Task<PayrollInvoice> BuildPayrollInvoiceAsync(
        Employee employee, DateOnly startDate, DateOnly endDate, CancellationToken ct = default)
    {
        var payroll = NewDraft(employee, startDate, endDate);
        payroll.ReplaceSettlementLines(await BuildInputsAsync(payroll, ct));
        return payroll;
    }

    public async Task<PayrollInvoice> CreatePayrollInvoiceAsync(
        Employee employee, DateOnly startDate, DateOnly endDate, CancellationToken ct = default)
    {
        var payroll = NewDraft(employee, startDate, endDate);
        var inputs = await BuildInputsAsync(payroll, ct);
        payroll.ReplaceSettlementLines(inputs);
        ClaimDeductions(payroll, inputs);
        return payroll;
    }

    public async Task RecalculatePayrollAsync(PayrollInvoice payroll, CancellationToken ct = default)
    {
        await ReleaseDeductionsAsync(payroll, ct);

        var inputs = await BuildInputsAsync(payroll, ct);
        var (removed, added) = payroll.ReplaceSettlementLines(inputs);

        var lineRepository = tenantUow.Repository<InvoiceLineItem>();
        foreach (var line in removed)
        {
            lineRepository.Delete(line);
        }

        await lineRepository.AddRangeAsync(added, ct);
        ClaimDeductions(payroll, inputs);

        logger.LogInformation(
            "Recalculated payroll {PayrollId} for employee {EmployeeId}: Net={Amount}, Distance={Distance}, Hours={Hours}",
            payroll.Id, payroll.EmployeeId, payroll.Total.Amount, payroll.TotalDistanceDriven, payroll.TotalHoursWorked);
    }

    public async Task LinkTimeEntriesToPayrollAsync(PayrollInvoice payroll, CancellationToken ct = default)
    {
        if (payroll.Employee.SalaryType != SalaryType.Hourly)
        {
            return;
        }

        var (from, to) = payroll.PeriodUtcRange();
        var timeEntries = await tenantUow.Repository<TimeEntry>().GetListAsync(te =>
            te.EmployeeId == payroll.EmployeeId &&
            te.PayrollInvoiceId == null &&
            te.Date >= from &&
            te.Date < to, ct);

        foreach (var entry in timeEntries)
        {
            entry.PayrollInvoiceId = payroll.Id;
        }

        logger.LogInformation("Linked {Count} time entries to payroll {PayrollId}", timeEntries.Count, payroll.Id);
    }

    private async Task GenerateAsync(
        string cadence, DateOnly startDate, DateOnly endDate, SalaryType[] salaryTypes, CancellationToken ct)
    {
        var alreadyGenerated = await tenantUow.Repository<PayrollInvoice>().Query()
            .Where(p => p.PeriodStart >= startDate && p.PeriodEnd <= endDate)
            .Select(p => p.EmployeeId)
            .ToHashSetAsync(ct);
        var employees = await tenantUow.Repository<Employee>()
            .GetListAsync(e => salaryTypes.Contains(e.SalaryType), ct);

        foreach (var employee in employees.Where(e => !alreadyGenerated.Contains(e.Id)))
        {
            var payroll = await CreatePayrollInvoiceAsync(employee, startDate, endDate, ct);
            await tenantUow.Repository<PayrollInvoice>().AddAsync(payroll, ct);
        }

        await tenantUow.SaveChangesAsync(ct);
        logger.LogInformation(
            "Generated {Cadence} payrolls for tenant {TenantName}, {StartDate} - {EndDate}",
            cadence, tenantUow.GetCurrentTenant().Name, startDate, endDate);
    }

    private PayrollInvoice NewDraft(Employee employee, DateOnly startDate, DateOnly endDate)
    {
        var currency = tenantUow.GetCurrentTenant().Settings.Currency.ToString();
        return new PayrollInvoice
        {
            Subtotal = Money.Zero(currency),
            TaxTotal = Money.Zero(currency),
            Total = Money.Zero(currency),
            PeriodStart = startDate,
            PeriodEnd = endDate,
            EmployeeId = employee.Id,
            Employee = employee
        };
    }

    private async Task<SettlementInputs> BuildInputsAsync(PayrollInvoice payroll, CancellationToken ct)
    {
        var employee = payroll.Employee;
        var gross = await CalculateGrossPayAsync(payroll, ct);
        payroll.TotalDistanceDriven = gross.Distance;
        payroll.TotalHoursWorked = gross.Hours;

        var deductions = await tenantUow.Repository<RecurringDeduction>()
            .GetListAsync(d => d.EmployeeId == employee.Id, ct);

        var escrowBalance = deductions.Any(d => d.Type == PayDeductionType.Escrow)
            ? await GetEscrowBalanceAsync(employee.Id, payroll.Id, ct)
            : 0;

        var advances = await GetOwedCashAdvancesAsync(payroll, ct);
        var fuel = employee.DeductFuelCardCharges ? await GetFuelChargesAsync(payroll, ct) : [];

        return new SettlementInputs(gross.Amount, gross.Description, deductions, escrowBalance, advances, fuel);
    }
}
