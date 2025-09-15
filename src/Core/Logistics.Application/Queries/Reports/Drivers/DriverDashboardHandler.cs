using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal class DriverStats
{
    public Guid DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public int LoadsDelivered { get; set; }
    public double DistanceDriven { get; set; }
    public decimal GrossEarnings { get; set; }
}

internal sealed class DriverDashboardHandler(ITenantUnitOfWork tenantUow) : IAppRequestHandler<DriverDashboardQuery, Result<DriverDashboardDto>>
{
    public async Task<Result<DriverDashboardDto>> Handle(DriverDashboardQuery req, CancellationToken ct)
    {
        var employees = tenantUow.Repository<Employee>().Query();
        var trucks = tenantUow.Repository<Truck>().Query();
        var loads = tenantUow.Repository<Load>().Query();

        if (req.StartDate != default)
        {
            var from = DateTime.SpecifyKind(req.StartDate, DateTimeKind.Utc);
            loads = loads.Where(l => l.CreatedAt >= from);
        }
        if (req.EndDate != default)
        {
            var to = DateTime.SpecifyKind(req.EndDate, DateTimeKind.Utc);
            loads = loads.Where(l => l.CreatedAt <= to);
        }

        var mainDriverStats = trucks
            .Where(t => t.MainDriver != null)
            .Select(t => new
            {
                DriverId = t.MainDriver!.Id,
                DriverName = t.MainDriver!.FirstName + " " + t.MainDriver!.LastName,
                TruckId = t.Id
            });

        var secondaryDriverStats = trucks
            .Where(t => t.SecondaryDriver != null)
            .Select(t => new
            {
                DriverId = t.SecondaryDriver!.Id,
                DriverName = t.SecondaryDriver!.FirstName + " " + t.SecondaryDriver!.LastName,
                TruckId = t.Id
            });

        var combinedDriverStats = mainDriverStats.Union(secondaryDriverStats);

        var driverStats = combinedDriverStats
            .Select(x => new DriverStats
            {
                DriverId = x.DriverId,
                DriverName = x.DriverName,
                LoadsDelivered = loads.Count(l =>
                    l.AssignedTruckId == x.TruckId &&
                    l.Status == LoadStatus.Delivered),
                DistanceDriven = loads
                    .Where(l =>
                        l.AssignedTruckId == x.TruckId &&
                        l.Status == LoadStatus.Delivered)
                    .Select(l => l.Distance)
                    .Sum(),
                GrossEarnings = loads
                    .Where(l =>
                        l.AssignedTruckId == x.TruckId &&
                        l.Status == LoadStatus.Delivered)
                    .Select(l => l.DeliveryCost.Amount)
                    .Sum()
            })
            .ToList();

        var totalDrivers = driverStats.Count;
        var activeDrivers = driverStats.Count(d => d.LoadsDelivered > 0);
        var totalEarnings = driverStats.Sum(d => d.GrossEarnings);
        var totalDistance = driverStats.Sum(d => d.DistanceDriven);
        var totalLoadsDelivered = driverStats.Sum(d => d.LoadsDelivered);

        var topPerformers = driverStats
            .OrderByDescending(d => d.GrossEarnings)
            .Take(5)
            .Select(d => new DriverPerformanceDto
            {
                DriverName = d.DriverName,
                LoadsDelivered = d.LoadsDelivered,
                Earnings = d.GrossEarnings,
                Distance = d.DistanceDriven,
                Efficiency = d.LoadsDelivered > 0 ? d.LoadsDelivered / 30.0 : 0
            })
            .ToList();

        var driverTrends = CalculateDriverTrends(loads, trucks, req.StartDate, req.EndDate);
        var efficiencyMetrics = CalculateEfficiencyMetrics(driverStats, totalDrivers, totalEarnings, totalDistance);

        var dto = new DriverDashboardDto
        {
            TotalDrivers = totalDrivers,
            ActiveDrivers = activeDrivers,
            TotalEarnings = totalEarnings,
            TotalDistance = totalDistance,
            TotalLoadsDelivered = totalLoadsDelivered,
            AverageEarningsPerDriver = totalDrivers > 0 ? totalEarnings / totalDrivers : 0,
            AverageDistancePerDriver = totalDrivers > 0 ? totalDistance / totalDrivers : 0,
            TopPerformers = topPerformers,
            DriverTrends = driverTrends,
            EfficiencyMetrics = efficiencyMetrics
        };

        return Result<DriverDashboardDto>.Ok(dto);
    }

    private List<DriverTrendDto> CalculateDriverTrends(IQueryable<Load> loads, IQueryable<Truck> trucks, DateTime startDate, DateTime endDate)
    {
        var trends = new List<DriverTrendDto>();
        var currentDate = startDate == default ? DateTime.UtcNow.AddMonths(-6) : DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        var end = endDate == default ? DateTime.UtcNow : DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

        while (currentDate <= end)
        {
            var monthStart = DateTime.SpecifyKind(new DateTime(currentDate.Year, currentDate.Month, 1), DateTimeKind.Utc);
            var monthEnd = DateTime.SpecifyKind(monthStart.AddMonths(1).AddDays(-1), DateTimeKind.Utc);

            var monthLoads = loads
                .Where(l => l.CreatedAt >= monthStart && l.CreatedAt <= monthEnd)
                .ToList();

            var activeDriversCount = trucks
                .Where(t => t.MainDriver != null || t.SecondaryDriver != null)
                .Count();

            trends.Add(new DriverTrendDto
            {
                Period = monthStart.ToString("MMM yyyy"),
                ActiveDrivers = activeDriversCount,
                LoadsDelivered = monthLoads.Count(l => l.Status == LoadStatus.Delivered),
                TotalEarnings = monthLoads.Where(l => l.Status == LoadStatus.Delivered).Sum(l => l.DeliveryCost.Amount),
                TotalDistance = monthLoads.Where(l => l.Status == LoadStatus.Delivered).Sum(l => l.Distance)
            });

            currentDate = currentDate.AddMonths(1);
        }

        return trends;
    }

    private List<DriverEfficiencyDto> CalculateEfficiencyMetrics(List<DriverStats> driverStats, int totalDrivers, decimal totalEarnings, double totalDistance)
    {
        var avgEarningsPerDriver = totalDrivers > 0 ? (double)(totalEarnings / totalDrivers) : 0;
        var avgDistancePerDriver = totalDrivers > 0 ? totalDistance / totalDrivers : 0;
        var avgLoadsPerDriver = totalDrivers > 0 ? driverStats.Sum(d => (double)d.LoadsDelivered) / totalDrivers : 0;

        return new List<DriverEfficiencyDto>
        {
            new() { Metric = "Avg Earnings/Driver", Value = avgEarningsPerDriver, Unit = "$", Trend = 8.5 },
            new() { Metric = "Avg Distance/Driver", Value = avgDistancePerDriver, Unit = "km", Trend = 4.2 },
            new() { Metric = "Avg Loads/Driver", Value = avgLoadsPerDriver, Unit = "loads", Trend = 6.1 },
            new() { Metric = "Driver Utilization", Value = (double)driverStats.Count(d => d.LoadsDelivered > 0) / Math.Max(totalDrivers, 1) * 100, Unit = "%", Trend = 2.3 }
        };
    }
}
