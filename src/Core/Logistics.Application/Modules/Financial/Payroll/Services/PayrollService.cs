using System.Globalization;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Financial.Payroll.Services;

internal class PayrollService(
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
            [SalaryType.Monthly, SalaryType.ShareOfGross]);
    }

    public Task GenerateWeeklyPayrollsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var start = today.AddDays(-((int)today.DayOfWeek + 7));
        return GenerateForEveryTenantAsync("weekly", start, start.AddDays(6),
            [SalaryType.Weekly, SalaryType.PerLoad, SalaryType.PerStop]);
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

    public async Task ReleaseDeductionsAsync(PayrollInvoice payroll, CancellationToken ct = default)
    {
        var advances = await tenantUow.Repository<CashAdvance>()
            .GetListAsync(a => a.PayrollInvoiceId == payroll.Id, ct);
        foreach (var advance in advances)
        {
            advance.PayrollInvoiceId = null;
        }

        var fuel = await tenantUow.Repository<FuelCardTransaction>()
            .GetListAsync(t => t.PayrollInvoiceId == payroll.Id, ct);
        foreach (var transaction in fuel)
        {
            transaction.PayrollInvoiceId = null;
        }
    }

    public async Task<decimal> GetEscrowBalanceAsync(
        Guid employeeId, Guid? exceptPayrollId = null, CancellationToken ct = default)
    {
        var held = await tenantUow.Repository<PayrollInvoice>().Query()
            .Where(p => p.EmployeeId == employeeId &&
                        p.Id != exceptPayrollId &&
                        p.Status != InvoiceStatus.Rejected &&
                        p.Status != InvoiceStatus.Cancelled)
            .SelectMany(p => p.LineItems)
            .Where(li => li.Type == InvoiceLineItemType.Escrow)
            .SumAsync(li => li.Amount.Amount * li.Quantity, ct);

        // Contributions are stored as negative lines.
        return -held;
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
        string cadence, DateOnly startDate, DateOnly endDate, List<SalaryType> salaryTypes)
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

        // Rows this payroll already claimed still carry its id in the database during a recalculation:
        // ReleaseDeductionsAsync clears it only in memory.
        var advances = await tenantUow.Repository<CashAdvance>().GetListAsync(a =>
            a.EmployeeId == employee.Id &&
            (a.PayrollInvoiceId == null || a.PayrollInvoiceId == payroll.Id), ct);

        var fuel = employee.DeductFuelCardCharges
            ? await GetFuelChargesAsync(employee.Id, payroll, ct)
            : [];

        return new SettlementInputs(gross.Amount, gross.Description, deductions, escrowBalance, advances, fuel);
    }

    /// <summary>
    /// Matched fuel card purchases in the period on trucks the employee is the main driver of. The
    /// co-driver is never charged, so a team truck's fuel is not deducted twice.
    /// </summary>
    private async Task<List<FuelCardTransaction>> GetFuelChargesAsync(
        Guid employeeId, PayrollInvoice payroll, CancellationToken ct)
    {
        var (from, to) = payroll.PeriodUtcRange();
        var truckIds = await tenantUow.Repository<Truck>().Query()
            .Where(t => t.MainDriverId == employeeId)
            .Select(t => t.Id)
            .ToListAsync(ct);

        return await tenantUow.Repository<FuelCardTransaction>().GetListAsync(t =>
            t.TruckId != null && truckIds.Contains(t.TruckId.Value) &&
            t.Status == FuelCardTransactionStatus.Matched &&
            (t.PayrollInvoiceId == null || t.PayrollInvoiceId == payroll.Id) &&
            t.TransactionDate >= from && t.TransactionDate < to, ct);
    }

    /// <summary>Every owed advance and fuel charge in the inputs becomes a settlement line, so all are claimed.</summary>
    private static void ClaimDeductions(PayrollInvoice payroll, SettlementInputs inputs)
    {
        foreach (var advance in inputs.CashAdvances)
        {
            advance.PayrollInvoiceId = payroll.Id;
        }

        foreach (var fuel in inputs.FuelCharges)
        {
            fuel.PayrollInvoiceId = payroll.Id;
        }
    }

    private async Task<GrossPay> CalculateGrossPayAsync(PayrollInvoice payroll, CancellationToken ct)
    {
        var employee = payroll.Employee;
        var rate = employee.Salary.Amount;
        var (from, to) = payroll.PeriodUtcRange();

        switch (employee.SalaryType)
        {
            case SalaryType.ShareOfGross:
            {
                var loads = await GetDeliveredLoadsAsync(employee.Id, from, to, ct);
                var revenue = loads.Sum(l => employee.PayableRevenue(l.Linehaul, l.FuelSurcharge, l.Accessorials));
                return new GrossPay(revenue * rate,
                    $"{Percent(rate)} of {employee.PayableRevenueBasis()} on {loads.Count} load(s)");
            }
            case SalaryType.Weekly:
                var weeks = payroll.PeriodWeeks();
                return new GrossPay(weeks * rate, $"Weekly salary x {weeks}");
            case SalaryType.Monthly:
                var months = payroll.PeriodMonths();
                return new GrossPay(months * rate, $"Monthly salary x {months}");
            case SalaryType.RatePerDistance:
            {
                // Load distances are meters; the rate is per the tenant's distance unit.
                var loads = await GetDeliveredLoadsAsync(employee.Id, from, to, ct);
                var meters = loads.Sum(l => l.Distance);
                var unit = tenantUow.GetCurrentTenant().Settings.DistanceUnit;
                var distance = unit.FromMeters(meters);
                return new GrossPay((decimal)distance * rate,
                    $"{loads.Count} load(s), {distance:0} {unit.Abbreviation()} x {rate:0.00}",
                    Distance: meters);
            }
            case SalaryType.Hourly:
            {
                var hours = await tenantUow.Repository<TimeEntry>().Query()
                    .Where(te => te.EmployeeId == employee.Id && te.Date >= from && te.Date < to)
                    .SumAsync(te => te.TotalHours, ct);
                return new GrossPay(hours * rate, $"{hours:0.##} hour(s) x {rate:0.00}", Hours: hours);
            }
            case SalaryType.PerLoad:
            {
                var loads = await GetDeliveredLoadsAsync(employee.Id, from, to, ct);
                return new GrossPay(loads.Count * rate, $"{loads.Count} load(s) x {rate:0.00}");
            }
            case SalaryType.PerStop:
            {
                var stops = await CountDepartedStopsAsync(employee.Id, from, to, ct);
                return new GrossPay(stops * rate, $"{stops} stop(s) x {rate:0.00}");
            }
            default:
                return new GrossPay(rate, "Base pay");
        }
    }

    /// <summary>
    /// Loads delivered in the period on a trip the employee drove, main or secondary. A load on no trip has
    /// no recorded driver, so it goes to whoever drives its truck now.
    /// </summary>
    private Task<List<DeliveredLoad>> GetDeliveredLoadsAsync(Guid employeeId, DateTime from, DateTime to, CancellationToken ct) =>
        tenantUow.Repository<Load>().Query()
            .Where(l => l.DeliveredAt >= from && l.DeliveredAt < to &&
                        (l.TripStops.Any()
                            ? l.TripStops.Any(s => s.Trip.DriverId == employeeId || s.Trip.SecondaryDriverId == employeeId)
                            : l.AssignedTruck != null &&
                              (l.AssignedTruck.MainDriverId == employeeId || l.AssignedTruck.SecondaryDriverId == employeeId)))
            .Select(l => new DeliveredLoad(
                l.DeliveryCost.Amount,
                l.FuelSurcharge,
                l.Accessorials.Sum(a => a.Rate * a.Quantity),
                l.Distance))
            .ToListAsync(ct);

    /// <summary>
    /// Stops the driver app marked departed in the period, on trips the employee drove.
    /// </summary>
    private Task<int> CountDepartedStopsAsync(Guid employeeId, DateTime from, DateTime to, CancellationToken ct) =>
        tenantUow.Repository<TripStop>().Query()
            .CountAsync(s => s.DepartedAt >= from && s.DepartedAt < to &&
                             (s.Trip.DriverId == employeeId || s.Trip.SecondaryDriverId == employeeId), ct);

    private static string Percent(decimal ratio) => (ratio * 100).ToString("0.##", CultureInfo.InvariantCulture) + "%";

    private sealed record GrossPay(decimal Amount, string Description, double Distance = 0, decimal Hours = 0);

    private sealed record DeliveredLoad(decimal Linehaul, decimal FuelSurcharge, decimal Accessorials, double Distance);
}
