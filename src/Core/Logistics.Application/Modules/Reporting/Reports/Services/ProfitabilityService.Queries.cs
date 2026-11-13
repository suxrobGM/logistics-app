using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Reporting.Reports.Services;

internal sealed partial class ProfitabilityService
{
    private const string RepairsCategory = "Repairs";

    /// <summary>Pending expenses may still be rejected, so only these count as costs.</summary>
    private static readonly ExpenseStatus[] CountedStatuses = [ExpenseStatus.Approved, ExpenseStatus.Paid];

    private Task<List<DeliveredLoadRow>> GetDeliveredLoadsAsync(DateTime from, DateTime to, CancellationToken ct) =>
        tenantUow.Repository<Load>().Query()
            .Where(l => l.DeliveredAt >= from && l.DeliveredAt < to)
            .Select(l => new DeliveredLoadRow(
                l.Id,
                l.Number,
                l.AssignedTruckId,
                l.CustomerId,
                l.Customer.Name,
                l.Customer.FactoringCompanyId,
                l.OriginAddress,
                l.DestinationAddress,
                l.DeliveredAt!.Value,
                l.DeliveryCost.Amount,
                l.FuelSurcharge,
                l.Accessorials.Sum(a => a.Rate * a.Quantity),
                l.Distance,
                l.TripStops.Any(),
                l.Invoice != null ? l.Invoice.Total.Amount : null))
            .ToListAsync(ct);

    /// <returns>Each load's trips with their drivers and how many of the load's stops departed, keyed by load.</returns>
    private async Task<ILookup<Guid, TripDriverRow>> GetTripDriversAsync(DateTime from, DateTime to, CancellationToken ct)
    {
        var stops = await tenantUow.Repository<TripStop>().Query()
            .Where(s => s.Load.DeliveredAt >= from && s.Load.DeliveredAt < to)
            .Select(s => new
            {
                s.LoadId,
                s.TripId,
                s.Trip.DriverId,
                s.Trip.SecondaryDriverId,
                Departed = s.DepartedAt != null
            })
            .ToListAsync(ct);

        return stops
            .GroupBy(s => (s.LoadId, s.TripId))
            .Select(g => new TripDriverRow(
                g.Key.LoadId, g.First().DriverId, g.First().SecondaryDriverId, g.Count(s => s.Departed)))
            .ToLookup(t => t.LoadId);
    }

    private Task<Dictionary<Guid, TruckRow>> GetTrucksAsync(CancellationToken ct) =>
        tenantUow.Repository<Truck>().Query()
            .Select(t => new TruckRow(t.Id, t.Number, t.MainDriverId, t.SecondaryDriverId))
            .ToDictionaryAsync(t => t.Id, ct);

    /// <returns>Approved or paid truck expenses summed per category, keyed by truck.</returns>
    private async Task<ILookup<Guid, CostLineDto>> GetTruckCostsAsync(DateTime from, DateTime to, CancellationToken ct)
    {
        var truckExpenses = await tenantUow.Repository<Expense>().Query()
            .OfType<TruckExpense>()
            .Where(e => CountedStatuses.Contains(e.Status) && e.ExpenseDate >= from && e.ExpenseDate < to)
            .GroupBy(e => new { e.TruckId, e.Category })
            .Select(g => new { g.Key.TruckId, g.Key.Category, Amount = g.Sum(e => e.Amount.Amount) })
            .ToListAsync(ct);

        var repairs = await tenantUow.Repository<Expense>().Query()
            .OfType<BodyShopExpense>()
            .Where(e => CountedStatuses.Contains(e.Status) && e.ExpenseDate >= from && e.ExpenseDate < to)
            .GroupBy(e => e.TruckId)
            .Select(g => new { TruckId = g.Key, Amount = g.Sum(e => e.Amount.Amount) })
            .ToListAsync(ct);

        return truckExpenses
            .Select(e => (e.TruckId, Line: new CostLineDto { Category = e.Category.GetDescription(), Amount = e.Amount }))
            .Concat(repairs.Select(r => (r.TruckId, Line: new CostLineDto { Category = RepairsCategory, Amount = r.Amount })))
            .ToLookup(c => c.TruckId, c => c.Line);
    }

    /// <returns>Fees the factor actually charged, keyed by load.</returns>
    private async Task<Dictionary<Guid, decimal>> GetFundedFactoringFeesAsync(DateTime from, DateTime to, CancellationToken ct)
    {
        var fees = await tenantUow.Repository<FactoringBatchItem>().Query()
            .Where(f => f.FeeAmount != null &&
                        f.Invoice.Load.DeliveredAt >= from && f.Invoice.Load.DeliveredAt < to)
            .Select(f => new { f.Invoice.LoadId, Fee = f.FeeAmount!.Value })
            .ToListAsync(ct);

        return fees.GroupBy(f => f.LoadId).ToDictionary(g => g.Key, g => g.Sum(f => f.Fee));
    }

    private async Task<Dictionary<Guid, FactoringCompany>> GetFactoringCompaniesAsync(CancellationToken ct)
    {
        var companies = await tenantUow.Repository<FactoringCompany>().Query().ToListAsync(ct);
        return companies.ToDictionary(c => c.Id);
    }

    private async Task<Dictionary<Guid, Employee>> GetDriversAsync(IEnumerable<Guid> ids, CancellationToken ct)
    {
        var distinctIds = ids.Distinct().ToList();
        var drivers = await tenantUow.Repository<Employee>().Query()
            .Where(e => distinctIds.Contains(e.Id))
            .ToListAsync(ct);
        return drivers.ToDictionary(e => e.Id);
    }

    private async Task<Dictionary<Guid, decimal>> GetHoursAsync(
        IEnumerable<Employee> drivers, DateTime from, DateTime to, CancellationToken ct)
    {
        var hourlyIds = drivers.Where(d => d.SalaryType == SalaryType.Hourly).Select(d => d.Id).ToList();
        if (hourlyIds.Count == 0)
        {
            return [];
        }

        var hours = await tenantUow.Repository<TimeEntry>().Query()
            .Where(te => hourlyIds.Contains(te.EmployeeId) && te.Date >= from && te.Date < to)
            .GroupBy(te => te.EmployeeId)
            .Select(g => new { EmployeeId = g.Key, Hours = g.Sum(te => te.TotalHours) })
            .ToListAsync(ct);
        return hours.ToDictionary(h => h.EmployeeId, h => h.Hours);
    }

    /// <returns>Metres each truck drove per the IFTA mileage log.</returns>
    private async Task<Dictionary<Guid, double>> GetActualDistanceAsync(DateTime from, DateTime to, CancellationToken ct)
    {
        var fromDay = DateOnly.FromDateTime(from);
        var toDay = DateOnly.FromDateTime(to);
        var miles = await tenantUow.Repository<TruckJurisdictionMileage>().Query()
            .Where(m => m.Date >= fromDay && m.Date < toDay)
            .GroupBy(m => m.TruckId)
            .Select(g => new { TruckId = g.Key, Miles = g.Sum(m => m.Miles) })
            .ToListAsync(ct);
        return miles.ToDictionary(m => m.TruckId, m => DistanceUnit.Miles.ToMeters((double)m.Miles));
    }

    private Task<decimal> SumOverheadAsync(DateTime from, DateTime to, CancellationToken ct) =>
        tenantUow.Repository<Expense>().Query()
            .OfType<CompanyExpense>()
            .Where(e => CountedStatuses.Contains(e.Status) && e.ExpenseDate >= from && e.ExpenseDate < to)
            .SumAsync(e => e.Amount.Amount, ct);

    /// <summary>Period-paid employees who are not driving: dispatchers, office and management.</summary>
    private static readonly SalaryType[] StaffSalaryTypes = [SalaryType.Weekly, SalaryType.Monthly, SalaryType.Hourly];

    private async Task<decimal> SumStaffPayAsync(
        IEnumerable<Guid> driverIds, PayPeriod period, DateTime from, DateTime to, CancellationToken ct)
    {
        var excluded = driverIds.ToList();
        var staff = await tenantUow.Repository<Employee>().Query()
            .Where(e => !excluded.Contains(e.Id) &&
                        e.Status != EmployeeStatus.Terminated &&
                        StaffSalaryTypes.Contains(e.SalaryType))
            .ToListAsync(ct);
        var hours = await GetHoursAsync(staff, from, to, ct);
        return staff.Sum(e => PeriodPay(e, period, hours));
    }

    private Task<decimal> SumUnassignedFuelAsync(DateTime from, DateTime to, CancellationToken ct) =>
        tenantUow.Repository<FuelCardTransaction>().Query()
            .Where(t => t.Status == FuelCardTransactionStatus.Pending &&
                        t.TransactionDate >= from && t.TransactionDate < to)
            .SumAsync(t => t.Amount.Amount, ct);

    private sealed record DeliveredLoadRow(
        Guid Id,
        long Number,
        Guid? TruckId,
        Guid CustomerId,
        string CustomerName,
        Guid? FactoringCompanyId,
        Address Origin,
        Address Destination,
        DateTime DeliveredAt,
        decimal Linehaul,
        decimal FuelSurcharge,
        decimal Accessorials,
        double Distance,
        bool HasTripStops,
        decimal? InvoiceTotal)
    {
        public decimal Revenue => Linehaul + FuelSurcharge + Accessorials;
    }

    private sealed record TripDriverRow(Guid LoadId, Guid? DriverId, Guid? SecondaryDriverId, int DepartedStops)
    {
        public IEnumerable<Guid> DriverIds() => new[] { DriverId, SecondaryDriverId }.OfType<Guid>();
    }

    private sealed record TruckRow(Guid Id, string Number, Guid? MainDriverId, Guid? SecondaryDriverId)
    {
        public IEnumerable<Guid> DriverIds() => new[] { MainDriverId, SecondaryDriverId }.OfType<Guid>();
    }
}
