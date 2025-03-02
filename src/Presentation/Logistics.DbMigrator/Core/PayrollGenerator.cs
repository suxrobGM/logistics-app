using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.ValueObjects;
using Logistics.Shared.Consts;

namespace Logistics.DbMigrator.Core;

public class PayrollGenerator
{
    private readonly ITenantUnityOfWork _tenantUow;
    private readonly ILogger _logger;
    private readonly DateTime _startDate;
    private readonly DateTime _endDate;

    public PayrollGenerator(
        ITenantUnityOfWork tenantUow,
        DateTime startDate,
        DateTime endDate,
        ILogger logger)
    {
        _tenantUow = tenantUow;
        _startDate = startDate;
        _endDate = endDate;
        _logger = logger;
    }

    public async Task GeneratePayrolls(CompanyEmployees companyEmployees)
    {
        var monthlyEmployees = companyEmployees.AllEmployees.Where(i => i.SalaryType is SalaryType.Monthly or SalaryType.ShareOfGross).ToArray();
        var weeklyEmployees = companyEmployees.AllEmployees.Where(i => i.SalaryType is SalaryType.Weekly).ToArray();
        var monthlyRanges = DateRangeGenerator.GenerateMonthlyRanges(_startDate, _endDate);
        var weeklyRanges = DateRangeGenerator.GenerateWeeklyRanges(_startDate, _endDate);

        await ProcessPayrolls(monthlyEmployees, monthlyRanges);
        await ProcessPayrolls(weeklyEmployees, weeklyRanges);
        await _tenantUow.SaveChangesAsync();
    }
    
    private async Task ProcessPayrolls(Employee[] employees, List<(DateTime StartDate, DateTime EndDate)> dateRanges)
    {
        var payrollRepository = _tenantUow.Repository<Payroll>();
        foreach (var range in dateRanges)
        {
            foreach (var employee in employees)
            {
                var isPayrollExisting = await IsPayrollExisting(payrollRepository, employee.Id, range.StartDate, range.EndDate);
                
                if (isPayrollExisting)
                {
                    continue;
                }
                
                var payroll = CreatePayroll(employee, range.StartDate, range.EndDate);
                await payrollRepository.AddAsync(payroll);
                
                _logger.LogInformation(
                    "Generated payrolls for the employee '{EmployeeName}', date range: {StartDate} - {EndDate}",
                    employee.GetFullName(), range.StartDate.ToShortDateString(), range.EndDate.ToShortDateString());
            }
        }
    } 
    
    private async Task<bool> IsPayrollExisting(
        ITenantRepository<Payroll> payrollRepository,
        string employeeId,
        DateTime startDate,
        DateTime endDate)
    {
        var payroll = await payrollRepository.GetAsync(p =>
            p.EmployeeId == employeeId &&
            p.StartDate >= startDate &&
            p.EndDate <= endDate);

        return payroll != null;
    }
    
    private static Payroll CreatePayroll(Employee employee, DateTime startDate, DateTime endDate)
    {
        var billingAddress = new Address
        {
            Line1 = "40 Crescent Ave",
            City = "Boston",
            State = "Massachusetts",
            ZipCode = "02125",
            Country = "United States"
        };
        
        var payment = new Payment
        {
            Amount = CalculateSalary(employee, startDate, endDate),
            PaymentFor = PaymentFor.Payroll,
            Method = PaymentMethod.BankAccount,
            Status = PaymentStatus.Paid,
            PaymentDate = DateTime.UtcNow,
            BillingAddress = billingAddress
        };
        
        var payroll = new Payroll
        {
            StartDate = startDate,
            EndDate = endDate,
            Employee = employee,
            Payment = payment
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
