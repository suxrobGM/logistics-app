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
        var invoiceAmount = CalculateSalary(employee, startDate, endDate);

        var payrollInvoice = new PayrollInvoice
        {
            Total = invoiceAmount,
            Status = InvoiceStatus.Issued,
            PeriodStart = startDate,
            PeriodEnd = endDate,
            EmployeeId = employee.Id,
            Employee = employee
        };
        return payrollInvoice;
    }

    private async Task<bool> IsPayrollInvoiceExisting(Guid employeeId, DateTime startDate, DateTime endDate)
    {
        var payroll = await tenantUow.Repository<PayrollInvoice>().GetAsync(p =>
            p.EmployeeId == employeeId &&
            p.PeriodStart >= startDate &&
            p.PeriodEnd <= endDate);

        return payroll != null;
    }

    private decimal CalculateSalary(Employee employee, DateTime startDate, DateTime endDate)
    {
        // Share-of-gross: sum every load that was delivered in the period
        // by ANY truck where this employee was the main OR secondary driver.
        if (employee.SalaryType == SalaryType.ShareOfGross)
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

        // Weekly
        if (employee.SalaryType == SalaryType.Weekly)
        {
            return CountWeeks(startDate, endDate) * employee.Salary;
        }

        // Monthly
        if (employee.SalaryType == SalaryType.Monthly)
        {
            return CountMonths(startDate, endDate) * employee.Salary;
        }

        // Fallback – fixed amount
        return employee.Salary;
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
