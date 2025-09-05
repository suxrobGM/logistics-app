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

        var dto = new LoadsReportDto
        {
            TotalDistance = totalDistance,
            StatusBreakdown = statusBreakdown,
            TotalRevenue = totalRevenue,
            TotalLoads = totalLoads,
            TypeBreakdown = typeBreakdown,
            CancelledLoadsRevenue = cancelledLoadsRevenue,
            CancellationRate = cancellationRate
        };

        return Result<LoadsReportDto>.Ok(dto);
    }
}

