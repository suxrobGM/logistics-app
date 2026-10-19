using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Financial.Payroll.Services;

internal partial class PayrollService(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    ILogger<PayrollService> logger)
    : IPayrollService
{
    public Task GenerateMonthlyPayrollsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var start = new DateOnly(today.Year, today.Month, 1).AddMonths(-1);
        return GenerateForEveryTenantAsync("monthly", start, start.AddMonths(1).AddDays(-1),
            IPayrollService.MonthlySalaryTypes);
    }

    public Task GenerateWeeklyPayrollsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var start = today.AddDays(-((int)today.DayOfWeek + 7));
        return GenerateForEveryTenantAsync("weekly", start, start.AddDays(6),
            IPayrollService.WeeklySalaryTypes);
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

    private async Task GenerateForEveryTenantAsync(
        string cadence, DateOnly startDate, DateOnly endDate, SalaryType[] salaryTypes)
    {
        var tenants = await masterUow.Repository<Tenant>().GetListAsync();

        foreach (var tenant in tenants)
        {
            tenantUow.SetCurrentTenant(tenant);

            var alreadyGenerated = await tenantUow.Repository<PayrollInvoice>().Query()
                .Where(p => p.PeriodStart >= startDate && p.PeriodEnd <= endDate)
                .Select(p => p.EmployeeId)
                .ToHashSetAsync();
            var employees = await tenantUow.Repository<Employee>()
                .GetListAsync(e => salaryTypes.Contains(e.SalaryType));

            foreach (var employee in employees.Where(e => !alreadyGenerated.Contains(e.Id)))
            {
                var payroll = await CreatePayrollInvoiceAsync(employee, startDate, endDate);
                await tenantUow.Repository<PayrollInvoice>().AddAsync(payroll);
            }

            logger.LogInformation(
                "Generated {Cadence} payrolls for the tenant: {TenantName}, date range: {StartDate} - {EndDate}",
                cadence, tenant.Name, startDate.ToShortDateString(), endDate.ToShortDateString());
            await tenantUow.SaveChangesAsync();
        }
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
