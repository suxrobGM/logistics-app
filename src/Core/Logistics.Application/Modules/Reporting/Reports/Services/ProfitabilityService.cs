using Logistics.Application.Modules.AI.Negotiation;
using Logistics.Application.Modules.Financial.Payroll.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Reporting.Reports.Services;

internal sealed partial class ProfitabilityService(
    ITenantUnitOfWork tenantUow,
    IDriverPayCalculator payCalculator) : IProfitabilityService
{
    private const decimal DaysPerMonth = 365.25m / 12;

    public async Task<ProfitabilityResult> BuildAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        from = DateTime.SpecifyKind(from, DateTimeKind.Utc);
        to = DateTime.SpecifyKind(to, DateTimeKind.Utc);
        var unit = tenantUow.GetCurrentTenant().Settings.DistanceUnit;

        var loads = await GetDeliveredLoadsAsync(from, to, ct);
        var tripDrivers = await GetTripDriversAsync(from, to, ct);
        var trucks = await GetTrucksAsync(ct);
        var truckCosts = await GetTruckCostsAsync(from, to, ct);
        var funded = await GetFundedFactoringFeesAsync(from, to, ct);
        var factors = await GetFactoringCompaniesAsync(ct);

        var creditedDrivers = loads.ToDictionary(l => l.Id, l => CreditedDrivers(l, tripDrivers, trucks));
        var driverIds = creditedDrivers.Values.SelectMany(d => d).Concat(TruckDriverIds(trucks));
        var drivers = await GetDriversAsync(driverIds, ct);
        var hours = await GetHoursAsync(drivers.Values, from, to, ct);

        var rows = loads.ToDictionary(l => l.Id, l => NewRow(l, trucks, funded, factors));
        var unallocatedDriverPay = AddDriverPay(loads, rows, creditedDrivers, tripDrivers, drivers, hours, unit, from, to);
        var unallocatedTruckCosts = AddTruckCosts(loads, rows, truckCosts);

        foreach (var row in rows.Values)
        {
            row.TruckCostsTotal = row.TruckCosts.Sum(c => c.Amount);
            row.Margin = row.Revenue - row.DriverPay - row.TruckCostsTotal - row.FactoringFee;
            row.MarginPerDistance = PerDistance(row.Margin, row.Distance, unit);
        }

        var loadRows = rows.Values.OrderBy(r => r.Margin).ToList();
        var actualDistance = await GetActualDistanceAsync(from, to, ct);
        var overhead = await SumOverheadAsync(from, to, ct);
        var unassignedFuel = await SumUnassignedFuelAsync(from, to, ct);

        return new ProfitabilityResult(
            loadRows,
            GroupByTruck(loadRows, trucks, truckCosts, actualDistance, unit),
            GroupByLane(loadRows, unit),
            Summarize(loadRows, unallocatedTruckCosts, unallocatedDriverPay, overhead, unassignedFuel));
    }

    private static LoadProfitDto NewRow(
        DeliveredLoadRow load,
        Dictionary<Guid, TruckRow> trucks,
        Dictionary<Guid, decimal> funded,
        Dictionary<Guid, FactoringCompany> factors)
    {
        var isFunded = funded.TryGetValue(load.Id, out var fundedFee);
        var estimatedFee = isFunded ? null : EstimateFactoringFee(load, factors);

        return new LoadProfitDto
        {
            LoadId = load.Id,
            LoadNumber = load.Number,
            TruckId = load.TruckId,
            TruckNumber = load.TruckId is { } truckId && trucks.TryGetValue(truckId, out var truck) ? truck.Number : null,
            CustomerId = load.CustomerId,
            CustomerName = load.CustomerName,
            Lane = LaneName(load.Origin, load.Destination),
            DeliveredAt = load.DeliveredAt,
            Distance = load.Distance,
            Revenue = load.Revenue,
            FactoringFee = isFunded ? fundedFee : estimatedFee ?? 0m,
            FactoringFeeEstimated = estimatedFee is not null
        };
    }

    /// <summary>Same crediting rule as payroll: the trips' drivers, else whoever drives the truck now.</summary>
    private static HashSet<Guid> CreditedDrivers(
        DeliveredLoadRow load, ILookup<Guid, TripDriverRow> tripDrivers, Dictionary<Guid, TruckRow> trucks)
    {
        if (load.HasTripStops)
        {
            return tripDrivers[load.Id].SelectMany(t => t.DriverIds()).ToHashSet();
        }

        if (load.TruckId is { } truckId && trucks.TryGetValue(truckId, out var truck))
        {
            return truck.DriverIds().ToHashSet();
        }

        return [];
    }

    private static IEnumerable<Guid> TruckDriverIds(Dictionary<Guid, TruckRow> trucks) =>
        trucks.Values.SelectMany(t => t.DriverIds());

    /// <returns>Period pay of drivers with no load to spread it on.</returns>
    private decimal AddDriverPay(
        IReadOnlyList<DeliveredLoadRow> loads,
        Dictionary<Guid, LoadProfitDto> rows,
        Dictionary<Guid, HashSet<Guid>> creditedDrivers,
        ILookup<Guid, TripDriverRow> tripDrivers,
        Dictionary<Guid, Employee> drivers,
        Dictionary<Guid, decimal> hours,
        DistanceUnit unit,
        DateTime from,
        DateTime to)
    {
        var days = (to - from).TotalDays.ToDecimal();
        var weeks = days / 7;
        var months = days / DaysPerMonth;
        var unallocated = 0m;

        foreach (var driver in drivers.Values)
        {
            var driverLoads = loads.Where(l => creditedDrivers[l.Id].Contains(driver.Id)).ToList();
            foreach (var load in driverLoads)
            {
                var stops = tripDrivers[load.Id]
                    .Where(t => t.DriverIds().Contains(driver.Id))
                    .Sum(t => t.DepartedStops);
                var basis = new LoadPayBasis(load.Linehaul, load.FuelSurcharge, load.Accessorials, load.Distance, stops);
                rows[load.Id].DriverPay += payCalculator.PayForLoads(driver, [basis], unit);
            }

            var periodPay = Math.Round(
                payCalculator.PayForPeriod(driver, weeks, months, hours.GetValueOrDefault(driver.Id)), 2);
            if (periodPay == 0)
            {
                continue;
            }

            if (driverLoads.Count == 0)
            {
                unallocated += periodPay;
                continue;
            }

            var shares = Spread(periodPay, driverLoads.Select(l => l.Distance).ToList());
            for (var i = 0; i < driverLoads.Count; i++)
            {
                var row = rows[driverLoads[i].Id];
                row.DriverPay += shares[i];
                row.DriverPayEstimated = true;
            }
        }

        return unallocated;
    }

    /// <returns>Costs of trucks that delivered nothing in the period.</returns>
    private static decimal AddTruckCosts(
        IReadOnlyList<DeliveredLoadRow> loads,
        Dictionary<Guid, LoadProfitDto> rows,
        ILookup<Guid, CostLineDto> truckCosts)
    {
        var loadsByTruck = loads.Where(l => l.TruckId is not null).ToLookup(l => l.TruckId!.Value);
        var unallocated = 0m;

        foreach (var costs in truckCosts)
        {
            var truckLoads = loadsByTruck[costs.Key].ToList();
            if (truckLoads.Count == 0)
            {
                unallocated += costs.Sum(c => c.Amount);
                continue;
            }

            var weights = truckLoads.Select(l => l.Distance).ToList();
            var lines = truckLoads.ToDictionary(l => l.Id, _ => new List<CostLineDto>());
            foreach (var cost in costs)
            {
                var shares = Spread(cost.Amount, weights);
                for (var i = 0; i < truckLoads.Count; i++)
                {
                    lines[truckLoads[i].Id].Add(new CostLineDto { Category = cost.Category, Amount = shares[i] });
                }
            }

            foreach (var (loadId, loadLines) in lines)
            {
                rows[loadId].TruckCosts = loadLines;
            }
        }

        return unallocated;
    }

    /// <summary>Splits an amount by weight to the cent; the last share takes the rounding so the parts add up exactly.</summary>
    internal static decimal[] Spread(decimal amount, IReadOnlyList<double> weights)
    {
        var total = weights.Sum();
        var shares = new decimal[weights.Count];
        var assigned = 0m;
        for (var i = 0; i < weights.Count - 1; i++)
        {
            var ratio = total > 0 ? (weights[i] / total).ToDecimal() : 1m / weights.Count;
            shares[i] = Math.Round(amount * ratio, 2);
            assigned += shares[i];
        }

        shares[^1] = amount - assigned;
        return shares;
    }

    private static decimal? EstimateFactoringFee(DeliveredLoadRow load, Dictionary<Guid, FactoringCompany> factors)
    {
        if (load.FactoringCompanyId is not { } companyId || !factors.TryGetValue(companyId, out var company))
        {
            return null;
        }

        return company.EstimateAmounts(load.InvoiceTotal ?? load.Revenue).Fee;
    }

    private static string LaneName(Address origin, Address destination) =>
        $"{LanePoint(origin)} → {LanePoint(destination)}";

    private static string LanePoint(Address address) =>
        LaneKey.State(address.State) ?? LaneKey.Country(address.Country);

    private static decimal? PerDistance(decimal amount, double meters, DistanceUnit unit)
    {
        var distance = unit.FromMeters(meters);
        return distance > 0 ? Math.Round(amount / distance.ToDecimal(), 2) : null;
    }
}
