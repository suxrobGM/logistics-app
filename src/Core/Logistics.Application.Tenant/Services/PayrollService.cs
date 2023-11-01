using Logistics.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Tenant.Services;

public class PayrollService : IPayrollService
{
    private readonly IMasterRepository _masterRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<PayrollService> _logger;

    public PayrollService(
        IMasterRepository masterRepository,
        ITenantRepository tenantRepository,
        ILogger<PayrollService> logger)
    {
        _masterRepository = masterRepository;
        _tenantRepository = tenantRepository;
        _logger = logger;
    }

    public async Task GenerateMonthlyPayrollsAsync()
    {
        var tenants = await _masterRepository.GetListAsync<Domain.Entities.Tenant>();

        foreach (var tenant in tenants)
        {
            _tenantRepository.SetCurrentTenant(tenant);
        
            var previousMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-1);
            var previousMonthEnd = previousMonthStart.AddMonths(1).AddDays(-1);
            var employees = await _tenantRepository.GetListAsync<Employee>(e => 
                e.SalaryType == SalaryType.Monthly || 
                e.SalaryType == SalaryType.ShareOfGross);

            foreach (var employee in employees)
            {
                var existingPayroll = await _tenantRepository.GetAsync<Payroll>(p => 
                    p.EmployeeId == employee.Id && 
                    p.StartDate >= previousMonthStart && 
                    p.EndDate <= previousMonthEnd);
            
                if (existingPayroll != null)
                {
                    continue;
                }

                var payroll = CreatePayroll(employee, previousMonthStart, previousMonthEnd);
                await _tenantRepository.AddAsync(payroll);
            }

            _logger.LogInformation("Generated monthly payrolls for the tenant: {TenantName}", tenant.Name);
            await _tenantRepository.UnitOfWork.CommitAsync();
        }
    }

    public async Task GenerateWeeklyPayrollsAsync()
    {
        var tenants = await _masterRepository.GetListAsync<Domain.Entities.Tenant>();

        foreach (var tenant in tenants)
        {
            _tenantRepository.SetCurrentTenant(tenant);
        
            var previousWeekStart = StartOfPreviousWeek(DateTime.UtcNow);
            var previousWeekEnd = previousWeekStart.AddDays(6);
            var employees = await _tenantRepository.GetListAsync<Employee>(e => e.SalaryType == SalaryType.Weekly);

            foreach (var employee in employees)
            {
                var existingPayroll = await _tenantRepository.GetAsync<Payroll>(p => 
                    p.EmployeeId == employee.Id && 
                    p.StartDate >= previousWeekStart && 
                    p.EndDate <= previousWeekEnd);
            
                if (existingPayroll != null)
                {
                    continue;
                }

                var payroll = CreatePayroll(employee, previousWeekStart, previousWeekEnd);
                await _tenantRepository.AddAsync(payroll);
            }

            _logger.LogInformation("Generated weekly payrolls for the tenant: {TenantName}", tenant.Name);
            await _tenantRepository.UnitOfWork.CommitAsync();
        }
    }

    private static Payroll CreatePayroll(Employee employee, DateTime startDate, DateTime endDate)
    {
        var payment = new Payment
        {
            Amount = CalculateSalary(employee, startDate, endDate),
            PaymentFor = PaymentFor.Payroll,
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
    
    private static DateTime StartOfPreviousWeek(DateTime date)
    {
        var daysToSubtract = (int)date.DayOfWeek + 7;
        return date.AddDays(-daysToSubtract).Date;
    }
}
