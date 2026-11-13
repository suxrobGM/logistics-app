using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Reporting.Reports.Services;

internal sealed partial class ProfitabilityService
{
    /// <summary>Trucks with a delivered load or a cost in the period. A truck's margin also carries costs no load absorbed.</summary>
    private static List<TruckProfitDto> GroupByTruck(
        IReadOnlyList<LoadProfitDto> loads,
        Dictionary<Guid, TruckRow> trucks,
        ILookup<Guid, CostLineDto> truckCosts,
        Dictionary<Guid, double> actualDistance,
        DistanceUnit unit)
    {
        var loadsByTruck = loads.Where(l => l.TruckId is not null).ToLookup(l => l.TruckId!.Value);
        var truckIds = loadsByTruck.Select(g => g.Key).Union(truckCosts.Select(g => g.Key));

        return truckIds
            .Select(truckId =>
            {
                var truckLoads = loadsByTruck[truckId].ToList();
                var costs = truckCosts[truckId].ToList();
                var revenue = truckLoads.Sum(l => l.Revenue);
                var driverPay = truckLoads.Sum(l => l.DriverPay);
                var factoringFee = truckLoads.Sum(l => l.FactoringFee);
                var costsTotal = costs.Sum(c => c.Amount);
                var loaded = truckLoads.Sum(l => l.Distance);
                double? actual = LoggedDistance(actualDistance, truckId, loaded);

                return new TruckProfitDto
                {
                    TruckId = truckId,
                    TruckNumber = trucks.TryGetValue(truckId, out var truck) ? truck.Number : string.Empty,
                    LoadCount = truckLoads.Count,
                    LoadedDistance = loaded,
                    ActualDistance = actual,
                    DeadheadPercent = DeadheadPercent(loaded, actual),
                    Revenue = revenue,
                    DriverPay = driverPay,
                    TruckCosts = costs,
                    TruckCostsTotal = costsTotal,
                    FactoringFee = factoringFee,
                    Margin = revenue - driverPay - costsTotal - factoringFee,
                    CostPerDistance = PerDistance(driverPay + costsTotal, actual ?? loaded, unit)
                };
            })
            .OrderBy(t => t.Margin)
            .ToList();
    }

    /// <summary>
    /// A log shorter than the loads the truck delivered is missing days, so it would show no deadhead and
    /// overstate cost per distance; such a truck is treated as having no log.
    /// </summary>
    private static double? LoggedDistance(Dictionary<Guid, double> actualDistance, Guid truckId, double loaded) =>
        actualDistance.TryGetValue(truckId, out var meters) && meters >= loaded ? meters : null;

    private static double? DeadheadPercent(double loaded, double? actual)
    {
        if (actual is not > 0)
        {
            return null;
        }

        return Math.Round((actual.Value - loaded) / actual.Value * 100, 1);
    }

    private static List<LaneProfitDto> GroupByLane(IReadOnlyList<LoadProfitDto> loads, DistanceUnit unit) =>
        loads
            .GroupBy(l => l.Lane)
            .Select(lane =>
            {
                var revenue = lane.Sum(l => l.Revenue);
                var margin = lane.Sum(l => l.Margin);
                var distance = lane.Sum(l => l.Distance);
                var customers = lane
                    .GroupBy(l => l.CustomerName)
                    .Select(c => (Name: c.Key, Margin: c.Sum(l => l.Margin)))
                    .OrderByDescending(c => c.Margin)
                    .ToList();

                return new LaneProfitDto
                {
                    Lane = lane.Key,
                    LoadCount = lane.Count(),
                    Revenue = revenue,
                    Margin = margin,
                    RevenuePerDistance = PerDistance(revenue, distance, unit),
                    MarginPerDistance = PerDistance(margin, distance, unit),
                    BestCustomer = customers[0].Name,
                    WorstCustomer = customers[^1].Name
                };
            })
            .OrderBy(l => l.Margin)
            .ToList();

    private static ProfitabilitySummaryDto Summarize(
        IReadOnlyList<LoadProfitDto> loads,
        decimal unallocatedTruckCosts,
        decimal unallocatedDriverPay,
        decimal staffPay,
        decimal overhead,
        decimal unassignedFuel)
    {
        var loadMargin = loads.Sum(l => l.Margin);
        return new ProfitabilitySummaryDto
        {
            LoadCount = loads.Count,
            Revenue = loads.Sum(l => l.Revenue),
            DriverPay = loads.Sum(l => l.DriverPay),
            TruckCosts = loads.Sum(l => l.TruckCostsTotal),
            FactoringFees = loads.Sum(l => l.FactoringFee),
            LoadMargin = loadMargin,
            UnallocatedTruckCosts = unallocatedTruckCosts,
            UnallocatedDriverPay = unallocatedDriverPay,
            StaffPay = staffPay,
            Overhead = overhead,
            NetProfit = loadMargin - unallocatedTruckCosts - unallocatedDriverPay - staffPay - overhead,
            UnassignedFuel = unassignedFuel
        };
    }
}
