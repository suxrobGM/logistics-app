using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class DriversReportHandler(ITenantUnitOfWork tenantUow) : IAppRequestHandler<DriversReportQuery, PagedResult<DriverReportDto>>
{
    public async Task<PagedResult<DriverReportDto>> Handle(DriversReportQuery req, CancellationToken ct)
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
            .Select(x => new DriverReportDto
            {
                DriverId = x.DriverId,
                DriverName = x.DriverName,
                LoadsDelivered = loads.Count(l =>
                    l.AssignedTruckId == x.TruckId &&
                    l.Status == Domain.Primitives.Enums.LoadStatus.Delivered),
                DistanceDriven = loads
                    .Where(l =>
                        l.AssignedTruckId == x.TruckId &&
                        l.Status == Domain.Primitives.Enums.LoadStatus.Delivered)
                    .Select(l => l.Distance)
                    .Sum(),
                GrossEarnings = loads
                    .Where(l =>
                        l.AssignedTruckId == x.TruckId &&
                        l.Status == Domain.Primitives.Enums.LoadStatus.Delivered)
                    .Select(l => l.DeliveryCost.Amount)
                    .Sum(),
                TruckNumber = trucks.FirstOrDefault(t => t.Id == x.TruckId)!.Number,
                IsMainDriver = trucks.FirstOrDefault(t => t.Id == x.TruckId && t.MainDriverId == x.DriverId) != null
            })
            .Select(d => new DriverReportDto
            {
                DriverId = d.DriverId,
                DriverName = d.DriverName,
                LoadsDelivered = d.LoadsDelivered,
                DistanceDriven = d.DistanceDriven,
                GrossEarnings = d.GrossEarnings,
                TruckNumber = d.TruckNumber,
                IsMainDriver = d.IsMainDriver,
                AverageDistancePerLoad = d.LoadsDelivered > 0 ? d.DistanceDriven / d.LoadsDelivered : 0,
                AverageEarningsPerLoad = d.LoadsDelivered > 0 ? d.GrossEarnings / d.LoadsDelivered : 0,
                Efficiency = d.LoadsDelivered > 0 ? d.LoadsDelivered / 30.0 : 0 // Assuming 30 days period
            });

        if (!string.IsNullOrWhiteSpace(req.Search))
        {
            var term = req.Search.ToLower();
            driverStats = driverStats.Where(d => d.DriverName.ToLower().Contains(term));
        }

        var totalCount = driverStats.Count();

        var items = driverStats.OrderBy(req.OrderBy)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToList();


        return PagedResult<DriverReportDto>.Succeed(items, totalCount, req.PageSize);
    }
}

