using Logistics.Domain.Entities;
using Logistics.Domain.Primitives;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Financial.Payroll.Services;

internal sealed class DriverPayCalculator : IDriverPayCalculator
{
    public decimal PayForLoads(Employee employee, IReadOnlyCollection<LoadPayBasis> loads, DistanceUnit unit)
    {
        var rate = employee.Salary.Amount;
        return employee.SalaryType switch
        {
            SalaryType.ShareOfGross =>
                loads.Sum(l => employee.PayableRevenue(l.Linehaul, l.FuelSurcharge, l.Accessorials)) * rate,
            SalaryType.RatePerDistance => unit.FromMeters(loads.Sum(l => l.Distance)).ToDecimal() * rate,
            SalaryType.PerLoad => loads.Count * rate,
            SalaryType.PerStop => PayForStops(employee, loads.Sum(l => l.DepartedStops)),
            _ => 0m
        };
    }

    public decimal PayForStops(Employee employee, int stops) => stops * employee.Salary.Amount;

    public decimal PayForPeriod(Employee employee, decimal weeks, decimal months, decimal hours)
    {
        var rate = employee.Salary.Amount;
        return employee.SalaryType switch
        {
            SalaryType.Weekly => weeks * rate,
            SalaryType.Monthly => months * rate,
            SalaryType.Hourly => hours * rate,
            SalaryType.ShareOfGross or SalaryType.RatePerDistance or SalaryType.PerLoad or SalaryType.PerStop => 0m,
            _ => rate
        };
    }
}
