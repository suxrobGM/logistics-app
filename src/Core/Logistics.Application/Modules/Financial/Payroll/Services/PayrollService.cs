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
    public async Task GenerateMonthlyPayrollsAsync()
    {
        var tenants = await masterUow.Repository<Tenant>().GetListAsync();

        foreach (var tenant in tenants)
        {
            tenantUow.SetCurrentTenant(tenant);

            var previousMonthStart =
                new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1);
            var previousMonthEnd = previousMonthStart.AddMonths(1).AddDays(-1);
            var employees = await tenantUow.Repository<Employee>().GetListAsync(e =>
                e.SalaryType == SalaryType.Monthly ||
                e.SalaryType == SalaryType.ShareOfGross);

            await GenerateAsync(employees, previousMonthStart, previousMonthEnd);

            logger.LogInformation(
                "Generated monthly payrolls for the tenant: {TenantName}, date range: {StartDate} - {EndDate}",
                tenant.Name, previousMonthStart.ToShortDateString(), previousMonthEnd.ToShortDateString());
            await tenantUow.SaveChangesAsync();
        }
    }

    public async Task GenerateWeeklyPayrollsAsync()
    {
        var tenants = await masterUow.Repository<Tenant>().GetListAsync();

        foreach (var tenant in tenants)
        {
            tenantUow.SetCurrentTenant(tenant);
            var previousWeekStart = StartOfPreviousWeek(DateTime.UtcNow);
            var previousWeekEnd = previousWeekStart.AddDays(6);
            var employees = await tenantUow.Repository<Employee>().GetListAsync(e =>
                e.SalaryType == SalaryType.Weekly ||
                e.SalaryType == SalaryType.PerLoad ||
                e.SalaryType == SalaryType.PerStop);

            await GenerateAsync(employees, previousWeekStart, previousWeekEnd);

            logger.LogInformation(
                "Generated weekly payrolls for the tenant: {TenantName}, date range: {StartDate} - {EndDate}",
                tenant.Name, previousWeekStart.ToShortDateString(), previousWeekEnd.ToShortDateString());
            await tenantUow.SaveChangesAsync();
        }
    }

    public async Task<PayrollInvoice> BuildPayrollInvoiceAsync(
        Employee employee, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var tenant = tenantUow.GetCurrentTenant();
        var currency = tenant.Settings.Currency.ToString();

        var payroll = new PayrollInvoice
        {
            Subtotal = Money.Zero(currency),
            TaxTotal = Money.Zero(currency),
            Total = Money.Zero(currency),
            Status = InvoiceStatus.Draft,
            PeriodStart = startDate,
            PeriodEnd = endDate,
            EmployeeId = employee.Id,
            Employee = employee
        };

        var inputs = await BuildInputsAsync(payroll, ct);
        payroll.ReplaceSettlementLines(inputs);
        return payroll;
    }

    public async Task<PayrollInvoice> CreatePayrollInvoiceAsync(
        Employee employee, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var payroll = await BuildPayrollInvoiceAsync(employee, startDate, endDate, ct);
        await ClaimDeductionsAsync(payroll, ct);
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

        await ClaimDeductionsAsync(payroll, ct);

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

    public async Task<int> LinkTimeEntriesToPayrollAsync(PayrollInvoice payroll)
    {
        if (payroll.Employee?.SalaryType != SalaryType.Hourly)
        {
            return 0;
        }

        var timeEntries = await tenantUow.Repository<TimeEntry>()
            .GetListAsync(te =>
                te.EmployeeId == payroll.EmployeeId &&
                te.PayrollInvoiceId == null &&
                te.Date >= payroll.PeriodStart.Date &&
                te.Date <= payroll.PeriodEnd.Date);

        foreach (var entry in timeEntries)
        {
            entry.PayrollInvoiceId = payroll.Id;
            tenantUow.Repository<TimeEntry>().Update(entry);
        }

        logger.LogInformation(
            "Linked {Count} time entries to payroll {PayrollId}",
            timeEntries.Count, payroll.Id);

        return timeEntries.Count;
    }

    public bool ValidateTimeEntryDate(DateTime entryDate, DateTime periodStart, DateTime periodEnd)
    {
        return entryDate.Date >= periodStart.Date && entryDate.Date <= periodEnd.Date;
    }

    private async Task GenerateAsync(List<Employee> employees, DateTime startDate, DateTime endDate)
    {
        foreach (var employee in employees)
        {
            if (await IsPayrollInvoiceExisting(employee.Id, startDate, endDate))
            {
                continue;
            }

            var payroll = await CreatePayrollInvoiceAsync(employee, startDate, endDate);
            await tenantUow.Repository<PayrollInvoice>().AddAsync(payroll);
        }
    }

    private async Task<bool> IsPayrollInvoiceExisting(Guid employeeId, DateTime startDate, DateTime endDate)
    {
        var payroll = await tenantUow.Repository<PayrollInvoice>().GetAsync(p =>
            p.EmployeeId == employeeId &&
            p.PeriodStart >= startDate &&
            p.PeriodEnd <= endDate);

        return payroll != null;
    }

    private async Task<SettlementInputs> BuildInputsAsync(PayrollInvoice payroll, CancellationToken ct)
    {
        var employee = payroll.Employee;
        var gross = await CalculateGrossPayAsync(employee, payroll.PeriodStart, payroll.PeriodEnd, ct);
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
        var startDate = payroll.PeriodStart;
        var periodEnd = payroll.PeriodEnd.Date.AddDays(1);
        var truckIds = await tenantUow.Repository<Truck>().Query()
            .Where(t => t.MainDriverId == employeeId)
            .Select(t => t.Id)
            .ToListAsync(ct);

        return await tenantUow.Repository<FuelCardTransaction>().GetListAsync(t =>
            t.TruckId != null && truckIds.Contains(t.TruckId.Value) &&
            t.Status == FuelCardTransactionStatus.Matched &&
            (t.PayrollInvoiceId == null || t.PayrollInvoiceId == payroll.Id) &&
            t.TransactionDate >= startDate && t.TransactionDate < periodEnd, ct);
    }

    private async Task ClaimDeductionsAsync(PayrollInvoice payroll, CancellationToken ct)
    {
        var claimed = payroll.LineItems
            .Where(li => li.Type is InvoiceLineItemType.CashAdvance or InvoiceLineItemType.FuelCard)
            .Select(li => li.SourceId!.Value)
            .ToList();

        if (claimed.Count == 0)
        {
            return;
        }

        foreach (var advance in await tenantUow.Repository<CashAdvance>().GetListAsync(a => claimed.Contains(a.Id), ct))
        {
            advance.PayrollInvoiceId = payroll.Id;
        }

        foreach (var fuel in await tenantUow.Repository<FuelCardTransaction>().GetListAsync(t => claimed.Contains(t.Id), ct))
        {
            fuel.PayrollInvoiceId = payroll.Id;
        }
    }

    private async Task<GrossPay> CalculateGrossPayAsync(
        Employee employee, DateTime startDate, DateTime endDate, CancellationToken ct)
    {
        var rate = employee.Salary.Amount;

        switch (employee.SalaryType)
        {
            case SalaryType.ShareOfGross:
            {
                var loads = await GetDeliveredLoadsAsync(employee.Id, startDate, endDate, ct);
                var revenue = loads.Sum(l => l.PayableRevenue(employee.PayExcludesFuelSurcharge, employee.PayExcludesAccessorials));
                var basis = (employee.PayExcludesFuelSurcharge, employee.PayExcludesAccessorials) switch
                {
                    (true, true) => "linehaul",
                    (true, false) => "linehaul and accessorials",
                    (false, true) => "linehaul and fuel surcharge",
                    _ => "gross revenue"
                };
                return new GrossPay(revenue * rate, $"{Percent(rate)} of {basis} on {loads.Count} load(s)");
            }
            case SalaryType.Weekly:
                var weeks = CountWeeks(startDate, endDate);
                return new GrossPay(weeks * rate, $"Weekly salary x {weeks}");
            case SalaryType.Monthly:
                var months = CountMonths(startDate, endDate);
                return new GrossPay(months * rate, $"Monthly salary x {months}");
            case SalaryType.RatePerDistance:
            {
                // Load distances are meters; the rate is per the tenant's distance unit.
                var loads = await GetDeliveredLoadsAsync(employee.Id, startDate, endDate, ct);
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
                    .Where(te => te.EmployeeId == employee.Id &&
                                 te.Date >= startDate.Date &&
                                 te.Date <= endDate.Date)
                    .SumAsync(te => te.TotalHours, ct);
                return new GrossPay(hours * rate, $"{hours:0.##} hour(s) x {rate:0.00}", Hours: hours);
            }
            case SalaryType.PerLoad:
            {
                var loads = await GetDeliveredLoadsAsync(employee.Id, startDate, endDate, ct);
                return new GrossPay(loads.Count * rate, $"{loads.Count} load(s) x {rate:0.00}");
            }
            case SalaryType.PerStop:
            {
                var stops = await CountDepartedStopsAsync(employee.Id, startDate, endDate, ct);
                return new GrossPay(stops * rate, $"{stops} stop(s) x {rate:0.00}");
            }
            default:
                return new GrossPay(rate, "Base pay");
        }
    }

    /// <summary>
    /// Loads delivered in the period by any truck the employee drives, main or secondary.
    /// </summary>
    private Task<List<DeliveredLoad>> GetDeliveredLoadsAsync(
        Guid employeeId, DateTime startDate, DateTime endDate, CancellationToken ct)
    {
        var periodEnd = endDate.Date.AddDays(1);
        return tenantUow.Repository<Load>().Query()
            .Where(l => l.DeliveredAt >= startDate && l.DeliveredAt < periodEnd &&
                        l.AssignedTruck != null &&
                        (l.AssignedTruck.MainDriverId == employeeId || l.AssignedTruck.SecondaryDriverId == employeeId))
            .Select(l => new DeliveredLoad(
                l.DeliveryCost.Amount,
                l.FuelSurcharge,
                l.Accessorials.Sum(a => a.Rate * a.Quantity),
                l.Distance))
            .ToListAsync(ct);
    }

    /// <summary>
    /// Stops the driver app marked departed in the period, on trips run by a truck the employee drives.
    /// </summary>
    private Task<int> CountDepartedStopsAsync(Guid employeeId, DateTime startDate, DateTime endDate, CancellationToken ct)
    {
        var periodEnd = endDate.Date.AddDays(1);
        return tenantUow.Repository<TripStop>().Query()
            .CountAsync(s => s.DepartedAt >= startDate && s.DepartedAt < periodEnd &&
                             s.Trip.Truck != null &&
                             (s.Trip.Truck.MainDriverId == employeeId || s.Trip.Truck.SecondaryDriverId == employeeId), ct);
    }

    private static string Percent(decimal ratio) => (ratio * 100).ToString("0.##", CultureInfo.InvariantCulture) + "%";

    private static int CountWeeks(DateTime startDate, DateTime endDate)
    {
        // Assuming a week starts on Sunday and ends on Saturday.
        var days = (endDate - startDate).Days + 1; // +1 to include the start day in the count
        var fullWeeks = days / 7;
        var remainingDays = days % 7;

        // Check if the remaining days form a week when combined with the start and end dates.
        if (remainingDays > 0)
        {
            var startDay = startDate.DayOfWeek;
            var endDay = endDate.AddDays(-remainingDays).DayOfWeek;

            if (endDay >= startDay)
            {
                fullWeeks++;
            }
        }

        return fullWeeks;
    }

    private static int CountMonths(DateTime startDate, DateTime endDate)
    {
        var months = (endDate.Year - startDate.Year) * 12 + endDate.Month - startDate.Month;

        // If endDate is in a month but before the start date day, then reduce a month.
        if (endDate.Day < startDate.Day)
        {
            months--;
        }

        return months + 1; // +1 to include the starting month
    }

    private static DateTime StartOfPreviousWeek(DateTime date)
    {
        var daysToSubtract = (int)date.DayOfWeek + 7;
        return date.AddDays(-daysToSubtract).Date;
    }

    private sealed record GrossPay(decimal Amount, string Description, double Distance = 0, decimal Hours = 0);

    private sealed record DeliveredLoad(decimal Linehaul, decimal FuelSurcharge, decimal Accessorials, double Distance)
    {
        public decimal PayableRevenue(bool excludeFuelSurcharge, bool excludeAccessorials) =>
            Linehaul + (excludeFuelSurcharge ? 0 : FuelSurcharge) + (excludeAccessorials ? 0 : Accessorials);
    }
}
