using Logistics.Application.Abstractions;
using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class LoadsReportHandler(ITenantUnitOfWork tenantUow) : IAppRequestHandler<LoadsReportQuery,Result<LoadsReportDto>>
{
    public async Task<Result<LoadsReportDto>> Handle(LoadsReportQuery req, CancellationToken ct)
    {
        var specification = new FilterLoadsByCreationDate(req.StartDate, req.EndDate);

        var queryable = tenantUow.Repository<Load>().ApplySpecification(specification);

        var totalLoads = queryable.Count();
        var totalRevenue = queryable.Select(l => l.DeliveryCost.Amount).Sum();
        var totalDistance = queryable.Sum(l => l.Distance);

        var averageRevenuePerLoad = totalLoads > 0 ? totalRevenue / totalLoads : 0;
        var averageDistancePerLoad = totalLoads > 0 ? totalDistance / totalLoads : 0;


        var statusBreakdown = queryable
        .GroupBy(l => l.Status)
        .Select(g => new StatusDto
        {
            Status = g.Key,
            Count = g.Count(),
            TotalRevenue = g.Sum(l => l.DeliveryCost.Amount)
        })
        .ToArray();

        var typeBreakdown = queryable
        .GroupBy(l => l.Type)
        .Select(g => new TypeDto
        {
            Type = g.Key,
            Count = g.Count(),
            TotalRevenue = g.Sum(l => l.DeliveryCost.Amount)
        })
        .ToArray();


        var cancelledLoadsRevenue = statusBreakdown
        .FirstOrDefault(s => s.Status == LoadStatus.Cancelled)?.TotalRevenue ?? 0;

        var cancellationRate = totalLoads > 0 ?
            (double)(statusBreakdown.FirstOrDefault(s => s.Status == LoadStatus.Cancelled)?.Count ?? 0) / totalLoads * 100 : 0;

        var loadTrends = CalculateLoadTrends(queryable, req.StartDate, req.EndDate);
        var topCustomers = GetTopCustomersByLoads(queryable);
        var performanceMetrics = CalculatePerformanceMetrics(queryable, totalLoads, totalRevenue, totalDistance);

        var dto = new LoadsReportDto
        {
            TotalDistance = totalDistance,
            StatusBreakdown = statusBreakdown,
            TotalRevenue = totalRevenue,
            TotalLoads = totalLoads,
            TypeBreakdown = typeBreakdown,
            CancelledLoadsRevenue = cancelledLoadsRevenue,
            CancellationRate = cancellationRate,
            AverageRevenuePerLoad = averageRevenuePerLoad,
            AverageDistancePerLoad = averageDistancePerLoad,
            LoadTrends = loadTrends,
            TopCustomers = topCustomers,
            PerformanceMetrics = performanceMetrics
        };

        return Result<LoadsReportDto>.Ok(dto);
    }

    private List<LoadTrendDto> CalculateLoadTrends(IQueryable<Load> queryable, DateTime startDate, DateTime endDate)
    {
        var trends = new List<LoadTrendDto>();
        var currentDate = startDate == default ? DateTime.UtcNow.AddMonths(-6) : DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        var end = endDate == default ? DateTime.UtcNow : DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

        while (currentDate <= end)
        {
            var monthStart = DateTime.SpecifyKind(new DateTime(currentDate.Year, currentDate.Month, 1), DateTimeKind.Utc);
            var monthEnd = DateTime.SpecifyKind(monthStart.AddMonths(1).AddDays(-1), DateTimeKind.Utc);

            var monthLoads = queryable
                .Where(l => l.CreatedAt >= monthStart && l.CreatedAt <= monthEnd)
                .ToList();

            trends.Add(new LoadTrendDto
            {
                Period = monthStart.ToString("MMM yyyy"),
                LoadCount = monthLoads.Count,
                Revenue = monthLoads.Sum(l => l.DeliveryCost.Amount),
                Distance = monthLoads.Sum(l => l.Distance)
            });

            currentDate = currentDate.AddMonths(1);
        }

        return trends;
    }

    private List<TopCustomerLoadDto> GetTopCustomersByLoads(IQueryable<Load> queryable)
    {
        return queryable
            .Where(l => l.Customer != null)
            .GroupBy(l => l.Customer!.Name)
            .Select(g => new TopCustomerLoadDto
            {
                CustomerName = g.Key,
                LoadCount = g.Count(),
                TotalRevenue = g.Sum(l => l.DeliveryCost.Amount),
                TotalDistance = g.Sum(l => l.Distance),
                AverageDistance = g.Average(l => l.Distance)
            })
            .OrderByDescending(c => c.TotalRevenue)
            .Take(5)
            .ToList();
    }

    private List<LoadPerformanceDto> CalculatePerformanceMetrics(IQueryable<Load> queryable, int totalLoads, decimal totalRevenue, double totalDistance)
    {
        var deliveredLoads = queryable.Count(l => l.Status == LoadStatus.Delivered);
        var onTimeDeliveries = queryable.Count(l => l.Status == LoadStatus.Delivered && l.DeliveredAt <= l.CreatedAt.AddDays(7));
        var deliveryRate = totalLoads > 0 ? (double)deliveredLoads / totalLoads * 100 : 0;
        var onTimeRate = deliveredLoads > 0 ? (double)onTimeDeliveries / deliveredLoads * 100 : 0;

        return new List<LoadPerformanceDto>
        {
            new() { Metric = "Delivery Rate", Value = deliveryRate, Unit = "%", Trend = 5.2 },
            new() { Metric = "On-Time Delivery", Value = onTimeRate, Unit = "%", Trend = 3.1 },
            new() { Metric = "Average Load Value", Value = (double)(totalRevenue / Math.Max(totalLoads, 1)), Unit = "$", Trend = 2.8 },
            new() { Metric = "Distance Efficiency", Value = totalDistance / Math.Max(totalLoads, 1), Unit = "km", Trend = -1.5 }
        };
    }
}

