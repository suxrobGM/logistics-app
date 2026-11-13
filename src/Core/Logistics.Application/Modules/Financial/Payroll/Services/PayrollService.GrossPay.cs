using System.Globalization;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Financial.Payroll.Services;

internal partial class PayrollService
{
    private async Task<GrossPay> CalculateGrossPayAsync(PayrollInvoice payroll, CancellationToken ct)
    {
        var employee = payroll.Employee;
        var rate = employee.Salary;
        var (from, to) = payroll.PeriodUtcRange();

        switch (employee.SalaryType)
        {
            case SalaryType.ShareOfGross:
                {
                    var loads = await GetDeliveredLoadsAsync(employee.Id, from, to, ct);
                    return new GrossPay(employee.PayForLoads(loads),
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
                    var loads = await GetDeliveredLoadsAsync(employee.Id, from, to, ct);
                    var meters = loads.Sum(l => l.Distance);
                    var unit = GetDistanceUnit();
                    var distance = unit.FromMeters(meters);
                    return new GrossPay(employee.PayForLoads(loads),
                        $"{loads.Count} load(s), {distance:0} {unit.Abbreviation()} x {unit.RateFromPerKm(rate):0.00}",
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
                    return new GrossPay(employee.PayForLoads(loads),
                        $"{loads.Count} load(s) x {rate:0.00}");
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

    private DistanceUnit GetDistanceUnit() => tenantUow.GetCurrentTenant().Settings.DistanceUnit;

    /// <summary>
    /// Loads delivered in the period on a trip the employee drove, main or secondary. A load on no trip has
    /// no recorded driver, so it goes to whoever drives its truck now.
    /// </summary>
    private Task<List<LoadPayBasis>> GetDeliveredLoadsAsync(Guid employeeId, DateTime from, DateTime to, CancellationToken ct) =>
        tenantUow.Repository<Load>().Query()
            .Where(l => l.DeliveredAt >= from && l.DeliveredAt < to &&
                        (l.TripStops.Any()
                            ? l.TripStops.Any(s => s.Trip.DriverId == employeeId || s.Trip.SecondaryDriverId == employeeId)
                            : l.AssignedTruck != null &&
                              (l.AssignedTruck.MainDriverId == employeeId || l.AssignedTruck.SecondaryDriverId == employeeId)))
            .Select(l => new LoadPayBasis(
                l.DeliveryCost,
                l.FuelSurcharge,
                l.Accessorials.Sum(a => a.Rate * a.Quantity),
                l.Distance))
            .ToListAsync(ct);

    /// <summary>Stops the driver app marked departed in the period, on trips the employee drove.</summary>
    private Task<int> CountDepartedStopsAsync(Guid employeeId, DateTime from, DateTime to, CancellationToken ct) =>
        tenantUow.Repository<TripStop>().Query()
            .CountAsync(s => s.DepartedAt >= from && s.DepartedAt < to &&
                             (s.Trip.DriverId == employeeId || s.Trip.SecondaryDriverId == employeeId), ct);

    private static string Percent(decimal ratio) => (ratio * 100).ToString("0.##", CultureInfo.InvariantCulture) + "%";

    private sealed record GrossPay(decimal Amount, string Description, double Distance = 0, decimal Hours = 0);
}
