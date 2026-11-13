using Logistics.Application.Abstractions.ApplicationServices;
using Logistics.Application.Modules.Compliance.Ifta.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Reporting.Reports.Services;

internal sealed class ProfitEstimateService(ITenantUnitOfWork tenantUow) : IProfitEstimateService
{
    private const int TrailingDays = 90;
    private static readonly decimal MetersPerMile = DistanceUnit.Miles.ToMeters(1).ToDecimal();

    /// <summary>A load not on a trip yet has no stops; a pickup and a drop is the usual pair.</summary>
    private const int EstimatedStops = 2;

    public async Task<IReadOnlyDictionary<(Guid TruckId, Guid LoadId), decimal>> EstimateMarginsAsync(
        IReadOnlyCollection<MarginRequest> requests, CancellationToken ct = default)
    {
        var truckIds = requests.Select(r => r.TruckId).Distinct().ToList();
        var loadIds = requests.Select(r => r.LoadId).Distinct().ToList();

        var costs = await GetCostPerMetreAsync(ct);
        var loads = await tenantUow.Repository<Load>().Query()
            .Where(l => loadIds.Contains(l.Id))
            .Select(l => new
            {
                l.Id,
                Basis = new LoadPayBasis(
                    l.DeliveryCost.Amount,
                    l.FuelSurcharge,
                    l.Accessorials.Sum(a => a.Rate * a.Quantity),
                    l.Distance,
                    EstimatedStops)
            })
            .ToDictionaryAsync(l => l.Id, l => l.Basis, ct);
        var drivers = await tenantUow.Repository<Truck>().Query()
            .Where(t => truckIds.Contains(t.Id) && t.MainDriver != null)
            .Select(t => new { t.Id, Driver = t.MainDriver! })
            .ToDictionaryAsync(t => t.Id, t => t.Driver, ct);
        var unit = tenantUow.GetCurrentTenant().Settings.DistanceUnit;

        var margins = new Dictionary<(Guid, Guid), decimal>();
        foreach (var request in requests)
        {
            decimal? costPerMetre = costs.ByTruck.TryGetValue(request.TruckId, out var truckCost) ? truckCost : costs.Fleet;
            if (costPerMetre is null || !loads.TryGetValue(request.LoadId, out var load))
            {
                continue;
            }

            var driverPay = drivers.TryGetValue(request.TruckId, out var driver)
                ? driver.PayForLoads([load], unit)
                : 0m;
            var revenue = load.Linehaul + load.FuelSurcharge + load.Accessorials;
            var operatingCost = Math.Round(costPerMetre.Value * request.TotalMeters.ToDecimal(), 2);
            margins[(request.TruckId, request.LoadId)] = revenue - operatingCost - driverPay;
        }

        return margins;
    }

    public async Task<decimal?> GetFleetCostPerMileAsync(CancellationToken ct = default) =>
        (await GetCostPerMetreAsync(ct)).Fleet * MetersPerMile;

    /// <summary>The tenant's cost per mile when set, else each truck's trailing expenses over the distance it drove.</summary>
    private async Task<OperatingCosts> GetCostPerMetreAsync(CancellationToken ct)
    {
        if (tenantUow.GetCurrentTenant().Settings.CostPerMile is { } costPerMile)
        {
            return new OperatingCosts([], costPerMile / MetersPerMile);
        }

        var now = DateTime.UtcNow;
        var from = now.AddDays(-TrailingDays);

        var expenses = await tenantUow.Repository<Expense>().Query()
            .OfType<TruckLinkedExpense>()
            .Where(e => ExpenseStatuses.Counted.Contains(e.Status) && e.ExpenseDate >= from)
            .GroupBy(e => e.TruckId)
            .Select(g => new { TruckId = g.Key, Amount = g.Sum(e => e.Amount.Amount) })
            .ToDictionaryAsync(e => e.TruckId, e => e.Amount, ct);
        var miles = await tenantUow.SumLoggedMetersByTruckAsync(from, now.AddDays(1), ct);
        var loaded = await tenantUow.Repository<Load>().Query()
            .Where(l => l.DeliveredAt >= from && l.AssignedTruckId != null)
            .GroupBy(l => l.AssignedTruckId!.Value)
            .Select(g => new { TruckId = g.Key, Meters = g.Sum(l => l.Distance) })
            .ToDictionaryAsync(l => l.TruckId, l => l.Meters, ct);

        var byTruck = new Dictionary<Guid, decimal>();
        var fleetCost = 0m;
        var fleetMeters = 0d;
        foreach (var (truckId, amount) in expenses)
        {
            // A log shorter than the loaded distance is missing days; the longer figure is the truer one.
            var meters = Math.Max(miles.GetValueOrDefault(truckId), loaded.GetValueOrDefault(truckId));

            if (meters <= 0)
            {
                continue;
            }

            byTruck[truckId] = amount / meters.ToDecimal();
            fleetCost += amount;
            fleetMeters += meters;
        }

        return new OperatingCosts(byTruck, fleetMeters > 0 ? fleetCost / fleetMeters.ToDecimal() : null);
    }

    private sealed record OperatingCosts(Dictionary<Guid, decimal> ByTruck, decimal? Fleet);
}
