using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Services;

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

            foreach (var employee in employees)
            {
                var isPayrollExisting =
                    await IsPayrollInvoiceExisting(employee.Id, previousMonthStart, previousMonthEnd);
                if (isPayrollExisting)
                {
                    continue;
                }

                var payroll = CreatePayrollInvoice(employee, previousMonthStart, previousMonthEnd);
                await tenantUow.Repository<PayrollInvoice>().AddAsync(payroll);
            }

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
            var employees =
                await tenantUow.Repository<Employee>().GetListAsync(e => e.SalaryType == SalaryType.Weekly);

            foreach (var employee in employees)
            {
                var isPayrollExisting = await IsPayrollInvoiceExisting(employee.Id, previousWeekStart, previousWeekEnd);
                if (isPayrollExisting)
                {
                    continue;
                }

                var payroll = CreatePayrollInvoice(employee, previousWeekStart, previousWeekEnd);
                await tenantUow.Repository<PayrollInvoice>().AddAsync(payroll);
            }

            logger.LogInformation(
                "Generated weekly payrolls for the tenant: {TenantName}, date range: {StartDate} - {EndDate}",
                tenant.Name, previousWeekStart.ToShortDateString(), previousWeekEnd.ToShortDateString());
            await tenantUow.SaveChangesAsync();
        }
    }

    public PayrollInvoice CreatePayrollInvoice(Employee employee, DateTime startDate, DateTime endDate)
    {
        var (invoiceAmount, totalDistance, totalHours) = CalculateSalaryWithDetails(employee, startDate, endDate);

        var payrollInvoice = new PayrollInvoice
        {
            Total = invoiceAmount,
            Status = InvoiceStatus.Draft,
            PeriodStart = startDate,
            PeriodEnd = endDate,
            EmployeeId = employee.Id,
            Employee = employee,
            TotalDistanceDriven = totalDistance,
            TotalHoursWorked = totalHours
        };
        return payrollInvoice;
    }

    public void RecalculatePayroll(PayrollInvoice payroll)
    {
        var (invoiceAmount, totalDistance, totalHours) = CalculateSalaryWithDetails(
            payroll.Employee,
            payroll.PeriodStart,
            payroll.PeriodEnd);

        payroll.Total = invoiceAmount;
        payroll.TotalDistanceDriven = totalDistance;
        payroll.TotalHoursWorked = totalHours;

        logger.LogInformation(
            "Recalculated payroll {PayrollId} for employee {EmployeeId}: Amount={Amount}, Distance={Distance}, Hours={Hours}",
            payroll.Id, payroll.EmployeeId, invoiceAmount, totalDistance, totalHours);
    }

    public async Task<int> LinkTimeEntriesToPayrollAsync(PayrollInvoice payroll)
    {
        // Only link for hourly employees
        if (payroll.Employee?.SalaryType != SalaryType.Hourly)
        {
            return 0;
        }

        // Find unlinked time entries within the payroll period
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

    private async Task<bool> IsPayrollInvoiceExisting(Guid employeeId, DateTime startDate, DateTime endDate)
    {
        var payroll = await tenantUow.Repository<PayrollInvoice>().GetAsync(p =>
            p.EmployeeId == employeeId &&
            p.PeriodStart >= startDate &&
            p.PeriodEnd <= endDate);

        return payroll != null;
    }

    /// <summary>
    /// Calculates salary with additional tracking data (distance, hours).
    /// Returns (salary amount, total distance in km, total hours worked).
    /// </summary>
    private (decimal Amount, double TotalDistance, decimal TotalHours) CalculateSalaryWithDetails(
        Employee employee,
        DateTime startDate,
        DateTime endDate)
    {
        return employee.SalaryType switch
        {
            SalaryType.ShareOfGross => (CalculateShareOfGross(employee, startDate, endDate), 0, 0),
            SalaryType.Weekly => (CountWeeks(startDate, endDate) * employee.Salary, 0, 0),
            SalaryType.Monthly => (CountMonths(startDate, endDate) * employee.Salary, 0, 0),
            SalaryType.RatePerDistance => CalculateRatePerDistance(employee, startDate, endDate),
            SalaryType.Hourly => CalculateHourly(employee, startDate, endDate),
            _ => (employee.Salary, 0, 0)
        };
    }

    /// <summary>
    /// Share-of-gross: sum every load that was delivered in the period
    /// by ANY truck where this employee was the main OR secondary driver.
    /// </summary>
    private decimal CalculateShareOfGross(Employee employee, DateTime startDate, DateTime endDate)
    {
        var totalGross = tenantUow.Repository<Truck>()
            .Query()
            .Where(t => t.MainDriverId == employee.Id ||
                        t.SecondaryDriverId == employee.Id)
            .SelectMany(t => t.Loads.Where(l =>
                l.DeliveredAt.HasValue &&
                l.DeliveredAt.Value >= startDate &&
                l.DeliveredAt.Value <= endDate))
            .Sum(l => l.DeliveryCost.Amount);

        return totalGross * employee.Salary; // Salary holds the share ratio (0-1)
    }

    /// <summary>
    /// Rate per distance: sum all load distances for delivered loads where driver was assigned.
    /// Distance is stored in kilometers. Multiply by rate stored in Employee.Salary.
    /// </summary>
    private (decimal Amount, double TotalDistance, decimal TotalHours) CalculateRatePerDistance(
        Employee employee,
        DateTime startDate,
        DateTime endDate)
    {
        var totalDistanceKm = tenantUow.Repository<Truck>()
            .Query()
            .Where(t => t.MainDriverId == employee.Id ||
                        t.SecondaryDriverId == employee.Id)
            .SelectMany(t => t.Loads.Where(l =>
                l.DeliveredAt.HasValue &&
                l.DeliveredAt.Value >= startDate &&
                l.DeliveredAt.Value <= endDate))
            .Sum(l => l.Distance);

        // Employee.Salary holds rate per km (or rate per mile, depending on tenant settings)
        var amount = (decimal)totalDistanceKm * employee.Salary;

        return (amount, totalDistanceKm, 0);
    }

    /// <summary>
    /// Hourly: sum all time entries for the employee in the period.
    /// Multiply total hours by rate stored in Employee.Salary.
    /// </summary>
    private (decimal Amount, double TotalDistance, decimal TotalHours) CalculateHourly(
        Employee employee,
        DateTime startDate,
        DateTime endDate)
    {
        var totalHours = tenantUow.Repository<TimeEntry>()
            .Query()
            .Where(te => te.EmployeeId == employee.Id &&
                         te.Date >= startDate.Date &&
                         te.Date <= endDate.Date)
            .Sum(te => te.TotalHours);

        var amount = totalHours * employee.Salary;

        return (amount, 0, totalHours);
    }

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
}
