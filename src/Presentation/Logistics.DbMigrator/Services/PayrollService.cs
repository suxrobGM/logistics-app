using Logistics.DbMigrator.Utils;
using Logistics.DbMigrator.Models;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Consts;

namespace Logistics.DbMigrator.Services;

public class PayrollService
{
    private readonly ITenantUnityOfWork _tenantUow;
    private readonly ILogger<PayrollService> _logger;

    public PayrollService(
        ITenantUnityOfWork tenantUow,
        ILogger<PayrollService> logger)
    {
        _tenantUow = tenantUow;
        _logger = logger;
    }

    public async Task GeneratePayrolls(CompanyEmployees companyEmployees, DateTime startDate, DateTime endDate)
    {
        var monthlyEmployees = companyEmployees.AllEmployees.Where(i => i.SalaryType is SalaryType.Monthly or SalaryType.ShareOfGross).ToArray();
        var weeklyEmployees = companyEmployees.AllEmployees.Where(i => i.SalaryType is SalaryType.Weekly).ToArray();
        var monthlyRanges = DateRangeGenerator.GenerateMonthlyRanges(startDate, endDate);
        var weeklyRanges = DateRangeGenerator.GenerateWeeklyRanges(startDate, endDate);

        await ProcessPayrolls(monthlyEmployees, monthlyRanges);
        await ProcessPayrolls(weeklyEmployees, weeklyRanges);
        await _tenantUow.SaveChangesAsync();
    }
    
    private async Task ProcessPayrolls(Employee[] employees, List<(DateTime StartDate, DateTime EndDate)> dateRanges)
    {
        var payrollRepository = _tenantUow.Repository<PayrollInvoice>();
        foreach (var range in dateRanges)
        {
            foreach (var employee in employees)
            {
                var isPayrollExisting = await IsPayrollExisting(payrollRepository, employee.Id, range.StartDate, range.EndDate);
                
                if (isPayrollExisting)
                {
                    continue;
                }
                
                var payroll = CreatePayrollInvoice(employee, range.StartDate, range.EndDate);
                await payrollRepository.AddAsync(payroll);
                
                _logger.LogInformation(
                    "Generated payrolls for the employee '{EmployeeName}', date range: {StartDate} - {EndDate}",
                    employee.GetFullName(), range.StartDate.ToShortDateString(), range.EndDate.ToShortDateString());
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
    
    private static PayrollInvoice CreatePayrollInvoice(Employee employee, DateTime startDate, DateTime endDate)
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

    private static decimal CalculateSalary(Employee employee, DateTime startDate, DateTime endDate)
    {
        if (employee is { SalaryType: SalaryType.ShareOfGross, Truck: not null })
        {
            var totalDeliveredLoadsGross = employee.Truck.Loads
                .Where(i => i.DeliveryDate.HasValue && 
                            i.DeliveryDate.Value >= startDate && 
                            i.DeliveryDate.Value <= endDate)
                .Sum(i => i.DeliveryCost);

            return totalDeliveredLoadsGross * employee.Salary;
        }
        
        return employee.Salary;
    }
}
