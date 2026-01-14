using Logistics.DbMigrator.Models;
using Logistics.DbMigrator.Utils;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.DbMigrator.Services;

public class PayrollService(
    ITenantUnitOfWork tenantUow,
    ILogger<PayrollService> logger)
{
    public async Task GeneratePayrolls(CompanyEmployees companyEmployees, DateTime startDate, DateTime endDate)
    {
        var monthlyEmployees = companyEmployees.AllEmployees
            .Where(i => i.SalaryType is SalaryType.Monthly or SalaryType.ShareOfGross).ToArray();
        var weeklyEmployees = companyEmployees.AllEmployees.Where(i => i.SalaryType is SalaryType.Weekly).ToArray();
        var monthlyRanges = DateRangeGenerator.GenerateMonthlyRanges(startDate, endDate);
        var weeklyRanges = DateRangeGenerator.GenerateWeeklyRanges(startDate, endDate);

        await ProcessPayrolls(monthlyEmployees, monthlyRanges);
        await ProcessPayrolls(weeklyEmployees, weeklyRanges);
        await tenantUow.SaveChangesAsync();
    }

    private async Task ProcessPayrolls(Employee[] employees, List<(DateTime StartDate, DateTime EndDate)> dateRanges)
    {
        var payrollRepository = tenantUow.Repository<PayrollInvoice>();
        foreach (var (StartDate, EndDate) in dateRanges)
        {
            foreach (var employee in employees)
            {
                var isPayrollExisting =
                    await IsPayrollExisting(payrollRepository, employee.Id, StartDate, EndDate);

                if (isPayrollExisting)
                {
                    continue;
                }

                var payroll = CreatePayrollInvoice(employee, StartDate, EndDate);
                await payrollRepository.AddAsync(payroll);

                logger.LogInformation(
                    "Generated payrolls for the employee '{EmployeeName}', date range: {StartDate} - {EndDate}",
                    employee.GetFullName(), StartDate.ToShortDateString(), EndDate.ToShortDateString());
            }
        }
    }

    private static async Task<bool> IsPayrollExisting(
        ITenantRepository<PayrollInvoice, Guid> payrollRepository,
        Guid employeeId,
        DateTime startDate,
        DateTime endDate)
    {
        var payroll = await payrollRepository.GetAsync(p =>
            p.EmployeeId == employeeId &&
            p.PeriodStart >= startDate &&
            p.PeriodEnd <= endDate);

        return payroll != null;
    }

    private PayrollInvoice CreatePayrollInvoice(Employee employee, DateTime startDate, DateTime endDate)
    {
        var amount = CalculateSalary(employee, startDate, endDate);

        var payroll = new PayrollInvoice
        {
            Total = amount,
            Status = InvoiceStatus.Paid,
            PeriodStart = startDate,
            PeriodEnd = endDate,
            EmployeeId = employee.Id,
            Employee = employee
        };

        return payroll;
    }

    private decimal CalculateSalary(Employee employee, DateTime startDate, DateTime endDate)
    {
        // Fixed amount (weekly / monthly) – nothing changed
        if (employee.SalaryType is not SalaryType.ShareOfGross)
        {
            return employee.Salary;
        }

        // Share-of-gross: sum every load that was delivered in the period
        // by ANY truck where this employee was the main OR secondary driver.
        var totalGross = tenantUow.Repository<Truck>()
            .Query() // IQueryable<Truck>
            .Where(t => t.MainDriverId == employee.Id ||
                        t.SecondaryDriverId == employee.Id)
            .SelectMany(t => t.Loads.Where(l =>
                l.DeliveredAt.HasValue &&
                l.DeliveredAt.Value >= startDate &&
                l.DeliveredAt.Value <= endDate))
            .Sum(l => l.DeliveryCost.Amount);

        // `employee.Salary` stores the share ratio (e.g. 0.25 for 25 %)
        return totalGross * employee.Salary;
    }
}
