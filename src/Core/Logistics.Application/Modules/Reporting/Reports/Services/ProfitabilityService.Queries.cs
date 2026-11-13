using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Reporting.Reports.Services;

internal sealed partial class ProfitabilityService
{
    private const string RepairsCategory = "Repairs";

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
        var trips = await tenantUow.Repository<TripStop>().Query()
            .Where(s => s.Load.DeliveredAt >= from && s.Load.DeliveredAt < to)
            .GroupBy(s => new { s.LoadId, s.TripId, s.Trip.DriverId, s.Trip.SecondaryDriverId })
            .Select(g => new TripDriverRow(
                g.Key.LoadId, g.Key.DriverId, g.Key.SecondaryDriverId, g.Count(s => s.DepartedAt != null)))
            .ToListAsync(ct);
        return trips.ToLookup(t => t.LoadId);
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
            .Where(e => ExpenseStatuses.Counted.Contains(e.Status) && e.ExpenseDate >= from && e.ExpenseDate < to)
            .GroupBy(e => new { e.TruckId, e.Category })
            .Select(g => new { g.Key.TruckId, g.Key.Category, Amount = g.Sum(e => e.Amount.Amount) })
            .ToListAsync(ct);

        var repairs = await tenantUow.Repository<Expense>().Query()
            .OfType<BodyShopExpense>()
            .Where(e => ExpenseStatuses.Counted.Contains(e.Status) && e.ExpenseDate >= from && e.ExpenseDate < to)
            .GroupBy(e => e.TruckId)
            .Select(g => new { TruckId = g.Key, Amount = g.Sum(e => e.Amount.Amount) })
            .ToListAsync(ct);

        return truckExpenses
            .Select(e => (e.TruckId, Line: new CostLineDto { Category = e.Category.GetDescription(), Amount = e.Amount }))
            .Concat(repairs.Select(r => (r.TruckId, Line: new CostLineDto { Category = RepairsCategory, Amount = r.Amount })))
            .ToLookup(c => c.TruckId, c => c.Line);
    }

    /// <returns>Fees the factor actually charged, keyed by load.</returns>
    private Task<Dictionary<Guid, decimal>> GetFundedFactoringFeesAsync(DateTime from, DateTime to, CancellationToken ct) =>
        tenantUow.Repository<FactoringBatchItem>().Query()
            .Where(f => f.FeeAmount != null &&
                        f.Invoice.Load.DeliveredAt >= from && f.Invoice.Load.DeliveredAt < to)
            .GroupBy(f => f.Invoice.LoadId)
            .Select(g => new { LoadId = g.Key, Fee = g.Sum(f => f.FeeAmount!.Value) })
            .ToDictionaryAsync(f => f.LoadId, f => f.Fee, ct);

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

    private Task<decimal> SumOverheadAsync(DateTime from, DateTime to, CancellationToken ct) =>
        tenantUow.Repository<Expense>().Query()
            .OfType<CompanyExpense>()
            .Where(e => ExpenseStatuses.Counted.Contains(e.Status) && e.ExpenseDate >= from && e.ExpenseDate < to)
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
        return staff.Sum(e => CalculatePeriodPay(e, period, hours));
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
